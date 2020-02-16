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
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;



namespace _20200216_分散を求めるマルチスレッド編double型
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double[] MyDoubleAry;
        private const int LOOP_COUNT = 100;
        private const int ELEMENT_COUNT = 10_000_000;
        //private const int ELEMENT_COUNT = 3264 * 2448;//7,990,272、今使っているスマカメ
        //private const int ELEMENT_COUNT = 7680 * 4320;//33,177,600、8K解像度
        //private double MyAverage;
        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
            SetRandomByte();

            ButtonReset.Click += (s, e) => MyReset();

            Button1.Click += (s, e) => MyExe(Test01_V1MT_ParallelForEach, Tb1, MyDoubleAry);
            Button2.Click += (s, e) => MyExe(Test02_V2MT_ParallelForEach, Tb2, MyDoubleAry);
            Button3.Click += (s, e) => MyExe(Test03_V1_ParallelLinq, Tb3, MyDoubleAry);
            Button4.Click += (s, e) => MyExe(Test04_V2_ParallelLinq, Tb4, MyDoubleAry);
            Button5.Click += (s, e) => MyExe(Test05_V1MT_ParallelForEach_VectorDouble, Tb5, MyDoubleAry);
            Button6.Click += (s, e) => MyExe(Test06_V2MT_ParallelForEach_VectorDouble, Tb6, MyDoubleAry);
            Button7.Click += (s, e) => MyExe(Test07_V1MT_ParallelForEach_Vector4, Tb7, MyDoubleAry);
            
            Button8.Click += (s, e) => MyExe(Test08_V2MT_ParallelForEach_Vector4, Tb8, MyDoubleAry);
            //Button9.Click += (s, e) => MyExe(Test09_V1_ParallelForE_VectorDouble, Tb9, MyByteAry);
            //Button10.Click += (s, e) => MyExe(Test10_V1_ParallelForE_VectorInteger, Tb10, MyByteAry);
            //Button11.Click += (s, e) => MyExe(Test11_V1_ParallelForE_VectorFloat, Tb11, MyByteAry);
            //Button12.Click += (s, e) => MyExe(Test12_V1_ParallelForE_Vector4, Tb12, MyByteAry);

            //Button13.Click += (s, e) => MyExe(Test13_V2ST_ForLoop, Tb13, MyByteAry);
            //Button14.Click += (s, e) => MyExe(Test14_V2_ParallelFor, Tb14, MyByteAry);
            //Button15.Click += (s, e) => MyExe(Test15_V2_ParallelForE, Tb15, MyByteAry);
            //Button16.Click += (s, e) => MyExe(Test16_V2ST_VectorDouble, Tb16, MyByteAry);
            //Button17.Click += (s, e) => MyExe(Test17_V2_ParallelForE_VectorDouble, Tb17, MyByteAry);
            //Button18.Click += (s, e) => MyExe(Test18_V2_ParallelForE_VectorInt, Tb18, MyByteAry);
            //Button19.Click += (s, e) => MyExe(Test19_V2_ParallelForE_VectorByteWiden, Tb19, MyByteAry);
            //Button20.Click += (s, e) => MyExe(Test20_V2_ParallelForE_Vector4, Tb20, MyByteAry);

            //Button21.Click += (s, e) => MyExe(Test21_V1_ParallelLinq, Tb21, MyByteAry);
            //Button22.Click += (s, e) => MyExe(Test22_V2_ParallelLinq, Tb22, MyByteAry);
            //Button23.Click += (s, e) => MyExe(Test20_V2_ParallelForE_Vector4, Tb23, MyByteAry);
            //Button24.Click += (s, e) => MyExe(Test22_V2_ParallelLinq, Tb24, MyByteAry);
            //Button25.Click += (s, e) => MyExe(Test21_V1_ParallelLinq, Tb25, MyByteAry);
        }

        private void MyInitialize()
        {
            MyTextBlock.Text = $"double型配列の値の分散、要素数{ELEMENT_COUNT.ToString("N0")}の分散を{LOOP_COUNT}回求める";
            MyTextBlockVectorCount.Text = $"Vector<long>.Count = {Vector<long>.Count}";
            string str = $"VectorCount : Long={Vector<long>.Count}, Double={Vector<double>.Count}, int={Vector<int>.Count}, flort={Vector<float>.Count}, short={Vector<short>.Count}, byte={Vector<byte>.Count}";
            MyTextBlockVectorCount.Text = str;
            MyTextBlockCpuThreadCount.Text = $"CPUスレッド数：{Environment.ProcessorCount.ToString()} thread";

        }
        private void SetRandomByte()
        {
            MyDoubleAry = new double[ELEMENT_COUNT];
            var r = new Random();
            for (int i = 0; i < ELEMENT_COUNT; i++)
            {
                MyDoubleAry[i] = r.NextDouble();
            }

            //MyAverage = GetAverage(MyDoubleAry);//要素の平均値
        }

        //平均値
        private double MyAverage(double[] ary)
        {
            ConcurrentBag<double> myBag = new ConcurrentBag<double>();
            Parallel.ForEach(Partitioner.Create(0, ary.Length),
                (range) =>
                {
                    double subtotal = 0;
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        subtotal += ary[i];
                    }
                    myBag.Add(subtotal);
                });
            return myBag.Sum() / ary.Length;
        }


        #region 分散
     
        //ParallelForEachで分散その1
        private double Test01_V1MT_ParallelForEach(double[] ary)
        {
            double average = MyAverage(ary);//平均値
            var myBag = new ConcurrentBag<double>();
            //1スレッドに配分する配列サイズ
            int windowSize = ary.Length / Environment.ProcessorCount;

            Parallel.ForEach(
                Partitioner.Create(0, ary.Length, windowSize),
                (range) =>
                {
                    double subtotal = 0;//小計用
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        //平均との差(偏差)の2乗を合計
                        subtotal += Math.Pow(ary[i] - average, 2);
                    }
                    myBag.Add(subtotal);//排他処理で小計を追加
                });

            //合計 / 要素数 = 分散
            return myBag.Sum() / ary.Length;
        }
        //ParallelForEachで分散その2
        private double Test02_V2MT_ParallelForEach(double[] ary)
        {
            var myBag = new ConcurrentBag<double>();
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount),
                (range) =>
                {
                    double subtotal = 0;
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        subtotal += ary[i] * ary[i];
                    }
                    myBag.Add(subtotal);
                });
            double average = MyAverage(ary);
            return (myBag.Sum() / ary.Length) - (average * average);
        }

        //LINQで分散その1
        private double Test03_V1_ParallelLinq(double[] ary)
        {
            double average = MyAverage(ary);
            return ary.AsParallel().Select(x => Math.Pow(x - average, 2)).Sum() / ary.Length;
        }
        //LINQで分散その2
        private double Test04_V2_ParallelLinq(double[] ary)
        {
            double average = MyAverage(ary);
            return (ary.AsParallel().Select(x => x * x).Sum(x => x) / ary.Length) - (average * average);
        }

        //Parallel.ForEachとVector<double>で分散その1
        private double Test05_V1MT_ParallelForEach_VectorDouble(double[] ary)
        {
            double average = MyAverage(ary);//平均値
            var myBag = new ConcurrentBag<double>();
            //1スレッドに配分する配列サイズ
            int windowSize = ary.Length / Environment.ProcessorCount;
            int simdLength = Vector<double>.Count;
            Vector<double> vAverage = new Vector<double>(average);//スレッドごとに作った方がいい？
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length, windowSize),
                (range) =>
                {
                    //Vector<double> vAverage = new Vector<double>(average);//スレッドごとに作った方がいい？
                    double subtotal = 0;//小計用
                    Vector<double> v;
                    for (int i = range.Item1; i < range.Item2; i += simdLength)
                    {
                        //偏差(平均との差)
                        v = System.Numerics.Vector.Subtract(new Vector<double>(ary, i), vAverage);
                        //偏差の2乗を小計
                        subtotal += System.Numerics.Vector.Dot(v, v);
                    }
                    //simdlengthで割り切れなかった余りの要素用
                    subtotal += MySuquareSumOfDeviation偏差の2乗和(ary, range.Item1, range.Item2, average,simdLength);
                    myBag.Add(subtotal);//排他処理で小計を追加
                });
            return myBag.Sum() / ary.Length;
        }
        //Parallel.ForEachとVector<double>で分散その2
        private double Test06_V2MT_ParallelForEach_VectorDouble(double[] ary)
        {
            var myBag = new ConcurrentBag<double>();
            int simdLength = Vector<double>.Count;
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount),
                (range) =>
                {
                    double subtotal = 0;
                    Vector<double> v;
                    for (int i = range.Item1; i < range.Item2; i += simdLength)
                    {
                        v = new Vector<double>(ary, i);
                        subtotal += System.Numerics.Vector.Dot(v, v);
                    }
                    subtotal += MySuquareSum2乗和(ary, range.Item1, range.Item2, simdLength);
                    myBag.Add(subtotal);
                });
            double average = MyAverage(ary);
            return (myBag.Sum() / ary.Length) - (average * average);
        }

        //Parallel.ForEachとVector4で分散その1
        private double Test07_V1MT_ParallelForEach_Vector4(double[] ary)
        {
            double average = MyAverage(ary);//平均値
            var myBag = new ConcurrentBag<double>();
            //1スレッドに配分する配列サイズ
            int windowSize = ary.Length / Environment.ProcessorCount;
            int simdLength = 4;
            Vector4 vAverage = new Vector4((float)average);
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length, windowSize),
                (range) =>
                {
                    double subtotal = 0;//小計用
                    Vector4 v;
                    for (int i = range.Item1; i < range.Item2; i += simdLength)
                    {
                        //偏差(平均との差)
                        v = Vector4.Subtract(new Vector4((float)ary[i], (float)ary[i + 1], (float)ary[i + 2], (float)ary[i + 3]), vAverage);
                        //偏差の2乗を小計
                        subtotal += Vector4.Dot(v, v);
                    }
                    //simdlengthで割り切れなかった余りの要素用
                    subtotal += MySuquareSumOfDeviation偏差の2乗和(ary, range.Item1, range.Item2, average,simdLength);
                    myBag.Add(subtotal);//排他処理で小計を追加
                });
            return myBag.Sum() / ary.Length;
        }


        //Parallel.ForEachとVector4で分散その2
        private double Test08_V2MT_ParallelForEach_Vector4(double[] ary)
        {
            var myBag = new ConcurrentBag<double>();
            int simdLength = 4;
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount),
                (range) =>
                {
                    double subtotal = 0;
                    Vector4 v;
                    for (int i = range.Item1; i < range.Item2; i += simdLength)
                    {
                        v = new Vector4((float)ary[i], (float)ary[i + 1], (float)ary[i + 2], (float)ary[i + 3]);
                        subtotal += Vector4.Dot(v, v);
                    }
                    subtotal += MySuquareSum2乗和(ary, range.Item1, range.Item2, simdLength);
                    myBag.Add(subtotal);
                });
            double average = MyAverage(ary);
            return (myBag.Sum() / ary.Length) - (average * average);
        }


        #endregion 分散




        //指定インデックスから最後までの偏差の2乗和を返す
        //VectorCountで割り切れなかった余り用、分散の求め方その1用
        private double MySuquareSumOfDeviation偏差の2乗和(double[] ary, int beginIndex, int endIndex, double average,int simdLength)
        {
            //あまりの位置のインデックス
            int lastIndex = endIndex - (endIndex - beginIndex) % simdLength;
            double total = 0;
            for (int i = lastIndex; i < endIndex; i++)
            {
                total += Math.Pow(ary[i] - average, 2.0);
            }
            return total;
        }

        /// <summary>
        /// byte型配列用、分散の求め方その2用、VectorCountで割り切れなかった余りの要素の2乗和を返す、要素数10でVectorCountが4のとき10%4=2なので、最後の2つの要素が対象になる
        /// </summary>
        /// <param name="ary">配列</param>
        /// <param name="beginIndex">範囲の開始インデックス</param>
        /// <param name="endIndex">範囲の終了インデックス</param>
        /// <param name="simdLength">Vector[T].Count</param>
        /// <returns></returns>
        private double MySuquareSum2乗和(double[] ary, int beginIndex, int endIndex, int simdLength)
        {
            //あまりの位置のインデックス
            int lastIndex = endIndex - (endIndex - beginIndex) % simdLength;
            double total = 0;
            for (int i = lastIndex; i < endIndex; i++)
            {
                total += ary[i] * ary[i];
            }
            return total;
        }


        private void MyReset()
        {
            var neko = EnumerateDescendantObjects2<TextBlock>(this);
            foreach (var item in neko)
            {
                item.Text = "";
            }
            MyInitialize();
        }

        #region コントロールの列挙

        private static List<T> EnumerateDescendantObjects2<T>(DependencyObject obj) where T : DependencyObject
        {
            var l = new List<T>();
            foreach (object child in LogicalTreeHelper.GetChildren(obj))
            {
                if (child is T)
                {
                    l.Add((T)child);
                }
                if (child is DependencyObject dobj)
                {
                    foreach (T cobj2 in EnumerateDescendantObjects2<T>(dobj))
                    {
                        l.Add(cobj2);
                    }
                }
            }
            return l;
        }
        #endregion

        private void MyExe(Func<double[], double> func, TextBlock tb, double[] ary)
        {
            double total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(ary);
            }
            sw.Stop();
            tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  分散 = {total.ToString("F15")}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
        }
    }
}
