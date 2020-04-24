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

//Auto Local Threshold - ImageJ
//https://imagej.net/Auto_Local_Threshold#Phansalkar


namespace _20200422_局所しきい値で2値化
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
            imagePath = @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん_256x192.png";

            (MyPixels, MyBitmapSource) = MakeBitmapSourceAndPixelData(imagePath, PixelFormats.Gray8, 96, 96);
            MyImage.Source = MyBitmapSource;
            MyImageOrigin.Source = MyBitmapSource;



        }



        private BitmapSource MakeBitmapSource(byte[] pixels, int width, int height, int stride)
        {
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
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
        private BitmapSource LocalAverage(BitmapSource bitmap, int near, double C = 0)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            Parallel.For(0, h, y =>
              {
                  for (int x = 0; x < w; x++)
                  {
                      long total = 0;
                      //局所範囲のピクセル数
                      int count = 0;
                      for (int i = -near; i <= near; i++)
                      {
                          int yy = y + i;
                          if (yy >= 0 && yy < h)//y座標有効判定
                          {
                              for (int j = -near; j <= near; j++)
                              {
                                  int xx = x + j;
                                  if (xx >= 0 && xx < w)//x座標有効判定
                                  {
                                      total += pixels[(yy * stride) + xx];
                                      count++;
                                  }
                              }
                          }
                      }

                      //局所範囲の平均値をしきい値にして2値化
                      double threshold = total / (double)count - C;
                      int p = (y * stride) + x;//注目ピクセルのインデックス
                      if (pixels[p] < threshold)
                          result[p] = 0;
                      else
                          result[p] = 255;
                  }
              });
            return MakeBitmapSource(result, w, h, stride);
        }

        private BitmapSource Niblack(BitmapSource bitmap, int near, double k)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            Parallel.For(0, h, y =>
            {
                for (int x = 0; x < w; x++)
                {
                    long total = 0;
                    long squareTotal = 0;//2乗の合計
                    int count = 0;//局所範囲のピクセル数
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    int v = pixels[(yy * stride) + xx];
                                    total += v;
                                    squareTotal += v * v;
                                    count++;
                                }
                            }
                        }
                    }

                    //局所範囲の 平均値 + (係数k * 標準偏差) をしきい値にして2値化
                    double average = total / (double)count;
                    //double stdev = Math.Sqrt( squareTotal / (average * average));
                    double stdev = Math.Sqrt((squareTotal / (double)count) - (average * average));
                    double threshold = average + (k * stdev);
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            });
            return MakeBitmapSource(result, w, h, stride);
        }

        private BitmapSource Sauvola(BitmapSource bitmap, int near, double kValue, int rValue)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            Parallel.For(0, h, y =>
            {
                for (int x = 0; x < w; x++)
                {
                    long total = 0;
                    long squareTotal = 0;//2乗の合計
                    //局所範囲のピクセル数
                    int count = 0;
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    int v = pixels[(yy * stride) + xx];
                                    total += v;
                                    squareTotal += v * v;
                                    count++;
                                }
                            }
                        }
                    }

                    double average = total / (double)count;
                    double variance = (squareTotal / (double)count) - (average * average);
                    double stdev = Math.Sqrt(variance);
                    //平均 * (1 + k * (標準偏差 / r - 1))
                    double threshold = average * (1 + kValue * (stdev / rValue - 1));

                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            });
            return MakeBitmapSource(result, w, h, stride);
        }

        //        opencvsharp/Binarizer.cs at master · shimat/opencvsharp
        //https://github.com/shimat/opencvsharp/blob/master/src/OpenCvSharp.Extensions/Binarizer.cs

        //MinMaxの中間をしきい値にする
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="near"></param>
        /// <param name="contrastMin">15</param>
        /// <param name="bgThreshold">128</param>
        /// <returns></returns>
        private BitmapSource Bernsen(BitmapSource bitmap, int near, byte contrastMin, byte bgThreshold)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            Parallel.For(0, h, y =>
            {
                for (int x = 0; x < w; x++)
                {
                    byte min = byte.MaxValue;
                    byte max = byte.MinValue;
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    byte v = pixels[(yy * stride) + xx];
                                    if (v < min) min = v;
                                    if (v > max) max = v;
                                }
                            }
                        }
                    }
                    double threshold;
                    int contrast = max - min;
                    if (contrast < contrastMin)
                        //15未満の時
                        threshold = bgThreshold;
                    else
                        threshold = (max + min) / 2.0;


                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            });
            return MakeBitmapSource(result, w, h, stride);
        }

        private BitmapSource Bernsen2(BitmapSource bitmap, int near)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte min = byte.MaxValue;
                    byte max = byte.MinValue;
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    byte v = pixels[(yy * stride) + xx];
                                    if (v < min) min = v;
                                    if (v > max) max = v;
                                }
                            }
                        }
                    }

                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    byte pv = pixels[p];
                    double mid = (max + min) / 2.0;
                    if (max - min < 15)
                    {
                        result[p] = mid >= 128 ? (byte)255 : (byte)0;
                    }
                    else
                    {
                        result[p] = pv >= mid ? (byte)255 : (byte)0;
                    }
                }
            }
            return MakeBitmapSource(result, w, h, stride);
        }


        //MinMaxの中間をしきい値にする
        private BitmapSource MidGray(BitmapSource bitmap, int near, double c = 0.0)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            Parallel.For(0, h, y =>
            {
                for (int x = 0; x < w; x++)
                {
                    byte min = byte.MaxValue;
                    byte max = byte.MinValue;
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    byte v = pixels[(yy * stride) + xx];
                                    if (v < min) min = v;
                                    if (v > max) max = v;
                                }
                            }
                        }
                    }
                    double threshold = (max + min) / 2.0 - c;
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            });
            return MakeBitmapSource(result, w, h, stride);
        }


        //MinMaxの差をしきい値にする
        private BitmapSource Contrast(BitmapSource bitmap, int near)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            Parallel.For(0, h, y =>
            {
                for (int x = 0; x < w; x++)
                {
                    byte min = byte.MaxValue;
                    byte max = byte.MinValue;
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    byte v = pixels[(yy) * stride + xx];
                                    if (v < min) min = v;
                                    if (v > max) max = v;
                                }
                            }
                        }
                    }
                    double threshold = max - min;
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            });
            return MakeBitmapSource(result, w, h, stride);
        }


        //Medianの差をしきい値にする
        private BitmapSource Median(BitmapSource bitmap, int near, double c = 0)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            Parallel.For(0, h, y =>
            {
                byte[] window = new byte[(near * 2 + 1) * (near * 2 + 2)];

                for (int x = 0; x < w; x++)
                {
                    int count = 0;
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    byte v = pixels[(yy * stride) + xx];
                                    window[count] = v;
                                    count++;
                                }
                            }
                        }
                    }
                    Array.Sort(window);
                    double threshold = GetMedian(window, count) - c;
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            });
            //for (int y = 0; y < h; y++)
            //{

            //}
            return MakeBitmapSource(result, w, h, stride);
        }
        private double GetMedian(byte[] vs, int length)
        {
            int index = length / 2;
            if (length % 2 == 1)
                return vs[index];
            else
            {
                return (vs[index - 1] + vs[index]) / 2.0;
            }
        }

        //わからん、どこか間違っている→しきい値計算のとき、平均値に掛けるじゃなくて足すだと思う
        //しきい値 = 平均値 * (1 + p * exp(-q * 平均値) + k * ((標準偏差 / r) -1))、これだと真っ黒になる
        //しきい値 = 平均値 + (1 + p * exp(-q * 平均値) + k * ((標準偏差 / r) -1))、これだと期待通りになる
        //k = 0.25, r = 0.5, p = 2, q = 10、pとqは固定でkとrで調整
        private BitmapSource Phansalkar(BitmapSource bitmap, int near, double k, double r)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //局所範囲のピクセル数
            int count;
            double p = 2.0;
            double q = 10.0;
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    long total = 0;
                    long squareTotal = 0;//2乗の合計
                    count = 0;
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    int v = pixels[(y + i) * stride + x + j];
                                    total += v;
                                    squareTotal += v * v;
                                    count++;
                                }
                            }
                        }
                    }

                    double average = total / (double)count;
                    double stdev = Math.Sqrt((squareTotal / (double)count) - (average * average));
                    //k = 0.25, r = 0.5, p = 2, q = 10
                    //average * (1 + p * exp(-q * average) + k * ((stdev / r) -1))
                    //k = 1; r = 2;
                    //double threshold = average * (1.0 + p * Math.Exp(-q * average) + k * ((stdev / r) - 1.0));
                    double threshold = average + (1.0 + p * Math.Exp(-q * average) + k * ((stdev / r) - 1.0));
                    //double threshold = average * (1 + Math.Pow(p, Math.Exp(-q * average)) + (k * ((stdev / r) - 1)));
                    //double threshold = average * (1 + Math.Pow(p, -Math.Exp(q * average)) + (k * ((stdev / r) - 1)));
                    int pIndex = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[pIndex] < threshold)
                        result[pIndex] = 0;
                    else
                        result[pIndex] = 255;
                }
            }
            return MakeBitmapSource(result, w, h, stride);
        }

        //大津の2値化、時間かかる
        private BitmapSource Ootu(BitmapSource bitmap, int near)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化

            Parallel.For(0, h, y =>
            {
                int count;
                byte[] window = new byte[(near * 2 + 1) * (near * 2 + 2)];
                for (int x = 0; x < w; x++)
                {
                    count = 0;
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    //局所の値を配列に入れる
                                    byte v = pixels[(yy * stride) + xx];
                                    window[count] = v;
                                    count++;
                                }
                            }
                        }
                    }
                    //局所の値の配列から大津の方法でしきい値計算
                    double threshold = GetThresholdOotu(window, count);
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            });

            return MakeBitmapSource(result, w, h, stride);
        }
        private int GetThresholdOotu(byte[] window, int count)
        {
            double allCount = count;
            double max = double.MinValue;
            int threshold = 0;
            //しきい値を1～255まで全て試して、AB両範囲の分離度が最大になるところを探す
            //分離度x = A画素割合 * B画素割合 * (A平均 - B平均) ^ 2
            //ここをパラレルにしても速くならない
            for (int i = 1; i < 256; i++)
            {
                int aCount = 0;
                int bCount = 0;
                int aTotal = 0;
                int bTotal = 0;
                for (int j = 0; j < count; j++)
                {
                    byte v = window[j];
                    if (v < i)
                    {
                        aTotal += v;
                        aCount++;
                    }
                    else
                    {
                        bTotal += v;
                        bCount++;
                    }
                }
                double aRatio = aCount / allCount;
                double bRatio = bCount / allCount;
                double aAverage = aTotal / allCount;
                double bAverage = bTotal / allCount;
                double X = aRatio * bRatio * ((aAverage - bAverage) * (aAverage - bAverage));
                if (X > max)
                {
                    max = X;
                    threshold = i;
                }
            }
            return threshold;
        }


        //        PowerPoint Presentation
        //https://ocw.kyoto-u.ac.jp/ja/09-faculty-of-engineering-jp/image-processing/pdf/dip_04.pdf
        //平均値制限法

        private BitmapSource LimitedAverage(BitmapSource bitmap, int near,double k,double gamma)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double count = 0;
                    int total = 0;
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    byte v = pixels[(yy * stride) + xx];
                                    total += v;
                                    count++;
                                }
                            }
                        }
                    }
                    double threshold;
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    double average = total / count;
                    //double gamma = 0.1;
                    //k=1~10
                    threshold = gamma * (k - 1) + (1 - 2 * gamma) * average;

                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }
            return MakeBitmapSource(result, w, h, stride);
        }



        //        PowerPoint Presentation
        //https://ocw.kyoto-u.ac.jp/ja/09-faculty-of-engineering-jp/image-processing/pdf/dip_04.pdf
        //平均値制限法

        private BitmapSource LocalThresholdTest2(BitmapSource bitmap, int near)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte min = byte.MaxValue;
                    byte max = byte.MinValue;
                    double count = 0;
                    int total = 0;
                    long squareTotal = 0;
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    byte v = pixels[(yy * stride) + xx];
                                    if (v < min) min = v;
                                    if (v > max) max = v;
                                    total += v;
                                    squareTotal += v * v;
                                    count++;
                                }
                            }
                        }
                    }
                    double threshold;
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    byte pv = pixels[p];
                    double average = total / count;
                    double varp = (squareTotal / count) - (average * average);
                    int mid = max - min;
                    double r = 0.1;
                    threshold = r * (10 - 1) + (1 - 2 * r) * average;
                    
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }
            return MakeBitmapSource(result, w, h, stride);
        }










        #region 失敗、どこか間違っている
        //大津の2値化でヒストグラム使ってみたけど違う、結果も違うし処理時間10倍
        private BitmapSource LocalThresholdOotu2(BitmapSource bitmap, int near)
        {
            //Bitmapから配列作成
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            //2値に置き換えた用
            byte[] result = new byte[pixels.Length];
            //2値化
            int[] histogram = new int[256];
            for (int y = 0; y < h; y++)
            {
                int count;
                Array.Clear(histogram, 0, 256);
                for (int x = 0; x < w; x++)
                {
                    count = 0;
                    for (int i = -near; i <= near; i++)
                    {
                        int yy = y + i;
                        if (yy >= 0 && yy < h)//y座標有効判定
                        {
                            for (int j = -near; j <= near; j++)
                            {
                                int xx = x + j;
                                if (xx >= 0 && xx < w)//x座標有効判定
                                {
                                    byte v = pixels[(yy * stride) + xx];
                                    histogram[v]++;
                                    count++;
                                }
                            }
                        }
                    }

                    double threshold = GetThresholdOotu3(histogram, count);
                    int p = (y * stride) + x;//注目ピクセルのインデックス
                    if (pixels[p] < threshold)
                        result[p] = 0;
                    else
                        result[p] = 255;
                }
            }
            //Parallel.For(0, h, y =>
            //{

            //});

            return MakeBitmapSource(result, w, h, stride);
        }
        private int GetThresholdOotu2(int[] histogram, int count)
        {
            double allCount = count;
            double max = double.MinValue;
            int threshold = 0;
            for (int i = 1; i < 256; i++)
            {
                int aCount = 0;
                int bCount = 0;
                int aTotal = 0;
                int bTotal = 0;
                for (int j = 0; j < i; j++)
                {
                    aCount += histogram[j];
                    aTotal += histogram[j] * j;
                }
                for (int j = i; j < 256; j++)
                {
                    bCount += histogram[j];
                    bTotal += histogram[j] * j;
                }
                double aRatio = aCount / allCount;
                double bRatio = bCount / allCount;
                double aAverage = aTotal / allCount;
                double bAverage = bTotal / allCount;
                double X = aRatio * bRatio * ((aAverage - bAverage) * (aAverage - bAverage));
                if (X > max)
                {
                    max = X;
                    threshold = i;
                }
            }
            return threshold;
        }
        private int GetThresholdOotu3(int[] histogram, int count)
        {
            double allCount = count;
            double max = double.MinValue;
            int threshold = 0;
            for (int i = 1; i < 256; i++)
            {
                int aCount = CountHistogram(histogram, 0, i);
                int bCount = CountHistogram(histogram, i, 256);
                double aRatio = aCount / allCount;
                double bRatio = bCount / allCount;
                double aAverage = AverageHistogram(histogram, 0, i);
                double bAverage = AverageHistogram(histogram, i, 256);
                double X = aRatio * bRatio * ((aAverage - bAverage) * (aAverage - bAverage));
                if (X > max)
                {
                    max = X;
                    threshold = i;
                }
            }
            return threshold;
        }

        private double AverageHistogram(int[] histogram, int begin, int end)
        {
            long total = 0;
            long count = 0;
            for (int i = begin; i < end; i++)
            {
                total += i * histogram[i];
                count += histogram[i];
            }
            return total / (double)count;
        }
        private int CountHistogram(int[] histogram, int begin, int end)
        {
            int count = 0;
            for (int i = begin; i < end; i++)
            {
                count += histogram[i];
            }
            return count;
        }

        private int[] MakeHistogram(byte[] vs, int count)
        {
            int[] histogram = new int[256];
            for (int i = 0; i < count; i++)
            {
                histogram[vs[i]]++;
            }
            return histogram;
        }
        #endregion


        #region 局所範囲の計算の高速化はめんどくさいので使っていない
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

        #endregion 局所範囲の計算の高速化はめんどくさいので使っていない













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

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Clipboard.SetImage((BitmapSource)MyImage.Source);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Button1.Content = nameof(Phansalkar);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = Phansalkar(MyBitmapSource, (int)ScrollBarLocalArea.Value, ScrollBarPhansalkerK.Value, ScrollBarPhansalkerR.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Button2.Content = nameof(LocalAverage);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = LocalAverage(MyBitmapSource, (int)ScrollBarLocalArea.Value, (int)ScrollBarAverage.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            Button3.Content = nameof(Niblack);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = Niblack(MyBitmapSource, (int)ScrollBarLocalArea.Value, ScrollBarNiblack.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }


        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            Button4.Content = nameof(Sauvola);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = Sauvola(MyBitmapSource, (int)ScrollBarLocalArea.Value, ScrollBarSauvolaK.Value, (int)ScrollBarSauvolaR.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            Button5.Content = nameof(Bernsen);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = Bernsen(MyBitmapSource, (int)ScrollBarLocalArea.Value, 15, 128);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            Button6.Content = nameof(MidGray);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = MidGray(MyBitmapSource, (int)ScrollBarLocalArea.Value, ScrollBarMidGray.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            Button7.Content = nameof(Contrast);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = Contrast(MyBitmapSource, (int)ScrollBarLocalArea.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            Button8.Content = nameof(Median);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = Median(MyBitmapSource, (int)ScrollBarLocalArea.Value, ScrollBarMedian.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }


        private void Button9_Click(object sender, RoutedEventArgs e)
        {
            int near = (int)ScrollBarLocalArea.Value;
            long keisanryou = (near * 2 + 1) * (near * 2 + 1) * MyBitmapSource.PixelWidth * MyBitmapSource.PixelHeight;
            if (keisanryou > 50_000_000)
            {
                double zikan = keisanryou / (double)10_000_000;
                if (MessageBoxResult.Cancel == MessageBox.Show($"処理完了まで{zikan:F1}秒以上かかるかも", "caption", MessageBoxButton.OKCancel))
                    return;
            }

            Button9.Content = nameof(Ootu);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = Ootu(MyBitmapSource, (int)ScrollBarLocalArea.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }

        private void Button10_Click(object sender, RoutedEventArgs e)
        {
            Button10.Content = nameof(Bernsen2);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = Bernsen2(MyBitmapSource, (int)ScrollBarLocalArea.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }


        private void Button11_Click(object sender, RoutedEventArgs e)
        {
            Button11.Content = nameof(LimitedAverage);
            var sw = new Stopwatch();
            sw.Start();
            MyImage.Source = LimitedAverage(MyBitmapSource, (int)ScrollBarLocalArea.Value,ScrollBarLimitedAverageK.Value,ScrollBarLimitedAverageG.Value);
            sw.Stop();
            TextBlockTime.Text = $"{sw.Elapsed.TotalSeconds:F3}秒";
        }
    }
}