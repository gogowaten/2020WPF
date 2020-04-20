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
            //int[,] vs = new int[5,5];
            //for (int i = 0; i < 5; i++)
            //{
            //    for (int j = 0; j < 5; j++)
            //    {
            //        vs[i, j] = i * j;
            //    }
            //}
            //for (int i = 0; i < 5; i++)
            //{
            //    vs[i, 0] = 100;
            //}


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

        private double GetThresholdNiblack(byte[] localArea, int effectiveNumber, double niblack)
        {
            double threshold;
            (double stdev, double average) vv = StdevAndAverage(localArea, effectiveNumber);
            if (vv.stdev != 0)
                threshold = vv.average + (niblack * vv.stdev);
            else
                threshold = 127.5;
            return threshold;
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
            //局所範囲のピクセル数
            int localAreaLength = (near * 2 + 1) * (near * 2 + 1);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //係数-0.2が丁度いいらしい
            double NiblackK = ScrollBarNiblack.Value;
            //2値化
            Parallel.For(0, height, y =>
            {
                byte[] localArea = new byte[localAreaLength];
                for (int x = 0; x < width; x++)
                {
                    //周囲ピクセルの位置が画像範囲内のものだけの値を入れる用
                    int effectiveIndex = 0;//周囲ピクセルのカウント用、画像範囲外はカウントしない
                    //double threshold = 0;// 127.5;//標準しきい値

                    //注目ピクセルの周囲(局所範囲)の値を配列に集める
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


                    //局所範囲の標準偏差と平均値を取得
                    (double stdev, double average) vv = StdevAndAverage(localArea, effectiveIndex);
                    //しきい値計算は標準偏差が0以外ならって条件をつけたけど、これはないほうがいいかも？
                    //if (vv.stdev != 0)
                    double threshold = vv.average + (NiblackK * vv.stdev);

                    //しきい値で2値化
                    int p;
                    p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            });
            //for (int y = 0; y < height; y++)
            //{


            //}
            return result;
        }


        //配列の標準偏差と平均値
        private (double stdev, double average) StdevAndAverage(byte[] vs, int effectiveNumber)
        {
            long aveTotal = 0;
            long squareToral = 0;//2乗の合計
            for (int i = 0; i < effectiveNumber; i++)
            {
                aveTotal += vs[i];
                squareToral += vs[i] * vs[i];
            }
            double average = aveTotal / (double)effectiveNumber;
            //標準偏差 = 2乗の平均 - 平均の2乗
            return (Math.Sqrt((squareToral / (double)effectiveNumber) - (average * average)), average);
        }

        private byte[] LocalThresholdAverage(byte[] pixels, int width, int height, int stride, int near)
        {
            //局所範囲のピクセル数
            int localAreaLength = (near * 2 + 1) * (near * 2 + 1);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            for (int y = 0; y < height; y++)
            {

                //周囲ピクセルの位置が画像範囲内のものだけの値を入れる用
                byte[] localArea = new byte[localAreaLength];
                int effectiveIndex;//周囲ピクセルのカウント用、画像範囲外はカウントしない

                for (int x = 0; x < width; x++)
                {
                    effectiveIndex = 0;
                    //注目ピクセルの周囲(局所範囲)の値を配列に集める
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


                    //局所範囲の平均値をしきい値にする
                    double threshold = Average(localArea, effectiveIndex);

                    //しきい値で2値化                    
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }
            return result;
        }

        //配列の平均値
        private double Average(byte[] vs, int effectiveNumber)
        {
            long aveTotal = 0;
            for (int i = 0; i < effectiveNumber; i++)
            {
                aveTotal += vs[i];
            }
            return aveTotal / (double)effectiveNumber;
        }

        //直に計算
        private byte[] LocalThresholdAverage2(byte[] pixels, int width, int height, int stride, int near)
        {
            //局所範囲のピクセル数
            int effectiveNumber;
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    long total = 0;
                    effectiveNumber = 0;
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
                                    total += pixels[(y + i) * stride + x + j];
                                    effectiveNumber++;
                                }
                            }
                        }
                    }

                    //局所範囲の平均値をしきい値にする
                    double threshold = total / (double)effectiveNumber;

                    //しきい値で2値化                    
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }
            return result;
        }


        //局所範囲の合計は差分で計算、周辺は計算しない
        private byte[] LocalThresholdAverage3(byte[] pixels, int width, int height, int stride, int near)
        {
            //局所範囲のピクセル数
            int localAreaLength = (near * 2 + 1) * (near * 2 + 1);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];

            //2値化
            for (int y = near; y < height - near; y++)
            {
                long motikosiTotal = 0;
                for (int my = -near; my <= near; my++)
                {
                    for (int mx = 0; mx < near * 2; mx++)
                    {
                        motikosiTotal += pixels[(y + my) * stride + mx];
                    }
                }

                for (int x = near; x < width - near; x++)
                {
                    long totalNew = 0;
                    long totalOld = 0;
                    for (int yy = -near; yy <= near; yy++)
                    {
                        totalNew += pixels[(y + yy) * stride + x + near];
                        totalOld += pixels[(y + yy) * stride + x - near];
                    }
                    long totalAll = motikosiTotal + totalNew;

                    //局所範囲の平均値をしきい値にする
                    double threshold = totalAll / (double)localAreaLength;

                    //しきい値で2値化                    
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;

                    //
                    motikosiTotal = totalAll - totalOld;
                }
            }
            return result;
        }

        //局所範囲の合計は差分で計算、画像周縁部は別計算する
        private byte[] LocalThresholdAverage4(byte[] pixels, int width, int height, int stride, int near)
        {
            //局所範囲のピクセル数
            int localAreaLength = (near * 2 + 1) * (near * 2 + 1);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];

            //中央部の2値化
            for (int y = near; y < height - near; y++)
            {
                long motikosiTotal = 0;
                for (int my = -near; my <= near; my++)
                {
                    for (int mx = 0; mx < near * 2; mx++)
                    {
                        motikosiTotal += pixels[(y + my) * stride + mx];
                    }
                }

                for (int x = near; x < width - near; x++)
                {
                    long totalNew = 0;
                    long totalOld = 0;
                    for (int yy = -near; yy <= near; yy++)
                    {
                        totalNew += pixels[(y + yy) * stride + x + near];
                        totalOld += pixels[(y + yy) * stride + x - near];
                    }
                    long totalAll = motikosiTotal + totalNew;

                    //局所範囲の平均値をしきい値にする
                    double threshold = totalAll / (double)localAreaLength;

                    //しきい値で2値化                    
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;

                    //
                    motikosiTotal = totalAll - totalOld;
                }
            }

            //画像周縁部の2値化
            //左側
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < near; x++)//左側
                {
                    //局所範囲の平均値をしきい値にする
                    double threshold = AroundAverage(pixels, x, y, near, width, height, stride);

                    //しきい値で2値化                    
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }

            //右側
            for (int y = 0; y < height; y++)
            {
                for (int x = width - near; x < width; x++)
                {
                    //局所範囲の平均値をしきい値にする
                    double threshold = AroundAverage(pixels, x, y, near, width, height, stride);

                    //しきい値で2値化                    
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }

            //上側
            for (int y = 0; y < near; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //局所範囲の平均値をしきい値にする
                    double threshold = AroundAverage(pixels, x, y, near, width, height, stride);

                    //しきい値で2値化                    
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }

            //下側
            for (int y = height - near; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //局所範囲の平均値をしきい値にする
                    double threshold = AroundAverage(pixels, x, y, near, width, height, stride);
                    //しきい値で2値化                    
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }


            return result;
        }
        //
        /// <summary>
        /// 指定座標の周縁部の平均値を返す
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="x">指定座標x</param>
        /// <param name="y">指定座標y</param>
        /// <param name="near">周縁部の広さ、1以上を指定、1なら上下左右1マスづつで3x3の範囲、2指定は5x5</param>
        /// <param name="width">画像の横ピクセル数</param>
        /// <param name="height">画像の縦ピクセル数</param>
        /// <param name="stride"></param>
        /// <returns></returns>
        private double AroundAverage(byte[] pixels, int x, int y, int near, int width, int height, int stride)
        {
            long total = 0;
            int effectiveNumber = 0;
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
                            total += pixels[(y + i) * stride + x + j];
                            effectiveNumber++;
                        }
                    }
                }
            }
            return total / (double)effectiveNumber;
        }


        //注目ピクセルの周囲(局所範囲)の値を配列に集める
        private byte[] GetLocalValue(byte[] pixels, int y, int x, int stride, int near)
        {
            byte[] localArea = new byte[(near * 2 + 1) * (near * 2 + 1)];
            int index = 0;

            for (int i = -near; i <= near; i++)
            {
                int yy = y + i;
                for (int j = -near; j <= near; j++)
                {
                    int xx = x + j;
                    localArea[index] = pixels[(yy * stride) + xx];
                    index++;
                }
            }
            return localArea;
        }
        //左端の1列の値を返す
        private byte[] GetLeftColumnValues(byte[] vs, int near)
        {
            byte[] result = new byte[near];
            int index = 0;
            for (int i = 0; i < vs.Length; i += near)
            {
                result[index] = vs[i];
                index++;
            }
            return result;
        }
        private byte[] GetLeftColumnValues(byte[] vs, int near, int p, int y, int x, int stride)
        {
            byte[] result = new byte[near];
            int index = 0;
            int yy;
            for (int i = 0; i < near; i++)
            {
                yy = p + (i * stride);
                result[index] = vs[yy];
                index++;
            }
            return result;
        }
        private double GetArrayAverage(byte[] vs)
        {
            long total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total / (double)vs.Length;
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
            byte[] pixels = Niblack2(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, (int)ScrollBarNiblackNear.Value);
            MyImage.Source = MakeBitmapSource(pixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }


        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            byte[] pixels = LocalThresholdAverage(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, (int)ScrollBarNiblackNear.Value);
            MyImage.Source = MakeBitmapSource(pixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            byte[] pixels = LocalThresholdAverage2(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, (int)ScrollBarNiblackNear.Value);
            MyImage.Source = MakeBitmapSource(pixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            byte[] pixels = LocalThresholdAverage3(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, (int)ScrollBarNiblackNear.Value);
            MyImage.Source = MakeBitmapSource(pixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            byte[] pixels = LocalThresholdAverage4(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, (int)ScrollBarNiblackNear.Value);
            MyImage.Source = MakeBitmapSource(pixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

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