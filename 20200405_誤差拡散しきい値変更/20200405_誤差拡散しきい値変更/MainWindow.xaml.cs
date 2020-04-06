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

//誤差拡散法の高画質化技術
//https://www.jstage.jst.go.jp/article/photogrst1964/60/6/60_6_353/_pdf/-char/ja
//可変しきい値？

namespace _20200405_誤差拡散しきい値変更
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource MyBitmapSource;
        byte[] MyPixels;
        byte[] BinaryPixels;
        byte[] BinaryPixels2;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();

            MyInitialize();
            //this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //using (var stream = assembly.GetManifestResourceStream("_20200401_誤差拡散2値.grayScale.bmp"))
            //{
            //    if (stream != null)
            //    {
            //        MyImage.Source = BitmapFrame.Create(stream);
            //    }
            //}

            using (var stream = assembly.GetManifestResourceStream("_20200401_誤差拡散2値.新しいフォルダー.grayscale256x256.png"))
            {
                if (stream != null)
                {
                    MyImage.Source = BitmapFrame.Create(stream);
                }
            }

        }

        private void MyInitialize()
        {
            //画像をクリックした時
            MyGrid.MouseLeftButtonDown += (s, e) => Panel.SetZIndex(MyImageOrigin, 1);
            MyGrid.MouseLeftButtonUp += (s, e) => Panel.SetZIndex(MyImageOrigin, -1);

            //ファイルをドロップ
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;

            string imagePath;
            imagePath = @"D:\ブログ用\チェック用2\WP_20200328_11_22_52_Pro_2020_03_28_午後わてん.jpg";
            imagePath = @"E:\オレ\携帯\2019スマホ\WP_20200328_11_22_52_Pro.jpg";
            imagePath = @"D:\ブログ用\テスト用画像\grayScale.bmp";
            imagePath = @"D:\ブログ用\テスト用画像\grayscale256x256.png";
            imagePath = @"D:\ブログ用\テスト用画像\grayScaleHorizontal255to0.bmp";
            //imagePath = @"D:\ブログ用\テスト用画像\Michelangelo's_David_-_63_grijswaarden.bmp";
            //imagePath = @"D:\ブログ用\テスト用画像\gray128.png";//0と255の中間みたい、pixelformats.blackwhiteだと市松模様になる
            //imagePath = @"D:\ブログ用\テスト用画像\gray127.png";//これは中間じゃないみたい
            //imagePath = @"D:\ブログ用\テスト用画像\gray250.png";
            //imagePath = @"D:\ブログ用\テスト用画像\HSVRectH90.png";
            //imagePath = @"D:\ブログ用\テスト用画像\ﾈｺは見ている.png";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_2097_.jpg";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん_256x192.png";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_0541_2017_07_21_午後わてん_.jpg";
            //imagePath = @"D:\ブログ用\テスト用画像\Gray240and90_255x255.bmp";
            //ローカルの画像をセット
            (MyPixels, MyBitmapSource) = MakeBitmapSourceAndPixelData(imagePath, PixelFormats.Gray8, 96, 96);


            //アプリに埋め込んだ画像をセット
            //SetResourceImage();

            //画像を表示
            MyImage.Source = MyBitmapSource;
            MyImageOrigin.Source = MyBitmapSource;

        }

        //画像ファイルをドロップしたとき
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            (MyPixels, MyBitmapSource) = MakeBitmapSourceAndPixelData(filePath[0], PixelFormats.Gray8, 96, 96);

            if (MyBitmapSource == null)
            {
                MessageBox.Show("画像として開くことができなかった");
            }
            else
            {
                //MyImageOrigin.Source = OriginBitmapSource;
                MyImage.Source = MyBitmapSource;
                MyImageOrigin.Source = MyBitmapSource;
            }
        }

       
        /// <summary>
        /// 誤差拡散、FloydSteinberg、PixelFormat.Gray8グレースケール画像専用
        /// </summary>
        /// <param name="source">元画像のピクセルの輝度値</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride">横1行分のbyte数</param>
        /// <returns></returns>
        private BitmapSource D1_FloydSteinberg(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];//変換先画像用
            double[] gosaPixels = new double[count];//誤差計算用
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                if (y % 2 == 0)
                {
                    //偶数行は右進行
                    //->->->
                    //  * 7
                    //3 5 1
                    //￣16￣
                    for (int x = 0; x < width; x++)
                    {
                        //注目ピクセルのインデックス
                        p = yp + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        SetBlackOrWhite(gosaPixels[p], pixels, p);
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
                    //奇数行は左進行
                    //<-<-<-
                    //7 *
                    //1 5 3
                    //￣16￣
                    for (int x = width - 1; x >= 0; x--)
                    {
                        //注目ピクセルのインデックス
                        p = yp + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        SetBlackOrWhite(gosaPixels[p], pixels, p);
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
                            if (x != width - 1)
                                //右下
                                gosaPixels[p + 1] += gosa * 3;
                            if (x != 0)
                                //左下
                                gosaPixels[p - 1] += gosa * 1;
                        }
                    }
                }
            }
            BinaryPixels = pixels;
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        /// <summary>
        /// しきい値127.5で 0 or 255、127.5未満は0、127.5以上は255に置き換える
        /// 127.49999999999999999...は0、127.5以上は255だから、整数の場合は127.5を四捨五入した128未満が0、それ以外は255にすればいい？
        /// </summary>
        /// <param name="value">対象の値</param>
        /// <param name="pixels">0 or 255を入れる配列</param>
        /// <param name="p">配列のIndex</param>
        private void SetBlackOrWhite(double value, byte[] pixels, int p)
        {
            if (127.5 > value)
                pixels[p] = 0;
            else
                pixels[p] = 255;
        }

        private BitmapSource D1_FloydSteinberg2(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];//変換先画像用
            double[] gosaPixels = new double[count];//誤差計算用
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                if (y % 2 == 0)
                {
                    //偶数行は右進行
                    //->->->
                    //  * 7
                    //3 5 1
                    //￣16￣
                    for (int x = 0; x < width; x++)
                    {
                        //注目ピクセルのインデックス
                        p = yp + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        SetBlackOrWhite2(gosaPixels[p], pixels, p);
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
                    //奇数行は左進行
                    //<-<-<-
                    //7 *
                    //1 5 3
                    //￣16￣
                    for (int x = width - 1; x >= 0; x--)
                    {
                        //注目ピクセルのインデックス
                        p = yp + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        SetBlackOrWhite2(gosaPixels[p], pixels, p);
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
                            if (x != width - 1)
                                //右下
                                gosaPixels[p + 1] += gosa * 3;
                            if (x != 0)
                                //左下
                                gosaPixels[p - 1] += gosa * 1;
                        }
                    }
                }
            }
            BinaryPixels2 = pixels;
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //可変しきい値？これであっているのかわからん
        private void SetBlackOrWhite2(double value, byte[] pixels, int p)
        {
            //var t = (value * 13 + 128) / 14;//これだと大きすぎるらしい
            //var t = (value * 7 + 128) / 8;//これくらいから
            var t = (value * 3 + 128) / 4;//これくらいまでがいいらしい

            if (127.5 > t)
                pixels[p] = 0;
            else
                pixels[p] = 255;
        }


        //全く変化ない、理解できていないから書き方間違っていると思う
        //IIEEJ 43(1): 91-107 (2014)
        //https://www.jstage.jst.go.jp/article/iieej/43/1/43_91/_pdf
        private BitmapSource D1_FloydSteinberg3(byte[] source, int width, int height, int stride, double m)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];//変換先画像用
            double[] gosaPixels = new double[count];//誤差計算用
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                if (y % 2 == 0)
                {
                    //偶数行は右進行
                    //->->->
                    //  * 7
                    //3 5 1
                    //￣16￣
                    for (int x = 0; x < width; x++)
                    {
                        //注目ピクセルのインデックス
                        p = yp + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        SetBlackOrWhite3(gosaPixels[p], pixels, p, m);
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
                    //奇数行は左進行
                    //<-<-<-
                    //7 *
                    //1 5 3
                    //￣16￣
                    for (int x = width - 1; x >= 0; x--)
                    {
                        //注目ピクセルのインデックス
                        p = yp + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        SetBlackOrWhite3(gosaPixels[p], pixels, p, m);
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
                            if (x != width - 1)
                                //右下
                                gosaPixels[p + 1] += gosa * 3;
                            if (x != 0)
                                //左下
                                gosaPixels[p - 1] += gosa * 1;
                        }
                    }
                }
            }
            BinaryPixels2 = pixels;
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }
        private void SetBlackOrWhite3(double value, byte[] pixels, int p, double m)
        {
            m = 0.8;
            double threshold = m * (value - 127.5) + 127.5;            
            if (value < threshold)
                pixels[p] = 0;
            else
                pixels[p] = 255;
        }











        //






















        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D1_FloydSteinberg(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button1.Content = nameof(D1_FloydSteinberg);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D1_FloydSteinberg2(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button2.Content = nameof(D1_FloydSteinberg2);
            //MyImage.Source = D2_JaJuNi(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            //Button2.Content = nameof(D2_JaJuNi);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D1_FloydSteinberg3(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth,0.8);
            Button3.Content = nameof(D1_FloydSteinberg3);
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

        #endregion

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Clipboard.SetImage((BitmapSource)MyImage.Source);
        }

        private void ButtonBlackWhite_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            MyImage.Source = new FormatConvertedBitmap(MyBitmapSource, PixelFormats.BlackWhite, null, 0);
        }


        private void ButtonHikaku_Click(object sender, RoutedEventArgs e)
        {
            int b = 0;
            int w = 0;
            for (int i = 0; i < BinaryPixels.Length; i++)
            {
                if (BinaryPixels[i] == 0) b--;
                else w--;
                if (BinaryPixels2[i] == 0) b++;
                else w++;
            }
            TextBlockHikaku.Text = $"black={b}, white={w}";
        }

       
    }
}
