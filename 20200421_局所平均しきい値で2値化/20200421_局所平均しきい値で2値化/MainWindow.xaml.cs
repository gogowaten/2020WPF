﻿using System;
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
using System.Windows.Media.Animation;

namespace _20200421_局所平均しきい値で2値化
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource MyBitmapSource;
        private byte[] MyPixels;


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

            //string imagePath;
            //imagePath = @"D:\ブログ用\チェック用2\WP_20200328_11_22_52_Pro_2020_03_28_午後わてん.jpg";
            ////imagePath = @"E:\オレ\携帯\2019スマホ\WP_20200328_11_22_52_Pro.jpg";
            //imagePath = @"D:\ブログ用\テスト用画像\grayScale.bmp";
            ////imagePath = @"D:\ブログ用\テスト用画像\grayscale256x256.png";
            ////imagePath = @"D:\ブログ用\テスト用画像\Michelangelo's_David_-_63_grijswaarden.bmp";
            ////imagePath = @"D:\ブログ用\テスト用画像\gray128.png";//0と255の中間みたい、pixelformats.blackwhiteだと市松模様になる
            ////imagePath = @"D:\ブログ用\テスト用画像\gray127.png";//これは中間じゃないみたい
            ////imagePath = @"D:\ブログ用\テスト用画像\gray250.png";
            ////imagePath = @"D:\ブログ用\テスト用画像\ﾈｺは見ている.png";
            ////imagePath = @"D:\ブログ用\テスト用画像\NEC_2097_.jpg";
            ////imagePath = @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん_256x192.png";

            //(MyPixels, MyBitmapSource) = MakeBitmapSourceAndPixelData(imagePath, PixelFormats.Gray8, 96, 96);
            //MyImage.Source = MyBitmapSource;
            //MyImageOrigin.Source = MyBitmapSource;



        }



        /// <summary>
        /// 2値化、注目ピクセルの周囲5x5ピクセルの平均をしきい値にして2値化
        /// </summary>
        /// <param name="source">PixelFormats.Gray8専用</param>
        /// <returns></returns>
        private BitmapSource LocalArea5x5Threshold(BitmapSource source)
        {
            //Bitmapから配列作成
            int w = source.PixelWidth;
            int h = source.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            source.CopyPixels(pixels, stride, 0);
            //2値化用配列
            byte[] result = new byte[pixels.Length];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    //注目ピクセルの上下左右2ピクセル(5x5)範囲の値合計
                    long total = 0;
                    int count = 0;
                    for (int i = -2; i <= 2; i++)
                    {
                        for (int j = -2; j <= 2; j++)
                        {
                            int yy = y + i;
                            int xx = x + j;
                            //画像の外になる座標は無視する
                            if (yy >= 0 && yy < h && xx >= 0 && xx < w)
                            {
                                total += pixels[yy * stride + xx];
                                count++;
                            }
                        }
                    }
                    //平均値をしきい値にして2値化
                    double threshold = total / (double)count;
                    int p = y * stride + x;
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }
            return BitmapSource.Create(w, h, 96, 96, PixelFormats.Gray8, null, result, stride);
        }






        /// <summary>
        /// 2値化、注目ピクセルの指定範囲の平均をしきい値にして2値化
        /// </summary>
        /// <param name="pixels">PixelFormats.Gray8専用</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <param name="near">範囲を1以上で指定、1なら上下左右1ピクセルで3x3マスの範囲になる、2なら5x5</param>
        /// <returns></returns>
        private BitmapSource LocalThreshold(byte[] pixels, int width, int height, int stride, int near)
        {
            //局所範囲のピクセル数
            int count;
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    long total = 0;
                    count = 0;
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
                                    count++;
                                }
                            }
                        }
                    }

                    //局所範囲の平均値をしきい値にして2値化
                    double threshold = total / (double)count;
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }
            return MakeBitmapSource(result, width, height, stride);
        }

        private BitmapSource LocalThresholdMulti(byte[] pixels, int width, int height, int stride, int near)
        {
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    long total = 0;
                    int count = 0;//局所範囲のピクセル数
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
                                    count++;
                                }
                            }
                        }
                    }

                    //局所範囲の平均値をしきい値にして2値化
                    double threshold = total / (double)count;
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            });

            return MakeBitmapSource(result, width, height, stride);
        }


        //局所範囲の合計は差分で計算、画像周縁部は計算しない
        private BitmapSource LocalThreshold差分計算中央部だけ(byte[] pixels, int width, int height, int stride, int near)
        {
            //局所範囲のピクセル数
            int localAreaLength = (near * 2 + 1) * (near * 2 + 1);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];

            //2値化
            for (int y = near; y < height - near; y++)
            {
                //持ち越しする合計値
                long motikosiTotal = 0;
                //一番左側はここで計算
                for (int my = -near; my <= near; my++)
                {
                    for (int mx = 0; mx < near * 2; mx++)
                    {
                        motikosiTotal += pixels[(y + my) * stride + mx];
                    }
                }

                for (int x = near; x < width - near; x++)
                {
                    long totalNew = 0;//右列合計値
                    long totalOld = 0;//左列合計値
                    for (int yy = -near; yy <= near; yy++)
                    {
                        totalNew += pixels[(y + yy) * stride + x + near];
                        totalOld += pixels[(y + yy) * stride + x - near];
                    }
                    //局所範囲合計 = 持ち越し + 右列合計値
                    long totalAll = motikosiTotal + totalNew;

                    //局所範囲の平均値をしきい値にする
                    double threshold = totalAll / (double)localAreaLength;

                    //しきい値で2値化                    
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;

                    //持ち越し合計 = 局所範囲合計 - 右列合計値
                    motikosiTotal = totalAll - totalOld;
                }
            }
            return MakeBitmapSource(result, width, height, stride);
        }

        //局所範囲の合計は差分で計算、画像周縁部は別計算する
        private BitmapSource LocalThreshold差分計算(byte[] pixels, int width, int height, int stride, int near)
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


            return MakeBitmapSource(result, width, height, stride);
        }

        private BitmapSource LocalThreshold差分計算Multi(byte[] pixels, int width, int height, int stride, int near)
        {
            //局所範囲のピクセル数
            int localAreaLength = (near * 2 + 1) * (near * 2 + 1);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];

            //中央部の2値化
            Parallel.For(near, height - near, y =>
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
            });

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
            return MakeBitmapSource(result, width, height, stride);
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

        private BitmapSource LocalThreshold差分計算Multi改(byte[] pixels, int width, int height, int stride, int near)
        {
            //局所範囲のピクセル数
            int localAreaLength = (near * 2 + 1) * (near * 2 + 1);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];

            //中央部の2値化
            Parallel.For(near, height - near, y =>
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
            });

            //画像周縁部の2値化
            //左側            
            SetBinaryArea(0, near, 0, height, pixels, near, width, height, stride, result);
            //右側            
            SetBinaryArea(width - near, width, 0, height, pixels, near, width, height, stride, result);
            //上側
            SetBinaryArea(0, width, 0, near, pixels, near, width, height, stride, result);
            //下側
            SetBinaryArea(0, width, height - near, height, pixels, near, width, height, stride, result);

            return MakeBitmapSource(result, width, height, stride);
        }

        //指定範囲を2値化する
        private void SetBinaryArea(int xBegin, int xEnd, int yBegin, int yEnd, byte[] pixels, int near, int width, int height, int stride, byte[] result)
        {
            Parallel.For(yBegin, yEnd, y =>
            {
                for (int x = xBegin; x < xEnd; x++)
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
            });
        }



        private BitmapSource MakeBitmapSource(byte[] pixels, int width, int height, int stride)
        {
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
            if (MyImage.Source == null) return;
            Clipboard.SetImage((BitmapSource)MyImage.Source);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Button1.Content = nameof(LocalArea5x5Threshold);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = LocalArea5x5Threshold(MyBitmapSource);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Button2.Content = nameof(LocalThreshold);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = LocalThreshold(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, (int)ScrollBarLocalArea.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Button3.Content = nameof(LocalThresholdMulti);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = LocalThresholdMulti(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, (int)ScrollBarLocalArea.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }


        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Button4.Content = nameof(LocalThreshold差分計算中央部だけ);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = LocalThreshold差分計算中央部だけ(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, (int)ScrollBarLocalArea.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Button5.Content = nameof(LocalThreshold差分計算);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = LocalThreshold差分計算(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, (int)ScrollBarLocalArea.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Button6.Content = nameof(LocalThreshold差分計算Multi);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = LocalThreshold差分計算Multi(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, (int)ScrollBarLocalArea.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Button7.Content = nameof(LocalThreshold差分計算Multi改);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = LocalThreshold差分計算Multi改(MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth, (int)ScrollBarLocalArea.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        //private void Button8_Click(object sender, RoutedEventArgs e)
        //{

        //}



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

        private void ScrollBarLocalArea_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var sb = (System.Windows.Controls.Primitives.ScrollBar)sender;
            if (e.Delta > 0)
                sb.Value += sb.LargeChange;
            else
                sb.Value -= sb.LargeChange;
        }
    }
}