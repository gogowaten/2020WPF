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

//イマイチ
//byte配列の平均値を求める
//SIMDを使う場合はVector.addを使うんだけどVector<byte>だと合計値が255いじょうにならないから
//Vector<int>を使う必要がある、ってことはbyte型配列をint型配列に変換する必要がある、この処理が重いので
//SIMD使わずに普通にループで足し算したほうが10倍以上速かった
namespace _20200201_SIMDでbyte配列の平均値
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var ary = MakeArray(1_000_000);
            //ary = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var sw = new Stopwatch();
            sw.Start();
            double aveSimd = 0;
            for (int i = 0; i < 100; i++)
            {
                aveSimd = AverageSimd(ary);
            }
            sw.Stop();
            MessageBox.Show($"{sw.ElapsedMilliseconds.ToString("00.000")}");

            sw.Restart();
            double ave = 0;
            for (int i = 0; i < 100; i++)
            {
                ave = Average(ary);
            }
            sw.Stop();
            MessageBox.Show($"{sw.ElapsedMilliseconds.ToString("00.000")}");

        }
        private double Average(byte[] ary)
        {
            int ave = ary[0];
            for (int i = 1; i < ary.Length; i++)
            {
                ave += ary[i];
            }
            return ave / (double)ary.Length;
        }
        private double AverageSimd(byte[] ary)
        {
            var ii = new int[ary.Length];
            ary.CopyTo(ii, 0);
            var v1 = new Vector<int>(ii);
            int simdLength = Vector<int>.Count;//Vectorの長さ、一度に計算できる量
            int amari = ii.Length % simdLength;//余り、配列の端数
            int lastIndex = ii.Length - amari;//SIMDで計算する最後のIndex
            //SIMDで足し算計算
            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                v1 = System.Numerics.Vector.Add(v1, new Vector<int>(ii, i));
            }
            //Vectorの中の合計
            int ave = v1[0];
            for (int i = 1; i < simdLength; i++)
            {
                ave += v1[i];
            }
            //配列の端数も合計
            for (int i = ii.Length - amari; i < ii.Length; i++)
            {
                ave += ii[i];
            }
            //平均値
            return ave / (double)ii.Length;
        }
        private byte[] MakeArray(int count)
        {
            var r = new Random();
            var ary = new byte[count];
            r.NextBytes(ary);
            return ary;
        }
    }


}
