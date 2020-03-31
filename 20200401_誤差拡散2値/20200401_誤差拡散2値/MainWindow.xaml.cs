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
//Libcaca study - 3. Error diffusion
//http://caca.zoy.org/study/part3.html

namespace _20200401_誤差拡散2値
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
            //imagePath = @"D:\ブログ用\テスト用画像\grayscale256x256.png";
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

        private BitmapSource FloydSteinberg(byte[] source, int width, int height, int stride)
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

        //0～255リミット
        private BitmapSource FloydSteinbergLimit(byte[] source, int width, int height, int stride)
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
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //リミット＋下と横だけに拡散
        private BitmapSource Test3(byte[] source, int width, int height, int stride)
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

                    gosa = (gosaPixels[p] - pixels[p]) / 12.0;
                    if (x != width - 1)
                        SetGosaLimited(gosaPixels, p + 1, gosa * 7);
                    if (p < bottomP)
                    {
                        p += stride;
                        SetGosaLimited(gosaPixels, p, gosa * 5);
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //下と横だけに拡散
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
                for (int x = 0; x < width; x++)
                {
                    p = yp + x;
                    SetBlackOrWhite(gosaPixels[p], pixels, p);

                    gosa = (gosaPixels[p] - pixels[p]) / 12.0;
                    if (x != width - 1)
                        SetGosa(gosaPixels, p + 1, gosa * 7);
                    if (p < bottomP)
                    {
                        p += stride;
                        SetGosa(gosaPixels, p, gosa * 5);
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }
        private void SetGosa(double[] pixels, int p, double gosa)
        {
            pixels[p] += gosa;
        }

        //Jarvis, Judice, and Nink
        private BitmapSource JaJuNi(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                for (int x = 0; x < width; x++)
                {
                    p = yp + x;
                    SetBlackOrWhite(gosaPixels[p], pixels, p);
                    gosa = (gosaPixels[p] - pixels[p]) / 48.0;


                    if (x < width - 1)
                    {
                        SetGosa(gosaPixels, p + 1, gosa * 7);
                        if (x < width - 2)
                        {
                            SetGosa(gosaPixels, p + 2, gosa * 5);
                        }
                    }
                    if (y < height - 1)
                    {
                        SetGosa(gosaPixels, p + stride, gosa * 7);
                        if (x < width - 1)
                        {
                            SetGosa(gosaPixels, p + stride + 1, gosa * 5);
                            if (x < width - 2)
                            {
                                SetGosa(gosaPixels, p + stride + 2, gosa * 3);
                            }
                        }
                        if (x > 0)
                        {
                            SetGosa(gosaPixels, p + stride - 1, gosa * 4);
                            if (x > 1)
                            {
                                SetGosa(gosaPixels, p + stride - 2, gosa * 3);
                            }
                        }
                    }
                    if (y < height - 2)
                    {
                        SetGosa(gosaPixels, p + (stride * 2), gosa * 5);
                        if (x < width - 1)
                        {
                            SetGosa(gosaPixels, p + (stride * 2) + 1, gosa * 3);
                            if (x < width - 2)
                            {
                                SetGosa(gosaPixels, p + (stride * 2) + 2, gosa * 1);
                            }
                        }
                        if (x > 0)
                        {
                            SetGosa(gosaPixels, p + (stride * 2) - 1, gosa * 3);
                            if (x > 1)
                            {
                                SetGosa(gosaPixels, p + (stride * 2) - 2, gosa * 1);
                            }
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

        //Jarvis, Judice, and Nink+リミット
        private BitmapSource JaJuNiLimit(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                for (int x = 0; x < width; x++)
                {
                    p = yp + x;
                    SetBlackOrWhite(gosaPixels[p], pixels, p);
                    gosa = (gosaPixels[p] - pixels[p]) / 48.0;


                    if (x < width - 1)
                    {
                        SetGosaLimited(gosaPixels, p + 1, gosa * 7);
                        if (x < width - 2)
                        {
                            SetGosaLimited(gosaPixels, p + 2, gosa * 5);
                        }
                    }
                    if (y < height - 1)
                    {
                        SetGosaLimited(gosaPixels, p + stride, gosa * 7);
                        if (x < width - 1)
                        {
                            SetGosaLimited(gosaPixels, p + stride + 1, gosa * 5);
                            if (x < width - 2)
                            {
                                SetGosaLimited(gosaPixels, p + stride + 2, gosa * 3);
                            }
                        }
                        if (x > 0)
                        {
                            SetGosaLimited(gosaPixels, p + stride - 1, gosa * 4);
                            if (x > 1)
                            {
                                SetGosaLimited(gosaPixels, p + stride - 2, gosa * 3);
                            }
                        }
                    }
                    if (y < height - 2)
                    {
                        SetGosaLimited(gosaPixels, p + (stride * 2), gosa * 5);
                        if (x < width - 1)
                        {
                            SetGosaLimited(gosaPixels, p + (stride * 2) + 1, gosa * 3);
                            if (x < width - 2)
                            {
                                SetGosaLimited(gosaPixels, p + (stride * 2) + 2, gosa * 1);
                            }
                        }
                        if (x > 0)
                        {
                            SetGosaLimited(gosaPixels, p + (stride * 2) - 1, gosa * 3);
                            if (x > 1)
                            {
                                SetGosaLimited(gosaPixels, p + (stride * 2) - 2, gosa * 1);
                            }
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //Floyd-Steinberg derivatives
        private BitmapSource FloydSteinbergDervatives(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                for (int x = 0; x < width; x++)
                {
                    p = yp + x;
                    SetBlackOrWhite(gosaPixels[p], pixels, p);
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;


                    if (x < width - 1)
                    {
                        SetGosa(gosaPixels, p + 1, gosa * 7);                        
                    }
                    if (y < height - 1)
                    {
                        SetGosa(gosaPixels, p + stride, gosa * 5);                        
                        if (x > 0)
                        {
                            SetGosa(gosaPixels, p + stride - 1, gosa * 3);
                            if (x > 1)
                            {
                                SetGosa(gosaPixels, p + stride - 2, gosa * 1);
                            }
                        }
                    }                    
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        private BitmapSource ShiauFanDithering(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                for (int x = 0; x < width; x++)
                {
                    p = yp + x;
                    SetBlackOrWhite(gosaPixels[p], pixels, p);
                    gosa = (gosaPixels[p] - pixels[p]) / 8.0;


                    if (x < width - 1)
                    {
                        SetGosa(gosaPixels, p + 1, gosa * 4);
                    }
                    if (y < height - 1)
                    {
                        SetGosa(gosaPixels, p + stride, gosa * 2);
                        if (x > 0)
                        {
                            SetGosa(gosaPixels, p + stride - 1, gosa * 1);
                            if (x > 1)
                            {
                                SetGosa(gosaPixels, p + stride - 2, gosa * 1);
                            }
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        private BitmapSource ShiauFanDithering2(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                for (int x = 0; x < width; x++)
                {
                    p = yp + x;
                    SetBlackOrWhite(gosaPixels[p], pixels, p);
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;

                    if (x < width - 1)
                    {
                        SetGosa(gosaPixels, p + 1, gosa * 8);
                    }
                    if (y < height - 1)
                    {
                        SetGosa(gosaPixels, p + stride, gosa * 4);
                        if (x > 0)
                        {
                            SetGosa(gosaPixels, p + stride - 1, gosa * 2);
                            if (x > 1)
                            {
                                SetGosa(gosaPixels, p + stride - 2, gosa * 1);
                                if (x > 2)
                                {
                                    SetGosa(gosaPixels, p + stride - 3, gosa * 1);
                                }
                            }
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        private BitmapSource StuckiDithering(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                for (int x = 0; x < width; x++)
                {
                    p = yp + x;
                    SetBlackOrWhite(gosaPixels[p], pixels, p);
                    gosa = (gosaPixels[p] - pixels[p]) / 42.0;

                    //    * 8 4
                    //2 4 8 4 2
                    //1 2 4 2 1
                    if (x < width - 1)
                    {
                        SetGosa(gosaPixels, p + 1, gosa * 8);
                        if (x < width - 2)
                        {
                            SetGosa(gosaPixels, p + 2, gosa * 4);
                        }
                    }
                    if (y < height - 1)
                    {
                        SetGosa(gosaPixels, p + stride, gosa * 8);
                        if (x < width - 1)
                        {
                            SetGosa(gosaPixels, p + stride + 1, gosa * 4);
                            if (x < width - 2)
                            {
                                SetGosa(gosaPixels, p + stride + 2, gosa * 2);
                            }
                        }
                        if (x > 0)
                        {
                            SetGosa(gosaPixels, p + stride - 1, gosa * 4);
                            if (x > 1)
                            {
                                SetGosa(gosaPixels, p + stride - 2, gosa * 2);
                            }
                        }
                    }
                    if (y < height - 2)
                    {
                        SetGosa(gosaPixels, p + (stride * 2), gosa * 4);
                        if (x < width - 1)
                        {
                            SetGosa(gosaPixels, p + (stride * 2) + 1, gosa * 2);
                            if (x < width - 2)
                            {
                                SetGosa(gosaPixels, p + (stride * 2) + 2, gosa * 1);
                            }
                        }
                        if (x > 0)
                        {
                            SetGosa(gosaPixels, p + (stride * 2) - 1, gosa * 2);
                            if (x > 1)
                            {
                                SetGosa(gosaPixels, p + (stride * 2) - 2, gosa * 1);
                            }
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        private BitmapSource BurkesDithering(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, pixels, count);
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                for (int x = 0; x < width; x++)
                {
                    p = yp + x;
                    SetBlackOrWhite(gosaPixels[p], pixels, p);
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;

                    //    * 4 2
                    //1 2 4 2 1
                    
                    if (x < width - 1)
                    {
                        SetGosa(gosaPixels, p + 1, gosa * 4);
                        if (x < width - 2)
                        {
                            SetGosa(gosaPixels, p + 2, gosa * 2);
                        }
                    }
                    if (y < height - 1)
                    {
                        SetGosa(gosaPixels, p + stride, gosa * 4);
                        if (x < width - 1)
                        {
                            SetGosa(gosaPixels, p + stride + 1, gosa * 2);
                            if (x < width - 2)
                            {
                                SetGosa(gosaPixels, p + stride + 2, gosa * 1);
                            }
                        }
                        if (x > 0)
                        {
                            SetGosa(gosaPixels, p + stride - 1, gosa * 2);
                            if (x > 1)
                            {
                                SetGosa(gosaPixels, p + stride - 2, gosa * 1);
                            }
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
            MyImage.Source = FloydSteinberg(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button1.Content = nameof(FloydSteinberg);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = FloydSteinbergLimit(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
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

            MyImage.Source = JaJuNi(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = JaJuNiLimit(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = FloydSteinbergDervatives(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = ShiauFanDithering(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button8.Content = nameof(ShiauFanDithering);
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

        private void Button9_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = ShiauFanDithering2(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button9.Content = nameof(ShiauFanDithering2);
        }

        private void Button10_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = StuckiDithering(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button10.Content = nameof(StuckiDithering);
        }

        private void Button11_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = BurkesDithering(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button10.Content = nameof(BurkesDithering);
        }
    }
}


