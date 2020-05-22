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

namespace _20200518_255or256
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource MyBitmapSource;


        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
            MyGrid.MouseLeftButtonDown += (s, e) => Panel.SetZIndex(MyImageOrigin, 1);
            MyGrid.MouseLeftButtonUp += (s, e) => Panel.SetZIndex(MyImageOrigin, -1);




            //            デバッグビルドでのみ特定のコードがコンパイルされるようにする - .NET Tips(VB.NET, C#...)
            //https://dobon.net/vb/dotnet/programing/define.html

#if DEBUG
            MyInitializeForDebug();

            byte[] vs = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();
            var v1 = MyTest(256, vs);
            var v2 = MyTest2(256, vs);
            double[] vv = Enumerable.Range(0, 256).Select(x => x + 0.1).ToArray();
            var v3 = MyTest3(256, vv);
            var v4 = MyTest4(256, vv);

#endif

        }

        private byte[] MyTest(int colorCount, byte[] pixels)
        {
            double step = 255.0 / colorCount;
            byte[] result = new byte[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                int index = (int)(pixels[i] / step);
                int ir = (int)(index * step);
                result[i] = (byte)ir;
            }
            return result;
        }
        private byte[] MyTest2(int colorCount, byte[] pixels)
        {
            double step = 256.0 / colorCount;
            byte[] result = new byte[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                int index = (int)(pixels[i] / step);
                int ir = (int)(index * step);
                result[i] = (byte)ir;
            }
            return result;
        }
        private byte[] MyTest3(int colorCount, double[] pixels)
        {
            double step = 255.0 / colorCount;
            byte[] result = new byte[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                int index = (int)(pixels[i] / step);
                int ir = (int)(index * step);
                result[i] = (byte)ir;
            }
            return result;
        }
        private byte[] MyTest4(int colorCount, double[] pixels)
        {
            double step = 256.0 / colorCount;
            byte[] result = new byte[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                int index = (int)(pixels[i] / step);
                int ir = (int)(index * step);
                result[i] = (byte)ir;
            }
            return result;
        }


        //画像ファイルがドロップされた時
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            //Gray8に変換して読み込む
            (_, MyBitmapSource) = MakeBitmapSourceAndPixelData(filePath[0], PixelFormats.Gray8, 96, 96);
            if (MyBitmapSource == null)
            {
                MessageBox.Show("not Image");
            }
            else
            {
                MyImage.Source = MyBitmapSource;
                MyImageOrigin.Source = MyBitmapSource;
            }
        }


        private void MyInitializeForDebug()
        {

            string imagePath;
            imagePath = @"D:\ブログ用\チェック用2\WP_20200328_11_22_52_Pro_2020_03_28_午後わてん.jpg";
            //imagePath = @"E:\オレ\携帯\2019スマホ\WP_20200328_11_22_52_Pro.jpg";
            imagePath = @"D:\ブログ用\テスト用画像\grayScale.bmp";
            //imagePath = @"D:\ブログ用\テスト用画像\grayscale256x256.png";
            //imagePath = @"D:\ブログ用\テスト用画像\Michelangelo's_David_-_63_grijswaarden.bmp";
            //imagePath = @"D:\ブログ用\テスト用画像\lena256bw.png";
            //imagePath = @"D:\ブログ用\テスト用画像\gray128.png";//0と255の中間みたい、pixelformats.blackwhiteだと市松模様になる
            //imagePath = @"D:\ブログ用\テスト用画像\gray127.png";//これは中間じゃないみたい
            //imagePath = @"D:\ブログ用\テスト用画像\gray250.png";
            //imagePath = @"D:\ブログ用\テスト用画像\ﾈｺは見ている.png";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_2097_.jpg";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん_256x192.png";


            (_, MyBitmapSource) = MakeBitmapSourceAndPixelData(imagePath, PixelFormats.Gray8, 96, 96);
            MyImage.Source = MyBitmapSource;
            MyImageOrigin.Source = MyBitmapSource;

        }

        //パレット作成
        private byte[] MakePalette255(int colorsCount)
        {
            byte[] palette = new byte[colorsCount];
            double stepColor = 255.0 / (colorsCount - 1);//色範囲 = 255 / (色数 - 1)
            for (int i = 0; i < colorsCount; i++)
            {
                double c = stepColor * i;
                //四捨五入
                palette[i] = (byte)(c + 0.5);
                //palette[i] = (byte)Math.Round(c, MidpointRounding.AwayFromZero);
            }
            return palette;
        }


        private BitmapSource D1_div256(BitmapSource source, int colorsCount)
        {
            //一つの色が担当する区間範囲は256を色数で割る
            //もし256じゃなくて255を割るとズレが生じる
            //区範囲 = 256 / 色数
            double stepRange = 256.0 / colorsCount;
            //パレット作成
            var palette = MakePalette255(colorsCount);

            //画像から画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            int p;//座標を配列のインデックスに変換した値用

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    //変換
                    var v1 = pixels[p];//入力値
                    var v2 = v1 / stepRange;//入力値 / 区範囲
                    int index = (int)v2;//小数点以下切り捨て
                    var vv = palette[index];//パレットから色選択
                    pixels[p] = vv;//出力
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //グレースケール画像を0と255の2色に変換
        private BitmapSource D2Colors(BitmapSource source)
        {
            //画像から画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            int p;//座標を配列のインデックスに変換した値用

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    //変換は128未満なら0、以上なら255
                    //128ってのは0と255の256階調の中間
                    if (pixels[p] < 128)
                        pixels[p] = 0;
                    else
                        pixels[p] = 255;
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //グレースケール画像を0と128、255の3色に変換
        private BitmapSource D3Colors(BitmapSource source)
        {
            //画像から画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            int p;//座標を配列のインデックスに変換した値用
            double step = 256.0 / 3;//＝ 85.333333、1色あたりが担当する区間範囲
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    //変換
                    if (pixels[p] < step * 1)// 85.3333 未満なら 0
                        pixels[p] = 0;
                    else if (pixels[p] < step * 2)// < 170.6666 未満なら 128
                        pixels[p] = 128;//0と255の中間127.5を四捨五入
                    else
                        pixels[p] = 255;
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //グレースケール画像を0と85、170、255の4色に変換
        private BitmapSource D4Colors(BitmapSource source)
        {
            //画像から画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            int p;//座標を配列のインデックスに変換した値用
            double stepRange = 256.0 / 4;//＝ 64、1色あたりが担当する区間範囲
            double stepColor = 255.0 / (4 - 1);//= 85、1階調の差、色範囲
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    //変換
                    if (pixels[p] < stepRange * 1)// 64 未満なら 0
                        pixels[p] = (byte)(stepColor * 0);
                    else if (pixels[p] < stepRange * 2)// 128 未満なら 85
                        pixels[p] = (byte)(stepColor * 1);
                    else if (pixels[p] < stepRange * 3)// 192 未満なら 170
                        pixels[p] = (byte)(stepColor * 2);
                    else
                        pixels[p] = 255;
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }














        #region 画像読み込み
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
            if (pixelFormat == null) { pixelFormat = PixelFormats.Gray8; }
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

        #endregion




        #region クリップボード
        //クリップボードから画像貼り付け
        private void ButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            //8bitグレースケールに変換して読み込む
            BitmapSource bmp = Clipboard.GetImage();
            if (bmp == null)
            {
                MessageBox.Show("not Image");
                return;
            }
            bmp = new FormatConvertedBitmap(bmp, PixelFormats.Gray8, null, 0);
            MyBitmapSource = bmp;
            MyImage.Source = MyBitmapSource;
            MyImageOrigin.Source = MyBitmapSource;
        }

        //クリップボードへ画像貼り付け
        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Clipboard.SetImage((BitmapSource)MyImage.Source);
        }
        #endregion クリップボード


        private void ScrollBarCount_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ScrollBarCount.Value += ScrollBarCount.SmallChange;
            else ScrollBarCount.Value -= ScrollBarCount.SmallChange;
        }

        private void TextBlock_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ScrollBarCount.Value += ScrollBarCount.LargeChange;
            else ScrollBarCount.Value -= ScrollBarCount.LargeChange;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button1.Content = nameof(D1_div256);
            MyImage.Source = D1_div256(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button2.Content = nameof(D2Colors);
            MyImage.Source = D2Colors(MyBitmapSource);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button3.Content = nameof(D3Colors);
            MyImage.Source = D3Colors(MyBitmapSource);
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button4.Content = nameof(D4Colors);
            MyImage.Source = D4Colors(MyBitmapSource);
        }

   
    }
}
