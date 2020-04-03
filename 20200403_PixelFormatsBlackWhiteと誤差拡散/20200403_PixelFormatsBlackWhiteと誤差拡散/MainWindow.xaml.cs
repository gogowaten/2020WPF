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
//PixelFormatsBlackWhiteの誤差拡散法はどれ？

namespace _20200403_PixelFormatsBlackWhiteと誤差拡散
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource MyBitmapSource;
        byte[] MyPixels;
        byte[] BlackWhitePixels;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();

            MyInitialize();
        }
        private void MyInitialize()
        {
            MyGrid.MouseLeftButtonDown += (s, e) => Panel.SetZIndex(MyImageOrigin, 1);
            MyGrid.MouseLeftButtonUp += (s, e) => Panel.SetZIndex(MyImageOrigin, -1);

            string imagePath;
            imagePath = @"D:\ブログ用\チェック用2\WP_20200328_11_22_52_Pro_2020_03_28_午後わてん.jpg";
            imagePath = @"E:\オレ\携帯\2019スマホ\WP_20200328_11_22_52_Pro.jpg";
            imagePath = @"D:\ブログ用\テスト用画像\grayScale.bmp";
            imagePath = @"D:\ブログ用\テスト用画像\grayscale256x256.png";
            //imagePath = @"D:\ブログ用\テスト用画像\Michelangelo's_David_-_63_grijswaarden.bmp";
            //imagePath = @"D:\ブログ用\テスト用画像\gray128.png";//0と255の中間みたい、pixelformats.blackwhiteだと市松模様になる
            //imagePath = @"D:\ブログ用\テスト用画像\gray127.png";//これは中間じゃないみたい
            //imagePath = @"D:\ブログ用\テスト用画像\gray250.png";
            //imagePath = @"D:\ブログ用\テスト用画像\HSVRectH90.png";
            //imagePath = @"D:\ブログ用\テスト用画像\ﾈｺは見ている.png";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_2097_.jpg";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん_256x192.png";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_0541_2017_07_21_午後わてん_.jpg";
            //ローカルの画像をセット
            (MyPixels, MyBitmapSource) = MakeBitmapSourceAndPixelData(imagePath, PixelFormats.Gray8, 96, 96);
            //PixelFormatsをBlackWhiteに変換してからgray8に戻したBitmapのPixel作成
            var bw = new FormatConvertedBitmap(MyBitmapSource, PixelFormats.BlackWhite, null, 0);
            MyImageOrigin.Source = bw;
            bw = new FormatConvertedBitmap(bw, PixelFormats.Gray8, null, 0);
            int stride = bw.PixelWidth;
            BlackWhitePixels = new byte[bw.PixelHeight * stride];
            bw.CopyPixels(BlackWhitePixels, stride, 0);

            //アプリに埋め込んだ画像をセット
            //SetResourceImage();

            //画像を表示
            MyImage.Source = MyBitmapSource;
            //MyImageOrigin.Source = MyBitmapSource;

        }
        //アプリに埋め込んだ画像をセット
        private void SetResourceImage()
        {
            string imagePath = "_20200402_誤差拡散2値.grayScale.bmp";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(imagePath))
            {
                if (stream != null)
                {
                    BitmapSource bitmap = BitmapFrame.Create(stream);
                    bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Gray8, null, 0);
                    MyBitmapSource = bitmap;
                    MyPixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight];
                    bitmap.CopyPixels(MyPixels, bitmap.PixelWidth, 0);
                }
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

            //  * 7
            //3 5 1
            //￣16￣
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
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

        //Jarvis, Judice, and Ninke
        private BitmapSource D2_JaJuNi(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;

            //    * 7 5
            //3 5 7 5 3
            //1 3 5 3 1
            //￣48￣
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
                        gosaPixels[p + 1] += gosa * 7;
                        if (x < width - 2)
                        {
                            gosaPixels[p + 2] += gosa * 5;
                        }
                    }
                    if (y < height - 1)
                    {
                        gosaPixels[p + stride] += gosa * 7;
                        if (x < width - 1)
                        {
                            gosaPixels[p + stride + 1] += gosa * 5;
                            if (x < width - 2)
                            {
                                gosaPixels[p + stride + 2] += gosa * 3;
                            }
                        }
                        if (x > 0)
                        {
                            gosaPixels[p + stride - 1] += gosa * 5;
                            if (x > 1)
                            {
                                gosaPixels[p + stride - 2] += gosa * 3;
                            }
                        }
                    }
                    if (y < height - 2)
                    {
                        gosaPixels[p + (stride * 2)] += gosa * 5;
                        if (x < width - 1)
                        {
                            gosaPixels[p + (stride * 2) + 1] += gosa * 3;
                            if (x < width - 2)
                            {
                                gosaPixels[p + (stride * 2) + 2] += gosa * 1;
                            }
                        }
                        if (x > 0)
                        {
                            gosaPixels[p + (stride * 2) - 1] += gosa * 3;
                            if (x > 1)
                            {
                                gosaPixels[p + (stride * 2) - 2] += gosa * 1;
                            }
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }




        private BitmapSource D3_Matrix3x2(byte[] source, int width, int height, int stride, int[,] matrix, int bunbo)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];//変換先画像用
            double[] gosaPixels = new double[count];//誤差計算用
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;//誤差(変換前 - 変換後)

            //  * 7
            //3 5 1
            //￣16￣
            //matrix = new int[,] { { 0, 0, 7 }, { 3, 5, 1 } };
            //bunbo = 16;
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                for (int x = 0; x < width; x++)
                {
                    //注目ピクセルのインデックス
                    p = yp + x;
                    //しきい値127.5未満なら0にする、それ以外は255にする
                    SetBlackOrWhite(gosaPixels[p], pixels, p);
                    gosa = (gosaPixels[p] - pixels[p]) / bunbo;

                    //誤差拡散
                    if (x != width - 1)//右端ではない
                    {
                        gosaPixels[p + 1] += gosa * matrix[0, 2];
                    }

                    if (y != height - 1)//下端ではない
                    {
                        p += stride;
                        if (x != 0)//左端ではない
                        {
                            gosaPixels[p - 1] += gosa * matrix[1, 0];
                        }
                        gosaPixels[p] += gosa * matrix[1, 1];
                        if (x != width - 1)//右端ではない
                        {
                            gosaPixels[p + 1] += gosa * matrix[1, 2];
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        private BitmapSource D4_Matrix3x2蛇行(byte[] source, int width, int height, int stride, int[,] matrix, int bunbo)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];//変換先画像用
            double[] gosaPixels = new double[count];//誤差計算用
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;//誤差(変換前 - 変換後)

            //  * 7
            //3 5 1
            //￣16￣
            //matrix = new int[,] { { 0, 0, 7 }, { 3, 5, 1 } };
            //bunbo = 16;
            for (int y = 0; y < height; y++)
            {
                yp = y * stride;
                if (y % 2 == 0)
                {
                    for (int x = 0; x < width; x++)
                    {
                        //注目ピクセルのインデックス
                        p = yp + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        SetBlackOrWhite(gosaPixels[p], pixels, p);
                        gosa = (gosaPixels[p] - pixels[p]) / bunbo;

                        //誤差拡散
                        if (x != width - 1)//右端ではない
                        {
                            gosaPixels[p + 1] += gosa * matrix[0, 2];
                        }

                        if (y != height - 1)//下端ではない
                        {
                            p += stride;
                            if (x != 0)//左端ではない
                            {
                                gosaPixels[p - 1] += gosa * matrix[1, 0];
                            }
                            gosaPixels[p] += gosa * matrix[1, 1];
                            if (x != width - 1)//右端ではない
                            {
                                gosaPixels[p + 1] += gosa * matrix[1, 2];
                            }
                        }
                    }
                }
                else
                {
                    for (int x = width - 1; x >= 0; x -= 1)
                    {
                        //注目ピクセルのインデックス
                        p = yp + x;
                        //しきい値127.5未満なら0にする、それ以外は255にする
                        SetBlackOrWhite(gosaPixels[p], pixels, p);
                        gosa = (gosaPixels[p] - pixels[p]) / bunbo;

                        //誤差拡散
                        if (x != 0)//左端ではない
                        {
                            gosaPixels[p - 1] += gosa * matrix[0, 2];
                        }

                        if (y != height - 1)//下端ではない
                        {
                            p += stride;
                            if (x != 0)//左端ではない
                            {
                                gosaPixels[p - 1] += gosa * matrix[1, 2];
                            }
                            gosaPixels[p] += gosa * matrix[1, 1];
                            if (x != width - 1)//右端ではない
                            {
                                gosaPixels[p + 1] += gosa * matrix[1, 0];
                            }
                        }
                    }
                }

            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        private BitmapSource D5_ShiauFan2(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;
            //      * 8
            //1 1 2 4
            //￣16￣

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
                        gosaPixels[p + 1] += gosa * 8;
                    }
                    if (y < height - 1)
                    {
                        gosaPixels[p + stride] += gosa * 4;
                        if (x > 0)
                        {
                            gosaPixels[p + stride - 1] += gosa * 2;
                            if (x > 1)
                            {
                                gosaPixels[p + stride - 2] += gosa * 1;
                                if (x > 2)
                                {
                                    gosaPixels[p + stride - 3] += gosa * 1;
                                }
                            }
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        private BitmapSource D6_Stucki(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
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
                    //￣42￣
                    if (x < width - 1)
                    {
                        gosaPixels[p + 1] += gosa * 8;
                        if (x < width - 2)
                        {
                            gosaPixels[p + 2] += gosa * 4;
                        }
                    }
                    if (y < height - 1)
                    {
                        gosaPixels[p + stride] += gosa * 8;
                        if (x < width - 1)
                        {
                            gosaPixels[p + stride + 1] += gosa * 4;
                            if (x < width - 2)
                            {
                                gosaPixels[p + stride + 2] += gosa * 2;
                            }
                        }
                        if (x > 0)
                        {
                            gosaPixels[p + stride - 1] += gosa * 4;
                            if (x > 1)
                            {
                                gosaPixels[p + stride - 2] += gosa * 2;
                            }
                        }
                    }
                    if (y < height - 2)
                    {
                        gosaPixels[p + (stride * 2)] += gosa * 4;
                        if (x < width - 1)
                        {
                            gosaPixels[p + (stride * 2) + 1] += gosa * 2;
                            if (x < width - 2)
                            {
                                gosaPixels[p + (stride * 2) + 2] += gosa * 1;
                            }
                        }
                        if (x > 0)
                        {
                            gosaPixels[p + (stride * 2) - 1] += gosa * 2;
                            if (x > 1)
                            {
                                gosaPixels[p + (stride * 2) - 2] += gosa * 1;
                            }
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        private BitmapSource D7_Burkes(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
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
                    //￣16￣

                    if (x < width - 1)
                    {
                        gosaPixels[p + 1] += gosa * 4;
                        if (x < width - 2)
                        {
                            gosaPixels[p + 2] += gosa * 2;
                        }
                    }
                    if (y < height - 1)
                    {
                        gosaPixels[p + stride] += gosa * 4;
                        if (x < width - 1)
                        {
                            gosaPixels[p + stride + 1] += gosa * 2;
                            if (x < width - 2)
                            {
                                gosaPixels[p + stride + 2] += gosa * 1;
                            }
                        }
                        if (x > 0)
                        {
                            gosaPixels[p + stride - 1] += gosa * 2;
                            if (x > 1)
                            {
                                gosaPixels[p + stride - 2] += gosa * 1;
                            }
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        private BitmapSource D8_Sierra(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
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
                    gosa = (gosaPixels[p] - pixels[p]) / 32.0;

                    //    * 5 3
                    //2 4 5 4 2
                    //  2 3 2
                    //￣32￣
                    if (x < width - 1)
                    {
                        gosaPixels[p + 1] += gosa * 5;
                        if (x < width - 2)
                        {
                            gosaPixels[p + 2] += gosa * 3;
                        }
                    }
                    if (y < height - 1)
                    {
                        gosaPixels[p + stride] += gosa * 5;
                        if (x < width - 1)
                        {
                            gosaPixels[p + stride + 1] += gosa * 4;
                            if (x < width - 2)
                            {
                                gosaPixels[p + stride + 2] += gosa * 2;
                            }
                        }
                        if (x > 0)
                        {
                            gosaPixels[p + stride - 1] += gosa * 4;
                            if (x > 1)
                            {
                                gosaPixels[p + stride - 2] += gosa * 2;
                            }
                        }
                    }
                    if (y < height - 2)
                    {
                        gosaPixels[p + (stride * 2)] += gosa * 3;
                        if (x < width - 1)
                        {
                            gosaPixels[p + (stride * 2) + 1] += gosa * 2;
                        }
                        if (x > 0)
                        {
                            gosaPixels[p + (stride * 2) - 1] += gosa * 2;
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        private BitmapSource D9_SierraTwoRow(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
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

                    //    * 4 3
                    //1 2 3 2 1
                    //￣16￣

                    if (x < width - 1)
                    {
                        gosaPixels[p + 1] += gosa * 4;
                        if (x < width - 2)
                        {
                            gosaPixels[p + 2] += gosa * 3;
                        }
                    }
                    if (y < height - 1)
                    {
                        gosaPixels[p + stride] += gosa * 3;
                        if (x < width - 1)
                        {
                            gosaPixels[p + stride + 1] += gosa * 2;
                            if (x < width - 2)
                            {
                                gosaPixels[p + stride + 2] += gosa * 1;
                            }
                        }
                        if (x > 0)
                        {
                            gosaPixels[p + stride - 1] += gosa * 2;
                            if (x > 1)
                            {
                                gosaPixels[p + stride - 2] += gosa * 1;
                            }
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        private BitmapSource D10_SierraLite(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
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
                    gosa = (gosaPixels[p] - pixels[p]) / 4.0;

                    //  * 2
                    //1 1
                    //￣4￣

                    if (x < width - 1)
                    {
                        gosaPixels[p + 1] += gosa * 2;
                    }
                    if (y < height - 1)
                    {
                        gosaPixels[p + stride] += gosa * 1;

                        if (x > 0)
                        {
                            gosaPixels[p + stride - 1] += gosa * 1;
                        }
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        private BitmapSource D11_Atkinson(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
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
                    gosa = (gosaPixels[p] - pixels[p]) / 8;

                    //  * 1 1
                    //1 1 1
                    //  1
                    //￣8￣
                    if (x < width - 1)
                    {
                        gosaPixels[p + 1] += gosa * 1;
                        if (x < width - 2)
                        {
                            gosaPixels[p + 2] += gosa * 1;
                        }
                    }
                    if (y < height - 1)
                    {
                        gosaPixels[p + stride] += gosa * 1;
                        if (x < width - 1)
                        {
                            gosaPixels[p + stride + 1] += gosa * 1;
                        }
                        if (x > 0)
                        {
                            gosaPixels[p + stride - 1] += gosa * 1;
                        }
                    }
                    if (y < height - 2)
                    {
                        gosaPixels[p + (stride * 2)] += gosa * 1;
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }



        private BitmapSource D12(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];
            double[] gosaPixels = new double[count];
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
                    gosa = (gosaPixels[p] - pixels[p]) / 7;

                    //  * 3
                    //1 2 1
                    //
                    if (x < width - 1)
                    {
                        gosaPixels[p + 1] += gosa * 3;
                        //if (x < width - 2)
                        //{
                        //    gosaPixels[p + 2] += gosa * 1;
                        //}
                    }
                    if (y < height - 1)
                    {
                        gosaPixels[p + stride] += gosa * 2;
                        if (x < width - 1)
                        {
                            gosaPixels[p + stride + 1] += gosa * 1;
                        }
                        if (x > 0)
                        {
                            gosaPixels[p + stride - 1] += gosa * 1;
                        }
                    }
                    //if (y < height - 2)
                    //{
                    //    gosaPixels[p + (stride * 2)] += gosa * 1;
                    //}
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }











        //






















        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D1_FloydSteinberg(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button1.Content = nameof(D1_FloydSteinberg);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D2_JaJuNi(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button2.Content = nameof(D2_JaJuNi);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            int[,] matrix = new int[,] { { 0, 0, 7 }, { 3, 5, 1 } };
            MyImage.Source = D3_Matrix3x2(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, matrix, 16);
            Button3.Content = nameof(D3_Matrix3x2);
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            int[,] matrix = new int[,] { { 0, 0, 7 }, { 3, 5, 1 } };
            MyImage.Source = D4_Matrix3x2蛇行(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, matrix, 16);
            Button4.Content = nameof(D4_Matrix3x2蛇行);
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            int bunbo = 16;
            
            //var matrix = MakeMatrix2x3(bunbo);
            var matrix = MakeRandomMatrix2x3(bunbo);
            string str = "";
            for (int y = 0; y < matrix.GetLength(0); y++)
            {
                for (int x = 0; x < matrix.GetLength(1); x++)
                {
                    str += matrix[y, x] + ", ";
                }
                str += Environment.NewLine;
            }
            TextBlockMatrix.Text = $"{str}";
            MyImage.Source = D4_Matrix3x2蛇行(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, matrix, bunbo);
            Button5.Content = nameof(D4_Matrix3x2蛇行);
        }
        private int[,] MakeMatrix2x3(int bunbo)
        {
            int[,] matrix = new int[2, 3];
            matrix[0, 0] = 0;
            matrix[0, 1] = 0;
            var r = new Random();
            int nokori = bunbo;
            int v = (int)(r.NextDouble() * nokori);
            matrix[0, 2] = v;
            nokori -= v;
            v = (int)(r.NextDouble() * nokori);
            matrix[1, 0] = v;
            nokori -= v;
            v = (int)(r.NextDouble() * nokori);
            matrix[1, 1] = v;
            nokori -= v;
            matrix[1, 2] = nokori;
            return matrix;
        }
        private int[,] MakeRandomMatrix2x3(int bunbo)
        {
            var r = new Random();
            var ll = new List<int>();
            
            int nokori = bunbo;
            //int[] vs = new int[4];
            int temp;
            for (int j = 0; j < 3; j++)
            {
                temp = (int)(r.NextDouble() * nokori);
                ll.Add(temp);
                nokori -= temp;
            }
            ll.Add(nokori);

            int[] ii = new int[4];
            for (int k = 3; k >= 0; k--)
            {
                int v = r.Next(k+1);
                ii[k] = ll[v];
                ll.RemoveAt(v);
            }
            int[,] matrix = new int[2,3];
            matrix[0, 0] = 0;
            matrix[0, 1] = 0;
            matrix[0, 2] = ii[0];
            matrix[1, 0] = ii[1];
            matrix[1, 1] = ii[2];
            matrix[1, 2] = ii[3];
            return matrix;
        }


        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D6_Stucki(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button6.Content = nameof(D6_Stucki);
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D7_Burkes(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button7.Content = nameof(D7_Burkes);
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D8_Sierra(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button8.Content = nameof(D8_Sierra);
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
            var bitmap = (BitmapSource)MyImage.Source;
            //bitmap =new FormatConvertedBitmap(bitmap, PixelFormats.Gray8,null,0);
            //int w = bitmap.PixelWidth;
            //int h = bitmap.PixelHeight;
            //int stride = w;
            //byte[] pixels = new byte[h * stride];
            //bitmap.CopyPixels(pixels, stride, 0);
            //for (int i = 0; i < pixels.Length; i++)
            //{
            //    if (pixels[i] == 0) { };
            //}
        }

        private void Button9_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D9_SierraTwoRow(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button9.Content = nameof(D9_SierraTwoRow);
        }

        private void Button10_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D10_SierraLite(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button10.Content = nameof(D10_SierraLite);
        }

        private void Button11_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D11_Atkinson(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button11.Content = nameof(D11_Atkinson);
        }

        private void Button12_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D12(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button12.Content = nameof(D12);
        }

        private void ButtonHikaku_Click(object sender, RoutedEventArgs e)
        {
            var (b, w) = Hikaku();
            TextBlockDiff.Text = $"黒={b}, 白={w}";
        }
        private (int b, int w) Hikaku()
        {
            var bmp = (BitmapSource)MyImage.Source;
            int stride = bmp.PixelWidth;
            byte[] pixels = new byte[bmp.PixelHeight * stride];
            bmp.CopyPixels(pixels, stride, 0);

            int b = 0;
            int w = 0;
            for (int i = 0; i < BlackWhitePixels.Length; i++)
            {
                if (BlackWhitePixels[i] == 0) b++;
                else w++;
                if (pixels[i] == 0) b--;
                else w--;
            }
            return (b, w);

        }

        //private void Button13_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = D9_SierraTwoRow(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button13.Content = nameof(D9_SierraTwoRow);
        //}

        //private void Button14_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = D10_SierraLite(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button14.Content = nameof(D10_SierraLite);
        //}

        //private void Button15_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = D11_Atkinson(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button15.Content = nameof(D11_Atkinson);
        //}
    }
}
