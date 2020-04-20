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
using System.Diagnostics;


namespace _20200420_局所的可変しきい値で2値化Niblack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource MyBitmapSource;
        byte[] MyPixels;
        byte[] MyPalette;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
            MyGrid.MouseLeftButtonDown += (s, e) => Panel.SetZIndex(MyImageOrigin, 1);
            MyGrid.MouseLeftButtonUp += (s, e) => Panel.SetZIndex(MyImageOrigin, -1);

            MyInitialize();


        }

        //画像ファイルがドロップされた時
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            //8bitグレースケールに変換して読み込む
            (MyPixels, MyBitmapSource) = MakeBitmapSourceAndPixelData(filePath[0], PixelFormats.Gray8, 96, 96);
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
            MyImageOrigin.Source = MyBitmapSource;



        }



        private byte[] NiblackMulti(byte[] pixels, int width, int height, int stride)
        {
            byte[] result = new byte[pixels.Length];
            double NiblackK = ScrollBarNiblack.Value;
            Parallel.For(0, height, y =>
            {
                int p;
                byte[] localArea = new byte[15 * 15];
                int effectiveNumber;//有効数
                double threshold = 127.5;
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    effectiveNumber = 0;

                    for (int i = -7; i < 8; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < height)
                        {
                            for (int j = -7; j < 8; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < width)
                                {
                                    localArea[effectiveNumber] = pixels[(yy * stride) + xx];
                                    effectiveNumber++;
                                }
                            }
                        }
                    }

                    (double stdev, double average) vv = StdevAndAverage(localArea, effectiveNumber);
                    if (vv.stdev != 0)
                        threshold = vv.average + (NiblackK * vv.stdev);
                    else
                        threshold = 127.5;

                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            });
            return result;
        }

        private byte[] Niblack(byte[] pixels, int width, int height, int stride)
        {
            //15*15 p-7~p+7 =225
            //自分(注目ピクセル)の周囲15x15ピクセル(局所範囲)画素値の平均と標準偏差からしきい値を決定する

            byte[] result = new byte[pixels.Length];//2値に置き換えた用
            int p;
            //周囲ピクセルの位置が画像範囲内のものだけの値を入れる用
            byte[] localArea = new byte[15 * 15];
            //係数-0.2が丁度いいらしい
            double NiblackK = ScrollBarNiblack.Value;
            //2値化
            for (int y = 0; y < height; y++)
            {
                int effectiveNumber;//有効数、周囲ピクセルの位置が画像範囲内の個数
                double threshold = 127.5;//標準しきい値
                for (int x = 0; x < width; x++)
                {
                    effectiveNumber = 0;
                    //注目ピクセルの周囲-7から7まで(局所範囲)の値を配列に集める
                    for (int i = -7; i < 8; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < height)//y座標有効判定
                        {
                            for (int j = -7; j < 8; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < width)//x座標有効判定
                                {
                                    localArea[effectiveNumber] = pixels[(yy * stride) + xx];
                                    effectiveNumber++;
                                }
                            }
                        }
                    }


                    //局所範囲の標準偏差と平均値を取得
                    (double stdev, double average) vv = StdevAndAverage(localArea, effectiveNumber);
                    //しきい値計算は標準偏差が0以外ならって条件をつけたけど、これはないほうがいいかも？
                    if (vv.stdev != 0)
                        threshold = vv.average + (NiblackK * vv.stdev);

                    //しきい値で2値化
                    p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }
            return result;
        }


        //15*15 p-7~p+7 =225
        //自分(注目ピクセル)の周囲15x15ピクセル(局所範囲)画素値の平均と標準偏差からしきい値を決定する
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="near">周囲の範囲、2なら自分の上下左右2ピクセルなので5x5ピクセルの意味になる</param>
        /// <returns></returns>
        private byte[] Niblack2(byte[] pixels, int width, int height, int stride, int near)
        {
            byte[] result = new byte[pixels.Length];//2値に置き換えた用
            //係数-0.2が丁度いいらしい
            double NiblackK = ScrollBarNiblack.Value;
            //2値化
            for (int y = 0; y < height; y++)
            {
                int p;
                //周囲ピクセルの位置が画像範囲内のものだけの値を入れる用
                byte[] localArea = new byte[near * 2 + 1 * near * 2 + 1];
                int effectiveIndex;//周囲ピクセルのカウント用、画像範囲外はカウントしない
                double threshold = 127.5;//標準しきい値
                for (int x = 0; x < width; x++)
                {
                    effectiveIndex = 0;
                    //注目ピクセルの周囲(局所範囲)の値を配列に集める
                    for (int i = -7; i < 8; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < height)//y座標有効判定
                        {
                            for (int j = -7; j < 8; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < width)//x座標有効判定
                                {
                                    localArea[effectiveIndex] = pixels[(yy * stride) + xx];
                                    effectiveIndex++;
                                }
                            }
                        }
                    }


                    //局所範囲の標準偏差と平均値を取得
                    (double stdev, double average) vv = StdevAndAverage(localArea, effectiveIndex);
                    //しきい値計算は標準偏差が0以外ならって条件をつけたけど、これはないほうがいいかも？
                    if (vv.stdev != 0)
                        threshold = vv.average + (NiblackK * vv.stdev);

                    //しきい値で2値化
                    p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }
            return result;
        }

        /// <summary>
        /// 注目ピクセルの周囲(局所範囲)の値を配列に集める
        /// </summary>
        /// <param name="localArea"></param>
        /// <param name="near"></param>
        /// <param name="pixels"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <returns></returns>
        private int GetLocalArea(byte[] localArea,int near, byte[] pixels, int x, int y, int width, int height, int stride)
        {
            //周囲ピクセルのカウント用、画像範囲外はカウントしない
            int effectiveIndex = 0;
            
            for (int i = -near; i <= near; i++)
            {
                int yy = y + i;
                if (yy >= 0 && yy < height)//y座標有効判定
                {
                    for (int j = -near; j <= near; j++)
                    {
                        int xx = x + j;
                        if (xx >= 0 && xx < width)//x座標有効判定
                        {
                            localArea[effectiveIndex] = pixels[(yy * stride) + xx];
                            effectiveIndex++;
                        }
                    }
                }
            }
            return effectiveIndex;
        }


        //配列の標準偏差と平均値
        private (double stdev, double average) StdevAndAverage(byte[] vs, int effectiveNumber)
        {
            long aveTotal = 0;
            long squareToral = 0;
            for (int i = 0; i < effectiveNumber; i++)
            {
                aveTotal += vs[i];
                squareToral += vs[i] * vs[i];
            }
            double average = aveTotal / (double)effectiveNumber;
            return (Math.Sqrt((squareToral / (double)effectiveNumber) - (average * average)), average);
        }


        private BitmapSource MakeBitmapSource(byte[] pixels, int width, int height, int stride)
        {
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }














        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Clipboard.SetImage((BitmapSource)MyImage.Source);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            byte[] pixels = Niblack(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            MyImage.Source = MakeBitmapSource(pixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
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

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            byte[] pixels = NiblackMulti(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            MyImage.Source = MakeBitmapSource(pixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            byte[] pixels = Sauvola(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            MyImage.Source = MakeBitmapSource(pixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }


        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            byte[] pixels = Bernsen(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            MyImage.Source = MakeBitmapSource(pixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = ReplaceColor蛇行(MyPalette, MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        //private void Button6_Click(object sender, RoutedEventArgs e)
        //{
        //    MyImage.Source = ReplaceColor蛇行2(MyPalette, MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //}

        //private void Button7_Click(object sender, RoutedEventArgs e)
        //{
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    MyImage.Source = ReplaceColor蛇行3(MyPalette, MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    sw.Stop();
        //    TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        //}

        //private void Button8_Click(object sender, RoutedEventArgs e)
        //{
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    MyImage.Source = ReplaceColor蛇行4xy(MyPalette, MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        //    sw.Stop();
        //    TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        //}

        private void ButtonBlackWhite_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = new FormatConvertedBitmap(MyBitmapSource, PixelFormats.BlackWhite, null, 0);
        }

        private void ScrollBarSauvolaR_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ScrollBarSauvolaR.Value += ScrollBarSauvolaR.LargeChange;
            else
                ScrollBarSauvolaR.Value -= ScrollBarSauvolaR.LargeChange;
        }

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
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
            int stride = w;
            MyPixels = new byte[h * stride];
            bmp.CopyPixels(MyPixels, stride, 0);
            MyBitmapSource = bmp;
            MyImage.Source = MyBitmapSource;
            MyImageOrigin.Source = MyBitmapSource;
        }
    }
}