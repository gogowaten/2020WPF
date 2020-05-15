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

namespace _20200515_ガンマ補正してから誤差拡散
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





            //MyInitialize();

            //            デバッグビルドでのみ特定のコードがコンパイルされるようにする - .NET Tips(VB.NET, C#...)
            //https://dobon.net/vb/dotnet/programing/define.html

#if DEBUG
            MyInitializeForDebug();
#endif

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

        //private void MyInitialize()
        //{
        //    //アプリに埋め込んだ画像を読み込んで、Gray8に変換して読み込む
        //    string path = "_20200507_ガンマ補正してから3色誤差拡散.grayScale.bmp";
        //    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        //    using (var stream = assembly.GetManifestResourceStream(path))
        //    {
        //        if (stream != null)
        //        {
        //            var source = BitmapFrame.Create(stream);
        //            MyBitmapSource = new FormatConvertedBitmap(source, PixelFormats.Gray8, null, 0);
        //            MyImage.Source = source;
        //            MyImageOrigin.Source = source;
        //        }
        //    }

        //}
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


        //private Dictionary<int, byte> MakeTable(int colorsCount)
        //{
        //    var table = new Dictionary<int, byte>();
        //    double stepColor = 1.0 / (colorsCount - 1);

        //    for (int i = 0; i < colorsCount; i++)
        //    {
        //        table.Add(i, (byte)((stepColor * i * 255) + 0.5));//四捨五入
        //        //table.Add(i, (byte)Math.Round(stepColor * i * 255, MidpointRounding.AwayFromZero));
        //    }
        //    table.Add(colorsCount, 255);
        //    return table;
        //}
        private byte[] MakePalette255(int colorsCount)
        {
            byte[] palette = new byte[colorsCount+1];
            double step = 1.0 / (colorsCount - 1);
            for (int i = 0; i < colorsCount; i++)
            {
                palette[i] = (byte)((step * i * 255) + 0.5);
            }
            palette[colorsCount] = 255;
            return palette;
        }

        private BitmapSource D1(BitmapSource source, int colorsCount)
        {   
            byte[] palette = MakePalette255(colorsCount);
            double stepRange = 1.0 / colorsCount;//1範囲値

            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用
            double[] gosaPixels = new double[height * stride];
            Array.Copy(pixels, gosaPixels, height * stride);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    //変換
                    double neko = gosaPixels[p] / (255 * stepRange);
                    int inu = (int)neko;//切り捨て、こっちが正しい感じ
                    //int inu = (int)(neko + 0.5);//四捨五入
                    pixels[p] = palette[inu];
                    //pixels[p] = convertTable[inu];

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //範囲ごとのガンマ値のリスト作成
        private double[] MakeLocalGamma(int colorsCount)
        {
            const double gamma = 2.2;
            int count = colorsCount - 1;
            double part = 1.0 / count;
            double[] vs = new double[colorsCount];
            vs[0] = gamma;

            double begin;//開始値
            double gCorrect;//補正後γ
            double gLength = gamma - 1.0;//=1.2、γが取る値の範囲、全体距離
            for (int i = 1; i < count + 1; i++)
            {
                begin = part * i;
                gCorrect = Math.Pow(begin, 1.0 / gamma);
                var gNew = (-gLength * gCorrect) + gamma;
                vs[i] = gNew;
            }
            return vs;
        }
        //ガンマ補正
        private double[] GammaCorrect1(byte[] pixels, int colorsCount)
        {
            //ガンマ補正結果用配列
            double[] correctPixels = new double[pixels.Length];
            //範囲ごとのガンマ値リスト取得
            double[] localGamma = MakeLocalGamma(colorsCount);
            //1範囲値
            double stepRange = 1.0 / (colorsCount - 1);
            //ガンマ補正
            for (int i = 0; i < pixels.Length; i++)
            {
                double v = pixels[i] / 255.0;//0～1に変換
                int index = (int)(v / stepRange);
                double begin = stepRange * index;//開始値
                double vCorrect = (v - begin) * (colorsCount - 1);
                vCorrect = Math.Pow(vCorrect, localGamma[index]);
                vCorrect /= colorsCount - 1;
                vCorrect += begin;
                vCorrect *= 255.0;
                correctPixels[i] = vCorrect;
            }
            return correctPixels;
        }
        //変換前に一括でガンマ補正、可変ガンマ値
        private BitmapSource D2(BitmapSource source, int colorsCount)
        {   
            byte[] palette = MakePalette255(colorsCount);
            double stepTone = 1.0 / colorsCount * 255;
            //double stepTone = 1.0 / colorsCount * 256;

            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            var gosaPixels = GammaCorrect1(pixels, colorsCount);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    //変換
                    double neko = gosaPixels[p] / stepTone;
                    int inu = (int)neko;
                    //int inu = (int)(neko + 0.5);//四捨五入
                    pixels[p] = palette[inu];

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }



      
        //パレット作成
        private double[] MakePalette(int colorsCount)
        {
            double[] palette = new double[colorsCount + 1];
            double step = 1.0 / (colorsCount - 1);//1階調値
            for (int i = 0; i < palette.Length; i++)
            {
                palette[i] = step * i;
            }
            palette[colorsCount] = 1.0;
            return palette;
        }
        //0～1に変換してから計算、ガンマ補正なし
        private BitmapSource D10(BitmapSource source, int colorsCount)
        {
            double[] palette = MakePalette(colorsCount);
            double step = 1.0 / colorsCount;//1範囲値

            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用            
            double[] gosaPixels = pixels.Select(x => x / 255.0).ToArray();//
            double[] normPixels = new double[pixels.Length];

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;

                    //変換
                    var neko = gosaPixels[p] / step;//切り捨てが正しい感じ
                    //int inu = (int)(neko+0.5);//四捨五入
                    int inu = (int)neko;//切り捨て
                    double uma = palette[inu];
                    normPixels[p] = uma;

                    //誤差拡散
                    gosa = (gosaPixels[p] - normPixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            //pixels = normPixels.Select(x => (byte)(x * 255)).ToArray();//切り捨ては違う
            pixels = normPixels.Select(x => (byte)((x * 255) + 0.5)).ToArray();//四捨五入がいい
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        //ガンマ値一覧作成
        private double[] MakeGammaTable1(int colorsCount)
        {
            //double v2 = 2.2 - 1.0;//1.2000000000002
            //decimal dc = 2.2m - 1.0m;//1.2

            double gamma = 1.0 / 2.2;// (double)gg;
            double step = 1.0 / (colorsCount - 1);
            double[] table = new double[colorsCount];
            for (int i = 0; i < table.Length; i++)
            {
                double corrected = (step * i);
                corrected = Math.Pow(corrected, gamma);
                decimal myGamma = (decimal)corrected * 1.2m;
                myGamma = 2.2m - myGamma;
                table[i] = (double)myGamma;
            }
            return table;
        }
        //0～1の配列にガンマ補正
        private void GammaCorrect1(double[] pixels, int colorsCount, double[] localGamma)
        {
            double stepGamma = 1.0 / (colorsCount - 1);//1ガンマ範囲値
            int splitCount = colorsCount - 1;//分割数
            for (int i = 0; i < pixels.Length; i++)
            {
                var neko = pixels[i];
                var inu = neko / stepGamma;
                int index = (int)inu;//小数点以下切り捨て
                double begin = index * stepGamma;//開始値
                double corrected = (neko - begin) * splitCount;
                corrected = Math.Pow(corrected, localGamma[index]);
                corrected /= splitCount;
                corrected += begin;
                pixels[i] = corrected;
            }

        }
        //0～1に変換してから計算、ガンマ補正あり
        private BitmapSource D11(BitmapSource source, int colorsCount)
        {
            double[] palette = MakePalette(colorsCount);
            double stepColor = 1.0 / colorsCount;//1範囲値

            var localGamma = MakeGammaTable1(colorsCount);


            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用            
            double[] gosaPixels = pixels.Select(x => x / 255.0).ToArray();
            //ガンマ補正
            GammaCorrect1(gosaPixels, colorsCount, MakeGammaTable1(colorsCount));

            double[] normPixels = new double[pixels.Length];

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;

                    //変換
                    if (y == 128) { var tako = 0; }
                    var neko = gosaPixels[p] / stepColor;
                    int inu = (int)neko;
                    double uma = palette[inu];
                    normPixels[p] = uma;
                    //誤差拡散
                    gosa = (gosaPixels[p] - normPixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            //pixels = normPixels.Select(x => (byte)(x * 255)).ToArray();
            pixels = normPixels.Select(x => (byte)((x * 255) + 0.5)).ToArray();//四捨五入
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        //ガンマ値一覧作成2
        private double[] MakeGammaTable2(int colorsCount)
        {
            //double v2 = 2.2 - 1.0;//1.2000000000002
            //decimal dc = 2.2m - 1.0m;//1.2

            double gamma = 1.0 / 2.2;// (double)gg;
            double step = 1.0 / (colorsCount - 1);
            double[] table = new double[colorsCount];
            for (int i = 0; i < table.Length; i++)
            {
                double corrected = (step * i);
                corrected = Math.Pow(corrected, gamma);
                corrected = Math.Pow(corrected, gamma);//ここが変更点
                decimal myGamma = (decimal)corrected * 1.2m;
                myGamma = 2.2m - myGamma;
                table[i] = (double)myGamma;
            }
            return table;
        }
        //0～1に変換してから計算、ガンマ補正あり2
        private BitmapSource D12(BitmapSource source, int colorsCount)
        {
            double[] palette = MakePalette(colorsCount);
            double stepColor = 1.0 / colorsCount;//1範囲値

            var localGamma = MakeGammaTable1(colorsCount);


            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用            
            double[] gosaPixels = pixels.Select(x => x / 255.0).ToArray();
            //ガンマ補正
            GammaCorrect1(gosaPixels, colorsCount, MakeGammaTable2(colorsCount));

            double[] normPixels = new double[pixels.Length];

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;

                    //変換
                    var neko = gosaPixels[p] / stepColor;
                    int inu = (int)neko;
                    double uma = palette[inu];
                    normPixels[p] = uma;
                    //誤差拡散
                    gosa = (gosaPixels[p] - normPixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            //0～1を0～255に戻すのは四捨五入がいい
            //pixels = normPixels.Select(x => (byte)(x * 255)).ToArray();
            pixels = normPixels.Select(x => (byte)((x * 255) + 0.5)).ToArray();//四捨五入
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
            Button1.Content = nameof(D1);
            MyImage.Source = D1(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button2.Content = nameof(D2);
            MyImage.Source = D2(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button3.Content = nameof(D10);
            MyImage.Source = D10(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button4.Content = nameof(D11);
            MyImage.Source = D11(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button5.Content = nameof(D12);
            MyImage.Source = D12(MyBitmapSource, (int)ScrollBarCount.Value);
        }


        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            //Button6.Content = nameof(D6);
            //MyImage.Source = D6(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            //Button7.Content = nameof(D7);
            //MyImage.Source = D7(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            //Button8.Content = nameof(D8);
            //MyImage.Source = D8(MyBitmapSource);
        }

        private void Button9_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            //Button9.Content = nameof(D9);
            //MyImage.Source = D9(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button10_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button10.Content = nameof(D10);
            MyImage.Source = D10(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button11_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button11.Content = nameof(D11);
            MyImage.Source = D11(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button12_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button12.Content = nameof(D12);
            MyImage.Source = D12(MyBitmapSource, (int)ScrollBarCount.Value);
        }
    }
}