using System;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//Libcacaスタディ-5.グレイスケールディザリング
//http://caca.zoy.org/study/part5.html


namespace _20200507_ガンマ補正してから3色誤差拡散
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
            string path = "_20200507_ガンマ補正してから3色誤差拡散.grayScale.bmp";
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





        /// <summary>
        /// ガンマ補正なしで0 127 255の3色に減色、FloydSteinbergで誤差拡散、PixelFormat.Gray8グレースケール画像専用
        /// 0～84を0に変換、85～170を127、それ以外を255に変換
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private BitmapSource D1_Color3(BitmapSource source)
        {
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
                    //3色に変換
                    if (gosaPixels[p] < 85) pixels[p] = 0;
                    else if (gosaPixels[p] < 171) pixels[p] = 127;
                    else pixels[p] = 255;

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


        /// <summary>
        /// もとの値を逆側にガンマ補正して返す、double型
        /// </summary>
        /// <param name="pixels">もとの値の配列</param>
        /// <param name="gamma">ガンマ値</param>
        /// <returns></returns>
        private double[] InvertGamma2Color(byte[] pixels, double gamma)
        {
            double[] vs = new double[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                double d = pixels[i] / 255.0;
                vs[i] = Math.Pow(d, gamma) * 255.0;
            }
            return vs;
        }

        //0.0～1.0までを一括2.2でガンマ補正してから変換
        /// <summary>
        /// 全体をガンマ値2.2で補正してから減色、誤差拡散
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private BitmapSource D2_Color3Gamma(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //2.2で逆ガンマ補正した値の配列、誤差拡散計算用
            double[] gosaPixels = InvertGamma2Color(pixels, 2.2);

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
                    //3色に変換
                    if (gosaPixels[p] < 85) pixels[p] = 0;
                    else if (gosaPixels[p] < 171) pixels[p] = 127;
                    else pixels[p] = 255;

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;
                    if (y < height - 1)
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



        /// <summary>
        /// 3色用ガンマ補正用、0～0.5までと0.5～1.0までを、それぞれ2.2で補正した配列を返す
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="gamma"></param>
        /// <returns></returns>
        private double[] InvertGamma3Color(byte[] pixels, double gamma)
        {
            double[] vs = new double[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                double d = pixels[i] / 255.0;
                if (d <= 0.5)
                {
                    vs[i] = (Math.Pow(d * 2, gamma) / 2.0 * 255);
                }
                else
                {
                    vs[i] = ((0.5 + Math.Pow((d - 0.5) * 2, gamma) / 2.0) * 255);
                }
            }
            return vs;
        }

        //0.0～0.5と0.5～1.0に2分して別々にガンマ補正、ガンマ値は両方とも2.2
        private BitmapSource D3_Color3EachGamma(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //2.2で逆ガンマ補正した値の配列、誤差拡散計算用
            double[] gosaPixels = InvertGamma3Color(pixels, 2.2);

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
                    //3色に変換
                    if (gosaPixels[p] < 85) pixels[p] = 0;
                    else if (gosaPixels[p] < 171) pixels[p] = 127;
                    else pixels[p] = 255;

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;
                    if (y < height - 1)
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

        //E:\オレ\エクセル\画像処理.xlsm_ガンマ補正_$E$71

        /// <summary>
        /// 3色用ガンマ補正、0～0.5までと0.5～1.0までを指定したガンマ値で補正する
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="gamma1">0.0～0.5までのガンマ値</param>
        /// <param name="gamma2">0.5～1.0までのガンマ値</param>
        /// <returns></returns>
        private double[] InvertEachGamma3Color(byte[] pixels, double gamma1, double gamma2)
        {
            double[] vs = new double[pixels.Length];
            //0～255を0～1に変換して補正してから、また0～255に戻す
            for (int i = 0; i < pixels.Length; i++)
            {
                double d = pixels[i] / 255.0;
                if (d <= 0.5)
                {
                    vs[i] = Math.Pow(d * 2, gamma1) / 2.0 * 255;
                }
                else
                {
                    //0.5～1.0区間は
                    // = 開始値 + ((入力値 - 開始値) * 分割数) ^ ガンマ値 / 分割数
                    // = 0.5 + ((入力値 - 0.5) * 2) ^ ガンマ値 / 2
                    vs[i] = (0.5 + (Math.Pow((d - 0.5) * 2, gamma2) / 2.0)) * 255;
                }
            }
            return vs;
        }

        //0.0～0.5と0.5～1.0を別々にガンマ補正、ガンマ値は
        //0.0～0.5は2.2
        //0.5～1.0は計算して1.3243119で補正
        private BitmapSource D4_Color3EachGamma2(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);
            //0～0.5区間で使うガンマ値は初期の2.2
            double gamma1 = 2.2;

            //0.5～1.0区間で使うガンマ値を計算
            //開始値は0.5これを初期γで補正する

            //補正後 = 0.72974005
            // = 開始値 ^ (1 / 初期γ)
            // = 0.5 ^ (1 / 2.2)
            // = 0.72974005
            //0.5のガンマ補正後の値 = 0.7297401
            double correct = Math.Pow(0.5, 1.0 / gamma1);

            //0.5～1.0区間で使うガンマ値を、0.5の補正後値を使って計算すると
            // = (-ガンマ値全体距離 * 補正後) + 初期γ
            // = -(2.2 - 1.0) * 0.72974005) + 2.2
            // = 1.3243119
            double gamma2 = -(gamma1 - 1.0) * correct + gamma1;

            //逆ガンマ補正した値の配列、誤差拡散計算用
            double[] gosaPixels = InvertEachGamma3Color(pixels, gamma1, gamma2);

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
                    //3色に変換
                    if (gosaPixels[p] < 85) pixels[p] = 0;
                    else if (gosaPixels[p] < 171) pixels[p] = 127;
                    else pixels[p] = 255;

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;
                    if (y < height - 1)
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


        private BitmapSource D5_Color3EachGamma2蛇行走査(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);
            //0～0.5区間で使うガンマ値は初期の2.2
            double gamma1 = 2.2;

            //0.5～1.0区間で使うガンマ値を計算
            //開始値は0.5これを初期γで補正する

            //補正後 = 0.72974005
            // = 開始値 ^ (1 / 初期γ)
            // = 0.5 ^ (1 / 2.2)
            // = 0.72974005
            //0.5のガンマ補正後の値 = 0.7297401
            //double correct = Math.Pow(0.5, gamma1);
            double correct = Math.Pow(0.5, 1.0 / gamma1);

            //0.5～1.0区間で使うガンマ値を、0.5の補正後値を使って計算すると
            // = (-ガンマ値全体距離 * 補正後) + 初期γ
            // = -(2.2 - 1.0) * 0.72974005) + 2.2
            // = 1.3243119
            double gamma2 = -(gamma1 - 1.0) * correct + gamma1;

            //逆ガンマ補正した値の配列、誤差拡散計算用
            double[] gosaPixels = InvertEachGamma3Color(pixels, gamma1, gamma2);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            
            for (int y = 0; y < height; y++)
            {
                if (y % 2 == 0)
                {
                    //  * 7
                    //3 5 1
                    //￣16￣
                    for (int x = 0; x < width; x++)
                    {
                        //注目ピクセルのインデックス
                        p = y * stride + x;
                        //3色に変換
                        if (gosaPixels[p] < 85) pixels[p] = 0;
                        else if (gosaPixels[p] < 171) pixels[p] = 127;
                        else pixels[p] = 255;

                        //誤差拡散
                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 7;
                        if (y < height - 1)
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
                    //7 *
                    //1 5 3
                    //￣16￣
                    for (int x = width-1; x >= 0; x--)
                    {
                        //注目ピクセルのインデックス
                        p = y * stride + x;
                        //3色に変換
                        if (gosaPixels[p] < 85) pixels[p] = 0;
                        else if (gosaPixels[p] < 171) pixels[p] = 127;
                        else pixels[p] = 255;

                        //誤差拡散
                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 7;
                        if (y < height - 1)
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
            Button1.Content = nameof(D1_Color3);
            MyImage.Source = D1_Color3(MyBitmapSource);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button2.Content = nameof(D2_Color3Gamma);
            MyImage.Source = D2_Color3Gamma(MyBitmapSource);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button3.Content = nameof(D3_Color3EachGamma);
            MyImage.Source = D3_Color3EachGamma(MyBitmapSource);
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button4.Content = nameof(D4_Color3EachGamma2);
            MyImage.Source = D4_Color3EachGamma2(MyBitmapSource);
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button5.Content = nameof(D5_Color3EachGamma2蛇行走査);
            MyImage.Source = D5_Color3EachGamma2蛇行走査(MyBitmapSource);
        }
    }
}
