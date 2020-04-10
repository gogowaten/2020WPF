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
        }
        private void Test()
        {
            int[] vs = new int[] { 3, 4, 0, 0, 0, 1, 1, 4, 0, 1, 2, 2, 3, 0, 3, 3, 0, 0, 4, 4 };
            double average = vs.Average();

            var varp = Variance(vs, average);
            var inu = NormDist(1, average, varp);
            var neko = NormDist(2, average, varp);
            var menseki = (neko + inu) * 1 / 2.0;
            double begin1 = 1;
            double end1 = 2;
            var v = (NormDist(begin1, average, varp) + NormDist(end1, average, varp)) * ((end1 - begin1) / 2.0);

            decimal begin2 = 1.0m;
            decimal end2 = 2.0m;
            var vv = Menseki(begin2, end2, average, varp, 100m);
        }

        private decimal Menseki(decimal begin, decimal end, double average, double varp, decimal resolution)
        {
            decimal unit = (end - begin) / resolution;
            decimal total = 0m;

            for (decimal i = begin; i < end; i += unit)
            {
                total += NormDist2(i, i + unit, (decimal)average, varp) * (unit / 2m);
            }
            return total;
        }

        private decimal NormDist2(decimal x1, decimal x2, decimal average, double variance)
        {
            double d = 1 / Math.Sqrt(2 * Math.PI * variance);
            decimal er1 = -((x1 - average) * (x1 - average) / (2 * (decimal)variance));
            double e1 = Math.Pow(Math.Exp(1), (double)er1);
            decimal er2 = -((x2 - average) * (x2 - average) / (2 * (decimal)variance));
            double e2 = Math.Pow(Math.Exp(1), (double)er2);
            return (decimal)(d * e1) + (decimal)(d * e2);
        }

        /// <summary>
        /// 確率密度関数、xのときの確率を返す
        /// </summary>
        /// <param name="x"></param>
        /// <param name="average">平均</param>
        /// <param name="variance">分散</param>
        /// <returns></returns>
        private double NormDist(double x, double average, double variance)
        {
            double d = 1 / Math.Sqrt(2 * Math.PI * variance);
            double er = -((x - average) * (x - average) / (2 * variance));
            double e = Math.Pow(Math.Exp(1), er);
            return d * e;
        }


        //分散
        private double Variance(int[] vs, double average)
        {
            int total = 0;
            foreach (var item in vs)
            {
                total += item * item;
            }
            return ((double)total / vs.Length) - (average * average);
        }
    }
}
