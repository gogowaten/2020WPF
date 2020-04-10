using System;
using System.Linq;
using System.Windows;

//正規分布の面積

namespace _20200410_正規分布の面積_累積分布
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Test();
        }
        private void Test()
        {
            int[] vs = new int[] { 3, 4, 0, 0, 0, 1, 1, 4, 0, 1, 2, 2, 3, 0, 3, 3, 0, 0, 4, 4 };
            double average = vs.Average();
            var variance = Variance(vs, average);
            decimal begin = 1.0m;//区間開始点
            decimal end = 2.0m;//区間終了点
            decimal resolution = 1000m;
            var vv = CumulativeDistribution(begin, end, average, variance, resolution);
            var mm = MyCalc(vs, begin, end, resolution);
        }

        private decimal MyCalc(int[] specimen, decimal begin, decimal end, decimal resolution)
        {
            double average = specimen.Average();
            var variance = Variance(specimen, average);
            return CumulativeDistribution(begin, end, average, variance, resolution);
        }

        /// <summary>
        /// 累積分布関数
        /// </summary>
        /// <param name="begin">区間開始</param>
        /// <param name="end">区間終了</param>
        /// <param name="average">平均値</param>
        /// <param name="variance">分散(標準偏差^2)</param>
        /// <param name="resolution">計算精度、区間の分割数で1000あれば十分</param>
        /// <returns></returns>
        private decimal CumulativeDistribution(decimal begin, decimal end, double average, double variance, decimal resolution)
        {
            decimal unit = (end - begin) / resolution;
            decimal total = 0m;
            for (decimal i = begin; i < end; i += unit)
            {
                //台形面積 = (上底＋下底)＊高さ/2
                total += (ProbabilityDensity(i, average, variance) + ProbabilityDensity(i + unit, average, variance)) * unit / 2m;
            }           
            return total;
        }
      
        /// <summary>
        /// 確率密度関数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="average">平均</param>
        /// <param name="variance">分散(標準偏差^2)</param>
        /// <returns></returns>
        private decimal ProbabilityDensity(decimal x, double average, double variance)
        {
            if (variance == 0)
                return 0m;
            double xa = (double)x - average;
            double ei = -(xa * xa / (2 * variance));//expIndex
            double e = Math.Pow(Math.Exp(1), ei);
            double el = 1 / Math.Sqrt(2 * Math.PI * variance);//expLeft
            return (decimal)(el * e);            
        }


        //分散 = 2乗の平均 - 平均の2乗
        private double Variance(int[] vs, double average)
        {
            int total = 0;
            foreach (var item in vs)
            {
                total += item * item;
            }
            //2乗の平均 - 平均の2乗
            return ((double)total / vs.Length) - (average * average);
        }
    }
}
