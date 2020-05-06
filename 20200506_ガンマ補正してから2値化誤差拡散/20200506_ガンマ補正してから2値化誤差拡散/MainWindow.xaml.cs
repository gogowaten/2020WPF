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

//Libcacaスタディ-5.グレイスケールディザリング
//http://caca.zoy.org/study/part5.html

namespace _20200506_ガンマ補正してから2値化誤差拡散
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource MyBitmapSource;
        //private byte[] MyPixels;


        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
            MyGrid.MouseLeftButtonDown += (s, e) => Panel.SetZIndex(MyImageOrigin, 1);
            MyGrid.MouseLeftButtonUp += (s, e) => Panel.SetZIndex(MyImageOrigin, -1);

            MyInitialize();

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

        private void MyInitialize()
        {
            //アプリに埋め込んだ画像を読み込んで、Gray8に変換して読み込む
            string path = "_20200506_ガンマ補正してから2値化誤差拡散.grayScale.bmp";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(path))
            {
                if (stream != null)
                {
                    var source = BitmapFrame.Create(stream);
                    MyBitmapSource = new FormatConvertedBitmap(source, PixelFormats.Gray8, null, 0);
                    MyImage.Source = source;
                    MyImageOrigin.Source = source;
                }
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
            imagePath = @"D:\ブログ用\テスト用画像\lena256bw.png";
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

        /// <summary>
        /// ガンマ補正なし2値化、FloydSteinbergで誤差拡散、PixelFormat.Gray8グレースケール画像専用
        /// </summary>
        /// <param name="source">元画像のピクセルの輝度値</param>
        /// <returns></returns>
        private BitmapSource D1_Color2(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//変換先画像用
            source.CopyPixels(pixels, stride, 0);
            double[] gosaPixels = new double[height * stride];//誤差計算用
            Array.Copy(pixels, gosaPixels, height * stride);
            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            //  * 7
            //3 5 1
            //￣16￣
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //注目ピクセルのインデックス
                    p = y * stride + x;
                    //しきい値127.5未満なら0にする、それ以外は255にする
                    pixels[p] = (gosaPixels[p] < 127.5) ? (byte)0 : (byte)255;
                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        //右
                        gosaPixels[p + 1] += gosa * 7;
                    if (y < height - 1)
                    {
                        p += stride;
                        //下
                        gosaPixels[p] += gosa * 5;
                        if (x != 0)
                            //左下
                            gosaPixels[p - 1] += gosa * 3;
                        if (x != width - 1)
                            //右下
                            gosaPixels[p + 1] += gosa * 1;
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }



        /// <summary>
        /// 逆ガンマ補正した配列を返す
        /// </summary>
        /// <param name="pixels">グレースケール画像の輝度値の配列</param>
        /// <param name="gamma">2.2が標準</param>
        /// <returns></returns>
        private byte[] InvertGammaByte(byte[] pixels, double gamma)
        {
            byte[] vs = new byte[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                double d = pixels[i] / 255.0;
                //四捨五入しても変化なかった
                //vs[i] = (byte)Math.Round(Math.Pow(d, gamma) * 255.0, MidpointRounding.AwayFromZero);
                //四捨五入なし
                vs[i] = (byte)Math.Round(Math.Pow(d, gamma) * 255.0);
            }
            return vs;
        }

        /// <summary>
        /// ガンマ補正してから2値化
        /// </summary>
        /// <param name="source">元画像のピクセルの輝度値</param>
        //逆ガンマ補正した値を使って誤差拡散
        private BitmapSource D2_Color2GammaByte(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] temp = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(temp, stride, 0);

            //2.2で逆ガンマ補正したPixels(byte型)取得
            byte[] pixels = InvertGammaByte(temp, 2.2);

            double[] gosaPixels = new double[height * stride];//誤差計算用
            Array.Copy(pixels, gosaPixels, height * stride);
            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            //  * 7
            //3 5 1
            //￣16￣
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //注目ピクセルのインデックス
                    p = y * stride + x;
                    //しきい値127.5未満なら0にする、それ以外は255にする
                    pixels[p] = (gosaPixels[p] < 127.5) ? (byte)0 : (byte)255;
                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        //右
                        gosaPixels[p + 1] += gosa * 7;
                    if (y < height - 1)
                    {
                        p += stride;
                        //下
                        gosaPixels[p] += gosa * 5;
                        if (x != 0)
                            //左下
                            gosaPixels[p - 1] += gosa * 3;
                        if (x != width - 1)
                            //右下
                            gosaPixels[p + 1] += gosa * 1;
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //D2を蛇行走査にしただけ
        private BitmapSource D3_Color2GammaByte蛇行走査(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] temp = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(temp, stride, 0);

            byte[] pixels = InvertGammaByte(temp, 2.2);//逆ガンマ補正したPixels(byte型)
            double[] gosaPixels = new double[height * stride];//誤差計算用
            Array.Copy(pixels, gosaPixels, height * stride);
            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            //   * 7
            // 3 5 1
            //￣16￣
            for (int y = 0; y < height; y++)
            {
                if (y % 2 == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        //注目ピクセルのインデックス
                        p = y * stride + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        pixels[p] = (gosaPixels[p] < 127.5) ? (byte)0 : (byte)255;
                        //誤差拡散
                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != width - 1)
                            //右
                            gosaPixels[p + 1] += gosa * 7;
                        if (y < height - 1)
                        {
                            p += stride;
                            //下
                            gosaPixels[p] += gosa * 5;
                            if (x != 0)
                                //左下
                                gosaPixels[p - 1] += gosa * 3;
                            if (x != width - 1)
                                //右下
                                gosaPixels[p + 1] += gosa * 1;
                        }
                    }
                }
                //逆方向走査は拡散具合の左右を入れ替える
                // 7 *
                // 1 5 3
                //￣16￣
                else
                {
                    for (int x = width - 1; x >= 0; x--)
                    {
                        //注目ピクセルのインデックス
                        p = y * stride + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        pixels[p] = (gosaPixels[p] < 127.5) ? (byte)0 : (byte)255;
                        //誤差拡散
                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != 0)
                            //左
                            gosaPixels[p - 1] += gosa * 7;
                        if (y < height - 1)
                        {
                            p += stride;
                            //下
                            gosaPixels[p] += gosa * 5;
                            if (x != 0)
                                //左下
                                gosaPixels[p - 1] += gosa * 1;
                            if (x != width - 1)
                                //右下
                                gosaPixels[p + 1] += gosa * 3;
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }




        /// <summary>
        /// もとの値を逆側にガンマ補正して返す、double型
        /// </summary>
        /// <param name="pixels">もとの値の配列</param>
        /// <param name="gamma">ガンマ値</param>
        /// <returns></returns>
        private double[] InvertGammaDouble(byte[] pixels, double gamma)
        {
            double[] vs = new double[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                double d = pixels[i] / 255.0;
                vs[i] = Math.Pow(d, gamma) * 255.0;
            }
            return vs;
        }

        //D2の改変、double型で計算
        private BitmapSource D4_Color2GammaDouble(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];
            source.CopyPixels(pixels, stride, 0);

            //ここをdouble型にしただけ、それ以外はD2と同じ
            //逆ガンマ補正したPixelsを取得して誤差計算用にする
            double[] gosaPixels = InvertGammaDouble(pixels, 2.2);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            //  * 7
            //3 5 1
            //￣16￣

            //double型で計算
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //注目ピクセルのインデックス
                    p = y * stride + x;
                    //しきい値127.5未満なら0にする、それ以外は255にする
                    pixels[p] = (gosaPixels[p] < 127.5) ? (byte)0 : (byte)255;
                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        //右
                        gosaPixels[p + 1] += gosa * 7;
                    if (y < height - 1)
                    {
                        p += stride;
                        //下
                        gosaPixels[p] += gosa * 5;
                        if (x != 0)
                            //左下
                            gosaPixels[p - 1] += gosa * 3;
                        if (x != width - 1)
                            //右下
                            gosaPixels[p + 1] += gosa * 1;
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        //D4を蛇行走査にしただけ、これが一番正確できれいになるはず
        private BitmapSource D5_Color2GammaDouble蛇行走査(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//
            source.CopyPixels(pixels, stride, 0);

            //ここをdouble型にしただけ、それ以外はD2と同じ
            //逆ガンマ補正したPixelsを取得して誤差計算用にする
            double[] gosaPixels = InvertGammaDouble(pixels, 2.2);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            //  * 7
            //3 5 1
            //￣16￣
            for (int y = 0; y < height; y++)
            {
                if (y % 2 == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        //注目ピクセルのインデックス
                        p = y * stride + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        pixels[p] = (gosaPixels[p] < 127.5) ? (byte)0 : (byte)255;
                        //誤差拡散
                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != width - 1)
                            //右
                            gosaPixels[p + 1] += gosa * 7;
                        if (y < height - 1)
                        {
                            p += stride;
                            //下
                            gosaPixels[p] += gosa * 5;
                            if (x != 0)
                                //左下
                                gosaPixels[p - 1] += gosa * 3;
                            if (x != width - 1)
                                //右下
                                gosaPixels[p + 1] += gosa * 1;
                        }
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x--)
                    {
                        //注目ピクセルのインデックス
                        p = y * stride + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        pixels[p] = (gosaPixels[p] < 127.5) ? (byte)0 : (byte)255;
                        //誤差拡散
                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != 0)
                            //左
                            gosaPixels[p - 1] += gosa * 7;
                        if (y < height - 1)
                        {
                            p += stride;
                            //下
                            gosaPixels[p] += gosa * 5;
                            if (x != 0)
                                //左下
                                gosaPixels[p - 1] += gosa * 1;
                            if (x != width - 1)
                                //右下
                                gosaPixels[p + 1] += gosa * 3;
                        }
                    }
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


        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button1.Content = nameof(D1_Color2);
            MyImage.Source = D1_Color2(MyBitmapSource);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button2.Content = nameof(D2_Color2GammaByte);
            MyImage.Source = D2_Color2GammaByte(MyBitmapSource);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button3.Content = nameof(D3_Color2GammaByte蛇行走査);
            MyImage.Source = D3_Color2GammaByte蛇行走査(MyBitmapSource);
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button4.Content = nameof(D4_Color2GammaDouble);
            MyImage.Source = D4_Color2GammaDouble(MyBitmapSource);
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button5.Content = nameof(D5_Color2GammaDouble蛇行走査);
            MyImage.Source = D5_Color2GammaDouble蛇行走査(MyBitmapSource);
        }


    }
}
