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
using System.Numerics;
using System.Threading;
using System.Collections.Concurrent;

namespace _20200210_配列の分散_VectorDot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyByteAry;
        private int[] MyIntAry;
        private long[] MyLongAry;
        private const int LOOP_COUNT = 100;
        private const int ELEMENT_COUNT = 1_000_001;
        private double MyAverage;

        public MainWindow()
        {
            InitializeComponent();

            var vb = new Vector<byte>(2);
            var vb2 = new Vector<byte>(3);
            var neko = System.Numerics.Vector.ConditionalSelect<byte>(vb, vb, vb2);
            

            MyTextBlock.Text = $"配列の値の分散、要素数{ELEMENT_COUNT.ToString("N0")}の分散を{LOOP_COUNT}回求める処理時間";
            MyTextBlockVectorCount.Text = $"Vector<long>.Count = {Vector<long>.Count}";
            MyIntAry = Enumerable.Range(1, ELEMENT_COUNT).ToArray();//連番値
            //MyIntAry = Enumerable.Repeat(1, ELEMENT_COUNT).ToArray();//全値1
            MyLongAry = new long[MyIntAry.Length];//long型配列作成
            MyIntAry.CopyTo(MyLongAry, 0);
            MyInitialize();


            Button1.Click += (s, e) => MyExe(Test01, Tb1, MyByteAry);
            Button2.Click += (s, e) => MyExe(Test02, Tb2, MyByteAry);
            Button3.Click += (s, e) => MyExe(Test03, Tb3, MyByteAry);
            Button4.Click += (s, e) => MyExe(Test04, Tb4, MyByteAry);
            Button5.Click += (s, e) => MyExe(Test05, Tb5, MyByteAry);
            Button6.Click += (s, e) => MyExe(Test06_Vector4, Tb6, MyByteAry);
            //Button7.Click += (s, e) => MyExe(Test07_Vector4, Tb7, MyByteAry);

        }
        private void MyInitialize()
        {
            MyByteAry = new byte[ELEMENT_COUNT];
            var r = new Random();
            r.NextBytes(MyByteAry);
            //要素の平均値
            MyAverage = GetAverage(MyByteAry);

        }

        private double Test01(byte[] ary)
        {
            //平均との差の2乗を合計
            double total = 0;
            for (int i = 0; i < ary.Length; i++)
            {
                total += Math.Pow(ary[i] - MyAverage, 2.0);
            }
            //合計 / 要素数 = 分散
            return total / ary.Length;
        }

        //Vector<float>で計算
        private float Test02(byte[] ary)
        {
            var vAverage = new Vector<float>((float)MyAverage);
            int simdLength = Vector<float>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<float> v;
            var ss = new float[simdLength];
            float total = 0;
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                for (int j = 0; j < simdLength; j++)
                {
                    ss[j] = ary[i + j];
                }
                //平均との差
                v = System.Numerics.Vector.Subtract(vAverage, new Vector<float>(ss));
                //差の2乗を合計
                total += System.Numerics.Vector.Dot(v, v);
            }
            return total / ary.Length;
        }

        //Vector<double>で計算
        private double Test03(byte[] ary)
        {
            var vAverage = new Vector<double>(MyAverage);
            int simdLength = Vector<double>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<double> v;
            var ss = new double[simdLength];
            double total = 0;
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                for (int j = 0; j < simdLength; j++)
                {
                    ss[j] = ary[i + j];
                }
                //平均との差
                v = System.Numerics.Vector.Subtract(vAverage, new Vector<double>(ss));
                //差の2乗を合計
                total += System.Numerics.Vector.Dot(v, v);
            }
            return total / ary.Length;
        }

        //Vector<int>で計算
        private double Test04(byte[] ary)
        {
            var vAverage = new Vector<int>((int)MyAverage);
            int simdLength = Vector<int>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<int> v;
            var ss = new int[simdLength];
            double total = 0;
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                for (int j = 0; j < simdLength; j++)
                {
                    ss[j] = ary[i + j];
                }
                //平均との差
                v = System.Numerics.Vector.Subtract(vAverage, new Vector<int>(ss));
                //差の2乗を合計
                total += System.Numerics.Vector.Dot(v, v);
            }
            return total / ary.Length;
        }

        //Vector<byte>で計算
        private double Test05(byte[] ary)
        {
            var vAverage = new Vector<byte>((byte)MyAverage);
            int simdLength = Vector<byte>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<byte> v;
            double total = 0;

            for (int i = 0; i < lastIndex; i += simdLength)
            {
                //平均との差
                v = System.Numerics.Vector.Subtract(vAverage, new Vector<byte>(ary, i));
                //差の2乗を合計
                total += System.Numerics.Vector.Dot(v, v);//オーバーフロー
            }
            return total / ary.Length;
        }

        private double Test06_Vector4(byte[] ary)
        {
            int lastIndex = ary.Length - (ary.Length % 4);
            var vAverage = new Vector4((float)MyAverage);
            Vector4 v = new Vector4();
            double total = 0;
            for (int i = 0; i < lastIndex; i += 4)
            {
                //平均との差
                v = Vector4.Subtract(new Vector4(ary[i], ary[i + 1], ary[i + 2], ary[i + 3]), vAverage);
                //差の2乗を合計
                total += Vector4.Dot(v, v);
            }            
            return total / ary.Length;
        }

        private double Test07_Vector4(ReadOnlySpan<byte> ary)
        {
            int lastIndex = ary.Length - (ary.Length % 4);
            var vAverage = new Vector4((float)MyAverage);
            Vector4 v = new Vector4();
            double total = 0;
            for (int i = 0; i < lastIndex; i += 4)
            {
                //平均との差
                v = Vector4.Subtract(new Vector4(ary[i], ary[i + 1], ary[i + 2], ary[i + 3]), vAverage);
                //差の2乗を合計
                total += Vector4.Dot(v, v);
            }
            return total / ary.Length;
        }













        private double GetAverage(byte[] ary)
        {
            long total = 0;
            Parallel.ForEach(Partitioner.Create(0, ary.Length),
                (range) =>
                {
                    long subtotal = 0;
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        subtotal += ary[i];
                    }
                    Interlocked.Add(ref total, subtotal);
                });
            return total / (double)ary.Length;
        }








        private void MyExe(Func<int[], long> func, TextBlock tb)
        {
            long total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(MyIntAry);
            }
            sw.Stop();
            tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  合計値：{total}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
        }
        private void MyExe(Func<long[], long> func, TextBlock tb)
        {
            long total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(MyLongAry);
            }
            sw.Stop();
            tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  合計値：{total}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
        }
        private void MyExe(Func<byte[], double> func, TextBlock tb, byte[] ary)
        {
            double total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(ary);
            }
            sw.Stop();
            tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  合計値：{total}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
        }
        private void MyExe(Func<byte[], float> func, TextBlock tb, byte[] ary)
        {
            double total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(ary);
            }
            sw.Stop();
            tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  合計値：{total}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
        }

      

    }

}
