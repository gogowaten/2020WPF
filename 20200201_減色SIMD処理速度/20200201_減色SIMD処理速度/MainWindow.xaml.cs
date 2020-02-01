using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;
using System.Diagnostics;


namespace _20200201_減色SIMD処理速度
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] OriginPixels;
        BitmapSource OriginBitmapSource;
        string ImageFileFullPath;

        public MainWindow()
        {
            InitializeComponent();

            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;

            string path = @"E:\オレ\携帯\2019スマホ\WP_20200125_12_31_30_Pro.jpg";
            path = @"D:\ブログ用\テスト用画像\1ピクセルだけ半透明_.png";
            (OriginPixels, OriginBitmapSource) = MakeBitmapSourceAndPixelData(path, PixelFormats.Bgra32, 96, 96);
            MyImageOrigin.Source = OriginBitmapSource;
            MyImage.Source = OriginBitmapSource;
            ImageFileFullPath = path;
        }

            //cubeを作成、色取得
        private List<Color> GetColors(byte[] pixels)
        {
            Cube cube = new Cube(OriginPixels);
            cube.Split(16);
            return cube.GetColors();
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            (OriginPixels, OriginBitmapSource) = MakeBitmapSourceAndPixelData(filePath[0], PixelFormats.Bgra32, 96, 96);

            if (OriginBitmapSource == null)
            {
                MessageBox.Show("画像として開くことができなかった");
            }
            else
            {
                MyImageOrigin.Source = OriginBitmapSource;
                MyImage.Source = OriginBitmapSource;
                ImageFileFullPath = filePath[0];
            }
        }

        private BitmapSource ZmT2SIMD減色9(BitmapSource source, byte[] pixels, List<byte[]> palette)
        {
            byte[] zPixels = new byte[pixels.Length];
            int MyThreads = Environment.ProcessorCount;
            int windowSize = pixels.Length / MyThreads;//1スレッドに割り当てる量

            Task.WhenAll(Enumerable.Range(0, MyThreads)
                .Select(n => Task.Run(() =>
                {
                    for (int p = n * windowSize; p < (n + 1) * windowSize; p += 4)
                    {
                        var r = pixels[p + 2];
                        var g = pixels[p + 1];
                        var b = pixels[p];
                        double min = ColorDistanceV3(r, g, b, palette[0]);
                        int pIndex = 0;
                        for (int i = 1; i < palette.Count; i++)
                        {
                            var distance = ColorDistanceV3(r, g, b, palette[i]);
                            if (min > distance)
                            {
                                min = distance;
                                pIndex = i;
                            }
                        }

                        zPixels[p] = palette[pIndex][2];
                        zPixels[p + 1] = palette[pIndex][1];
                        zPixels[p + 2] = palette[pIndex][0];
                        zPixels[p + 3] = 255;
                    }
                }))).Wait();

            return BitmapSource.Create(source.PixelWidth, source.PixelHeight, 96, 96, source.Format, null, zPixels, source.PixelWidth * 4);
        }
        //色の距離、SIMDを使うVectorクラスで計算
        private float ColorDistanceV3(byte r1, byte g1, byte b1, byte[] palette)
        {
            Vector3 value1 = new Vector3(r1, g1, b1);
            Vector3 value2 = new Vector3(palette[0], palette[1], palette[2]);
            return Vector3.Distance(value1, value2);
        }
        //private float ColorDistanceV3(byte[] RGBA, byte[] palette)
        //{
        //    Vector3 value1 = new Vector3(RGBA[0], RGBA[1], RGBA[2]);
        //    Vector3 value2 = new Vector3(palette[0], palette[1], palette[2]);
        //    return Vector3.Distance(value1, value2);
        //}


        //処理時間計測
        //private void MyTime(Func<BitmapSource, byte[], List<byte[]>, BitmapSource> func, TextBlock textBlock)
        //{
        //    if (OriginBitmapSource == null) return;
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    //MyImage.Source = func(OriginBitmapSource, OriginPixels, MakePalette());
        //    MyImage.Source = func(OriginBitmapSource, OriginPixels, MakePalette(16));
        //    sw.Stop();
        //    textBlock.Text = $"{sw.Elapsed.Seconds}秒{sw.Elapsed.Milliseconds.ToString("000")}";
        //}




        /// <summary>
        /// 画像ファイルからbitmapと、そのbyte配列を返す、ピクセルフォーマットは指定したものに変換
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsを指定、null指定ならBgra32で作成する</param>
        /// <param name="dpiX">96が基本、指定なしなら元画像と同じにする</param>
        /// <param name="dpiY">96が基本、指定なしなら元画像と同じにする</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource source) MakeBitmapSourceAndPixelData(
            string filePath,
            PixelFormat pixelFormat,
            double dpiX = 0, double dpiY = 0)
        {
            byte[] pixels = null;//PixelData
            BitmapSource source = null;
            if (pixelFormat == null) { pixelFormat = PixelFormats.Bgra32; }
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var frame = BitmapFrame.Create(fs);
                    var tempBitmap = new FormatConvertedBitmap(frame, pixelFormat, null, 0);
                    int w = tempBitmap.PixelWidth;
                    int h = tempBitmap.PixelHeight;
                    int stride = (w * pixelFormat.BitsPerPixel + 7) / 8;
                    pixels = new byte[h * stride];
                    tempBitmap.CopyPixels(pixels, stride, 0);
                    //dpi指定がなければ元の画像と同じdpiにする
                    if (dpiX == 0) { dpiX = frame.DpiX; }
                    if (dpiY == 0) { dpiY = frame.DpiY; }
                    //dpiを指定してBitmapSource作成
                    source = BitmapSource.Create(
                        w, h, dpiX, dpiY,
                        tempBitmap.Format,
                        tempBitmap.Palette, pixels, stride);
                };
            }
            catch (Exception)
            {
            }
            return (pixels, source);
        }

        private void MyGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Panel.SetZIndex(MyImageOrigin, 1);
        }

        private void MyGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Panel.SetZIndex(MyImageOrigin, -1);
        }
        private void ButtonSaveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveImage((BitmapSource)MyImage.Source);
        }

        private void SaveImage(BitmapSource source)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "*.png|*.png|*.bmp|*.bmp|*.tiff|*.tiff";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(ImageFileFullPath) + "_";
            saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(ImageFileFullPath);
            if (saveFileDialog.ShowDialog() == true)
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                if (saveFileDialog.FilterIndex == 1)
                {
                    encoder = new PngBitmapEncoder();
                }
                else if (saveFileDialog.FilterIndex == 2)
                {
                    encoder = new BmpBitmapEncoder();
                }
                else if (saveFileDialog.FilterIndex == 3)
                {
                    encoder = new TiffBitmapEncoder();
                }
                encoder.Frames.Add(BitmapFrame.Create(source));

                using (var fs = new System.IO.FileStream(saveFileDialog.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    encoder.Save(fs);
                }
            }
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            var neko = GetColors(OriginPixels);
        }
    }




}
