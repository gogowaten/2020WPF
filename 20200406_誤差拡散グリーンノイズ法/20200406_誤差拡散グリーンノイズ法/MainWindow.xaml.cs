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

namespace _20200406_誤差拡散グリーンノイズ法
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


            ////アプリに埋め込んだ画像をセット
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

        //アプリに埋め込んだ画像をセット
        private void SetResourceImage()
        {
            string imagePath = "_20200404_誤差拡散法蛇行走査.grayScale.bmp";
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
        
        //グリーンノイズ法？
        //127.5で2値化してから誤差拡散
        private BitmapSource D2_GreenNoise(byte[] source, int width, int height, int stride)
        {
            int count = source.Length;
            byte[] pixels = new byte[count];//変換先画像用
            double[] gosaPixels = new double[count];//誤差計算用
            Array.Copy(source, gosaPixels, count);
            int p, yp;//座標
            double gosa;//誤差(変換前 - 変換後)
            double gain = 0.5;
            double greenNoise=0;
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

                    //1 5 3
                    //7 *
                    //￣16￣
                    for (int x = 0; x < width; x++)
                    {
                        //注目ピクセルのインデックス
                        p = yp + x;
                        //グリーンノイズ量
                        if (x != 0)
                            greenNoise += pixels[p-1] * (7 / 16.0);
                        if (y !=0)
                        {
                            greenNoise += pixels[p - stride] * (5 / 16.0);
                            if (x != 0)
                                greenNoise += pixels[p - stride - 1] * (1 / 16.0);
                            if (x != width - 1)
                                greenNoise += pixels[p - stride + 1] * (3 / 16.0);
                        }
                        greenNoise *= gain;
                        gosaPixels[p] -= greenNoise;

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
                        greenNoise = 0;
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
                        //グリーンノイズ量
                        if (x != width-1)
                            greenNoise += pixels[p + 1] * 7 / 16.0;
                        if (y !=0)
                        {
                            greenNoise += pixels[p - stride] * 5 / 16.0;
                            if (x != 0)
                                greenNoise += pixels[p - stride - 1] * 3 / 16.0;
                            if (x != width - 1)
                                greenNoise += pixels[p - stride + 1] * 1 / 16.0;
                        }
                        greenNoise *= gain;
                        gosaPixels[p] -= greenNoise;

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
                        greenNoise = 0;
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }
        private void SetBlackOrWhite2(double value, byte[] pixels, int p)
        {
            if (127.5 > value)
                pixels[p] = 0;
            else
                pixels[p] = 255;
        }
        private void ToBinary(byte[] pixels, byte[] source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] < 127.5)
                    pixels[i] = 0;
                else
                    pixels[i] = 255;
            }

        }


        //Floyd-Steinberg derivatives
        private BitmapSource D3_FloydSteinbergDervatives(byte[] source, int width, int height, int stride)
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
                if (y % 2 == 0)
                {
                    //    * 7
                    //1 3 5
                    //￣16￣
                    for (int x = 0; x < width; x++)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);
                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;


                        if (x < width - 1)
                        {
                            gosaPixels[p + 1] += gosa * 7;
                        }
                        if (y < height - 1)
                        {
                            gosaPixels[p + stride] += gosa * 5;
                            if (x > 0)
                            {
                                gosaPixels[p + stride - 1] += gosa * 3;
                                if (x > 1)
                                {
                                    gosaPixels[p + stride - 2] += gosa * 1;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //  7 *
                    //    5 3 1
                    //  ￣16￣
                    for (int x = width - 1; x >= 0; x--)
                    {
                        p = yp + x;
                        SetBlackOrWhite(gosaPixels[p], pixels, p);
                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;

                        if (x != 0)
                        {
                            gosaPixels[p - 1] += gosa * 7;
                        }
                        if (y < height - 1)
                        {
                            gosaPixels[p + stride] += gosa * 5;
                            if (x != width - 1)
                            {
                                gosaPixels[p + stride + 1] += gosa * 3;
                                if (x < width - 2)
                                {
                                    gosaPixels[p + stride + 2] += gosa * 1;
                                }
                            }
                        }
                    }
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
            MyImage.Source = D2_GreenNoise(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button2.Content = nameof(D2_GreenNoise);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = D3_FloydSteinbergDervatives(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            Button3.Content = nameof(D3_FloydSteinbergDervatives);
        }

        //private void Button4_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = D4_ShiauFan(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button4.Content = nameof(D4_ShiauFan);
        //}

        //private void Button5_Click(object sender, RoutedEventArgs e)
        //{

        //    MyImage.Source = D5_ShiauFan2(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button5.Content = nameof(D5_ShiauFan2);
        //}

        //private void Button6_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = D6_Stucki(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button6.Content = nameof(D6_Stucki);
        //}

        //private void Button7_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = D7_Burkes(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button7.Content = nameof(D7_Burkes);
        //}

        //private void Button8_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = D8_Sierra(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button8.Content = nameof(D8_Sierra);
        //}


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

        //private void Button9_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = D9_SierraTwoRow(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button9.Content = nameof(D9_SierraTwoRow);
        //}

        //private void Button10_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = D10_SierraLite(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button10.Content = nameof(D10_SierraLite);
        //}

        //private void Button11_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = D11_Atkinson(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button11.Content = nameof(D11_Atkinson);
        //}

        //private void Button12_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = D12(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    Button12.Content = nameof(D12);
        //}

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