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
using System.Numerics;
using System.Diagnostics;


namespace _20200204_int型配列の値の合計
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int[] MyAry;

        public MainWindow()
        {
            InitializeComponent();

            MyAry = Enumerable.Range(1, 1_000_000).ToArray();
            var neko = Test2(MyAry);
        }

        private long Test1(int[] iAry)
        {
            long total = 0;
            for (int i = 0; i < iAry.Length; i++)
            {
                total += iAry[i];
            }
            return total;
        }
        private long Test11(int[] iAry)
        {
            long total = 0;
            foreach (var item in iAry)
            {
                total += item;
            }
            return total;
        }

        //Linq
//        LINQの処理中に使うメモリを節約するには？［C#、VB］：.NET TIPS - ＠IT
//https://www.atmarkit.co.jp/ait/articles/1409/24/news105.html
        private long Test2(int[] iAry)
        {
            return iAry.Sum((int i) => { return (long)i; });
            //return iAry.Sum()//オーバーフロー            
        }

        private long Test3(int[] iAry)
        {
            var en = iAry.GetEnumerator();
            iAry.AsSpan(9,8).ToArray().CopyTo()
        }

    }
}
