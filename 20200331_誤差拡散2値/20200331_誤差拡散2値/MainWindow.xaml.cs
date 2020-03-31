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

namespace _20200331_誤差拡散2値
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource MyBitmapSource;
        byte[] MyPixels;
        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();

            MyInitialize();

        }

        private void MyInitialize()
        {
            string imagePath;
            imagePath = @"D:\ブログ用\チェック用2\WP_20200328_11_22_52_Pro_2020_03_28_午後わてん.jpg";
            //imagePath = @"E:\オレ\携帯\2019スマホ\WP_20200328_11_22_52_Pro.jpg";
            imagePath = @"D:\ブログ用\テスト用画像\grayScale.bmp";
            imagePath = @"D:\ブログ用\テスト用画像\grayscale256x256.png";
            //imagePath = @"D:\ブログ用\テスト用画像\Michelangelo's_David_-_63_grijswaarden.bmp";
            //imagePath = @"D:\ブログ用\テスト用画像\gray128.png";//0と255の中間みたい、pixelformats.blackwhiteだと市松模様になる
            //imagePath = @"D:\ブログ用\テスト用画像\gray127.png";//これは中間じゃないみたい
            //imagePath = @"D:\ブログ用\テスト用画像\gray250.png";
            //imagePath = @"D:\ブログ用\テスト用画像\ﾈｺは見ている.png";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_2097_.jpg";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん_256x192.png";

            (MyPixels, MyBitmapSource) = MakeBitmapSourceAndPixelData(imagePath, PixelFormats.Gray8, 96, 96);
            MyImage.Source = MyBitmapSource;


        }

        private BitmapSource Test1(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            int bottomP = (height - 1) * stride;//最下段判定用、これ未満なら最下段ではないことになる
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                for (int x = 0; x < width; x++)
                {
                    p = yp + x;
                    SetBlackOrWhite(gosaPixels[p], pixels, p);

                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;
                    if (p < bottomP)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //左右蛇行
        private BitmapSource Test2(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            int bottomP = (height - 1) * stride;//最下段判定用、これ未満なら最下段ではないことになる
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                if (y % 2 == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);

                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 7;
                        if (p < bottomP)
                        {
                            p += stride;
                            gosaPixels[p] += gosa * 5;
                            if (x != 0)
                                gosaPixels[p - 1] += gosa * 3;
                            if (x != width - 1)
                                gosaPixels[p + 1] += gosa * 1;
                        }
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x -= 1)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);

                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 7;//左隣
                        if (p < bottomP)
                        {
                            p += stride;
                            gosaPixels[p] += gosa * 5;
                            if (x != 0)
                                gosaPixels[p - 1] += gosa * 1;
                            if (x != width - 1)
                                gosaPixels[p + 1] += gosa * 3;
                        }

                    }
                }

            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //左右蛇行＋横だけに拡散
        private BitmapSource Test3(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                if (y % 2 == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosaPixels[p] - pixels[p];
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x -= 1)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);
                        if (x != 0)
                            gosaPixels[p - 1] += gosaPixels[p] - pixels[p];//左隣
                    }
                }

            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //左右蛇行＋横と下だけに拡散
        private BitmapSource Test4(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            int bottomP = (height - 1) * stride;//最下段判定用、これ未満なら最下段ではないことになる
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                if (y % 2 == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);

                        gosa = (gosaPixels[p] - pixels[p]) / 12.0;
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 7;
                        if (p < bottomP)
                        {
                            p += stride;
                            gosaPixels[p] += gosa * 5;
                        }
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x -= 1)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);

                        gosa = (gosaPixels[p] - pixels[p]) / 12.0;
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 7;//左隣
                        if (p < bottomP)
                        {
                            p += stride;
                            gosaPixels[p] += gosa * 5;
                        }

                    }
                }

            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        //左右蛇行+0～255リミット
        private BitmapSource Test5(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            int bottomP = (height - 1) * stride;//最下段判定用、これ未満なら最下段ではないことになる
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                if (y % 2 == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);

                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != width - 1)
                            SetGosaLimited(gosaPixels, p + 1, gosa * 7);

                        if (p < bottomP)
                        {
                            p += stride;
                            SetGosaLimited(gosaPixels, p, gosa * 5);
                            if (x != 0)
                                SetGosaLimited(gosaPixels, p - 1, gosa * 3);
                            if (x != width - 1)
                                SetGosaLimited(gosaPixels, p + 1, gosa * 1);
                        }
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x -= 1)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);

                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != 0)
                            SetGosaLimited(gosaPixels, p - 1, gosa * 7);//左隣
                        if (p < bottomP)
                        {
                            p += stride;
                            SetGosaLimited(gosaPixels, p, gosa * 5);
                            if (x != 0)
                                SetGosaLimited(gosaPixels, p - 1, gosa * 1);
                            if (x != width - 1)
                                SetGosaLimited(gosaPixels, p + 1, gosa * 3);
                        }

                    }
                }

            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }
        //誤差拡散後の値を0～255に収める
        private void SetGosaLimited(double[] gosaPixels, int p, double gosa)
        {
            gosa = gosaPixels[p] + gosa;
            if (gosa > 255) gosa = 255;
            else if (gosa < 0) gosa = 0;
            gosaPixels[p] = gosa;
        }

        //左右蛇行＋斜め下に2/16
        private BitmapSource Test6(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            int bottomP = (height - 1) * stride;//最下段判定用、これ未満なら最下段ではないことになる
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                if (y % 2 == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);

                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 7;
                        if (p < bottomP)
                        {
                            p += stride;
                            gosaPixels[p] += gosa * 5;
                            if (x != 0)
                                gosaPixels[p - 1] += gosa * 2;
                            if (x != width - 1)
                                gosaPixels[p + 1] += gosa * 2;
                        }
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x -= 1)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);

                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 7;//左隣
                        if (p < bottomP)
                        {
                            p += stride;
                            gosaPixels[p] += gosa * 5;
                            if (x != 0)
                                gosaPixels[p - 1] += gosa * 2;
                            if (x != width - 1)
                                gosaPixels[p + 1] += gosa * 2;
                        }

                    }
                }

            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //左右蛇行＋+0～255リミット＋斜め下に2/16
        private BitmapSource Test7(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            int bottomP = (height - 1) * stride;//最下段判定用、これ未満なら最下段ではないことになる
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                if (y % 2 == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);

                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != width - 1)
                            SetGosaLimited(gosaPixels, p + 1, gosa * 7);
                        if (p < bottomP)
                        {
                            p += stride;
                            gosaPixels[p] += gosa * 5;
                            if (x != 0)
                                SetGosaLimited(gosaPixels, p - 1, gosa * 2);
                            if (x != width - 1)
                                SetGosaLimited(gosaPixels, p + 1, gosa * 2);
                        }
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x -= 1)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);

                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != 0)
                            SetGosaLimited(gosaPixels, p - 1, gosa * 7);//左隣
                        if (p < bottomP)
                        {
                            p += stride;
                            SetGosaLimited(gosaPixels, p, gosa * 5);
                            if (x != 0)
                                SetGosaLimited(gosaPixels, p - 1, gosa * 2);
                            if (x != width - 1)
                                SetGosaLimited(gosaPixels, p + 1, gosa * 2);
                        }

                    }
                }

            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }




        //
        /// <summary>
        /// しきい値で2値に置き換え、しきい値は127.5固定で、127.5未満は0、127.5以上は255に置き換える
        /// 127.49999999999999999...は0、127.5以上は255
        /// </summary>
        /// <param name="value">対象の値</param>
        /// <param name="pixels">配列</param>
        /// <param name="p">配列のIndex</param>
        private void SetBlackOrWhite(double value, byte[] pixels, int p)
        {
            if (127.5 > value)
                pixels[p] = 0;
            else
                pixels[p] = 255;
        }






















        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = Test1(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = Test2(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = Test3(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = Test4(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = Test5(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = Test6(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = Test7(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
