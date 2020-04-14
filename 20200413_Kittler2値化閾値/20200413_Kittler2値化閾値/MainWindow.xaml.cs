using System;
using System.Windows;


//0～7の値の配列を使ってKittlerの方法を使って閾値を求める
namespace _20200413_Kittler2値化閾値
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
        }

        //得られた閾値とヒストグラムをアプリに表示
        private void Hyouzi(int[] vs, int threshold)
        {
            TextBlockThreshold.Text = $"閾値＝{threshold}";
            string str;
            str = string.Join(", ", vs);
            TextBlock1.Text = str;
            int[] histogram = MakeHistogram0to7(vs);

            TextBlock2.Text = GetStr(histogram, 0, threshold);
            TextBlock3.Text = GetStr(histogram, threshold, 8);
        }
      //表示する文字列作成
        private string GetStr(int[] histogram, int begin, int end)
        {
            string str = "";
            for (int i = begin; i < end; i++)
            {
                for (int j = 0; j < histogram[i]; j++)
                {
                    str += i + ", ";
                }
                str += Environment.NewLine;
            }
            str = str.Remove(str.Length - 2);
            return str;
        }

        //Kittlerの方法で閾値を求める、0～7までの値の配列専用
        private int KittlerThreshold(int[] vs)
        {
            int[] histogram = MakeHistogram0to7(vs);
            double aError, bError;
            double min = double.MaxValue;
            int threshold = 1;//閾値
            for (int i = 1; i < 8; i++)
            {
                aError = KittlerSub(histogram, 0, i);//A範囲(しきい値未満)の誤差
                bError = KittlerSub(histogram, i, 8);//B範囲(しきい値以上)の誤差
                var E = aError + bError;
                //誤差の総量が最小になるインデックスを閾値にする
                if (E < min)
                {
                    min = E;
                    threshold = i;
                }
            }
            return threshold;
        }

        /// <summary>
        /// ヒストグラムの指定範囲の誤差を返す、誤差 = 要素の比率 * Log10(分散 / 要素の比率)
        /// </summary>
        /// <param name="histogram"></param>
        /// <param name="begin">範囲の開始点</param>
        /// <param name="end">範囲の終わり(未満なので、100指定なら99まで計算する)</param>
        /// <returns></returns>
        private double KittlerSub(int[] histogram, int begin, int end)
        {
            double varp = HistogramVariance(histogram, begin, end);//分散
            if (double.IsNaN(varp) || varp == 0)
                //分散が計算不能or0なら対象外になるように、大きな値(1.0)を返す
                return 1.0;
            else
            {
                int count = HistogramCount(histogram, 0, 7);//全要素数
                double ratio = HistogramCount(histogram, begin, end);
                ratio /= count;//画素数比率
                return ratio * Math.Log10(varp / ratio);
            }
        }

        /// <summary>
        /// ヒストグラムの指定範囲の分散を計算
        /// </summary>
        /// <param name="begin">範囲の始まり</param>
        /// <param name="end">範囲の終わり(未満なので、100指定なら99まで計算する)</param>
        /// <param name="count">範囲の画素数</param>
        /// <param name="average">範囲の平均値</param>
        /// <returns></returns>
        private double HistogramVariance(int[] histogram, int begin, int end)
        {
            long squareTotal = 0;
            long aveTotal = 0;
            long count = 0;//要素数
            for (long i = begin; i < end; i++)
            {
                squareTotal += i * i * histogram[i];//2乗の累計
                aveTotal += i * histogram[i];
                count += histogram[i];
            }
            //平均値
            double average = aveTotal / (double)count;
            //分散 = 2乗の平均 - 平均値の2乗
            return (squareTotal / (double)count) - (average * average);
        }

        /// <summary>
        /// ヒストグラムの指定範囲のピクセルの個数
        /// </summary>
        /// <param name="histogram">ヒストグラム</param>
        /// <param name="begin">範囲の始まり</param>
        /// <param name="end">範囲の終わり(未満なので、100指定なら99まで計算する)</param>
        /// <returns></returns>
        private int HistogramCount(int[] histogram, int begin, int end)
        {
            int count = 0;
            for (int i = begin; i < end; i++)
            {
                count += histogram[i];
            }
            return count;
        }

        //未使用
        /// <summary>
        /// ヒストグラムの指定範囲の平均値
        /// </summary>
        /// <param name="histogram">ヒストグラム</param>
        /// <param name="begin">範囲開始点</param>
        /// <param name="end">範囲終了点(未満なので、100指定なら99まで計算する)</param>
        /// <returns></returns>
        private double HistogramAverage(int[] histogram, int begin, int end)
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


        //配列からヒストグラムの配列作成
        private int[] MakeHistogram0to7(int[] vs)
        {
            int[] histogram = new int[8];
            for (int i = 0; i < vs.Length; i++)
            {
                histogram[vs[i]]++;
            }
            return histogram;
        }


        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            //0～7までの値の配列
            var vs = new int[] {
                3,  3,  3,  3,  3,
                3,  3,  3,  3,  3,
                5,  5,  5,  5,  5,
                4,  4,  4,  4,  4,
                2,  2,  2,  6,  1,
                5,  7,  0,  6,  1,};
            Hyouzi(vs, KittlerThreshold(vs));
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            int[] vs = new int[] {
                0,0,0,0,0,
                0,0,0,0,0,
                4,4,4,4,4,
                3,3,3,3,3,
                7,7,7,7,7,
                7,7,7,7,7 };
            Hyouzi(vs, KittlerThreshold(vs));
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            var vs = new int[] {
            7,  7,  7,  7,  7,
            7,  7,  7,  7,  7,
            7,  7,  7,  7,  7,
            6,  1,  7,  6,  4,
            5,  5,  4,  6,  5,
            3,  6,  2,  4,  3,
            };
            Hyouzi(vs, KittlerThreshold(vs));
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            var vs = new int[] {
            2,  2,  2,  2,  0,
            2,  2,  2,  2,  7,
            5,  5,  5,  5,  1,
            5,  5,  5,  5,  6,
            3,  3,  3,  1,  1,
            4,  4,  4,  6,  6,
            };
            Hyouzi(vs, KittlerThreshold(vs));
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            var vs = new int[] {
            0,  0,  0,  0,  0,
            1,  1,  1,  1,  1,
            2,  2,  2,  2,  2,
            3,  3,  3,  3,  3,
            4,  4,  4,  4,  4,
            5,  5,  5,  5,  5,
            };
            Hyouzi(vs, KittlerThreshold(vs));
        }

        private void ButtonRandom_Click(object sender, RoutedEventArgs e)
        {
            var r = new Random();
            var vs = new int[30];
            for (int i = 0; i < vs.Length; i++)
            {
                vs[i] = r.Next(8);
            }
            Hyouzi(vs, KittlerThreshold(vs));
        }
    }
}
