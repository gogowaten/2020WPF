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

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Numerics;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace _20200225_Intrinsics_MinMax
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyArray;
        private const int LOOP_COUNT = 1000;
        private const int ELEMENT_COUNT = 10_000_001;//要素数1千万
        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();

            MyTextBlock.Text = $"byte型配列要素数{ELEMENT_COUNT.ToString("N0")} から最小値と最大値を {LOOP_COUNT}回求める";
            MyTextBlockVectorCount.Text = $"Vector256<byte>.Count={Vector256<byte>.Count} Vector<byte>.Count={Vector<byte>.Count}";
            MyTextBlockCpuThreadCount.Text = $"CPUスレッド数：{Environment.ProcessorCount}";

            //var mm1 = Test1_MinMax_IntrinsicsVector(MyArray);
            //var mm2 = Test2_MinMax_IntrinsicsVector_Multi(MyArray);
            //var mm3 = Test3_MinMax_NumericsVector(MyArray);
            //var mm4 = Test4_MinMax_NumericsVector_Multi(MyArray);
            ButtonAll.Click += (s, e) => MyExeAll();
            Button1.Click += (s, e) => MyExe(Test1_MinMax_IntrinsicsVector, Tb1, MyArray);
            Button2.Click += (s, e) => MyExe(Test2_MinMax_IntrinsicsVector_Multi, Tb2, MyArray);
            Button3.Click += (s, e) => MyExe(Test3_MinMax_NumericsVector, Tb3, MyArray);
            Button4.Click += (s, e) => MyExe(Test4_MinMax_NumericsVector_Multi, Tb4, MyArray);
            Button5.Click += (s, e) => MyExeMultiLoop(Test1_MinMax_IntrinsicsVector, Tb5, MyArray);
            Button6.Click += (s, e) => MyExeMultiLoop(Test3_MinMax_NumericsVector, Tb6, MyArray);
            Button7.Click += (s, e) => MyExe(Test5_MinMax, Tb7, MyArray);
            Button8.Click += (s, e) => MyExe(Test6_MinMax_Multi, Tb8, MyArray);
            Button9.Click += (s, e) => MyExe(Test7_MinMaxKai, Tb9, MyArray);
            Button10.Click += (s, e) => MyExe(Test8_MinMax_Multi_Kai, Tb10, MyArray);
        }


        //MinMax Intrinsics
        private unsafe (byte min, byte max) Test1_MinMax_IntrinsicsVector(byte[] vs)
        {
            var vMin = Vector256.Create(byte.MaxValue);
            var vMax = Vector256<byte>.Zero;
            int simdLength = Vector256<byte>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    var vv = Avx.LoadVector256(p + i);
                    vMin = Avx2.Min(vMin, vv);
                    vMax = Avx2.Max(vMax, vv);
                }
            }
            byte* tMin = stackalloc byte[simdLength];
            byte* tMax = stackalloc byte[simdLength];
            Avx.Store(tMin, vMin);
            Avx.Store(tMax, vMax);
            byte min = byte.MaxValue;
            byte max = byte.MinValue;
            for (int k = 0; k < simdLength; k++)
            {
                if (min > tMin[k]) min = tMin[k];
                if (max < tMax[k]) max = tMax[k];
            }
            for (int i = lastIndex; i < vs.Length; i++)
            {
                if (min > vs[i]) min = vs[i];
                if (max < vs[i]) max = vs[i];
            }
            return (min, max);
        }

        //MinMax Intrinsics + MultiThread
        private unsafe (byte min, byte max) Test2_MinMax_IntrinsicsVector_Multi(byte[] vs)
        {
            var bag = new ConcurrentBag<byte>();
            int simdLength = Vector256<byte>.Count;
            var partition = Partitioner.Create(0, vs.Length, vs.Length / Environment.ProcessorCount);
            Parallel.ForEach(partition,
                (range) =>
                {
                    var vMin = Vector256.Create(byte.MaxValue);
                    var vMax = Vector256<byte>.Zero;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            var vv = Avx.LoadVector256(p + i);
                            vMin = Avx2.Min(vMin, vv);
                            vMax = Avx2.Max(vMax, vv);
                        }
                    }
                    byte* tMin = stackalloc byte[simdLength];
                    byte* tMax = stackalloc byte[simdLength];
                    Avx.Store(tMin, vMin);
                    Avx.Store(tMax, vMax);
                    byte min = byte.MaxValue;
                    byte max = byte.MinValue;
                    for (int k = 0; k < simdLength; k++)
                    {
                        if (min > tMin[k]) min = tMin[k];
                        if (max < tMax[k]) max = tMax[k];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        if (min > vs[i]) min = vs[i];
                        if (max < vs[i]) max = vs[i];
                    }
                    bag.Add(min);
                    bag.Add(max);
                });

            return (bag.Min(), bag.Max());
        }

        private (byte min, byte max) Test3_MinMax_NumericsVector(byte[] vs)
        {
            int simdLength = Vector<byte>.Count;
            int lastIndex = vs.Length - simdLength;
            var vMin = new Vector<byte>(byte.MaxValue);
            var vMax = new Vector<byte>(byte.MinValue);
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                var v = new Vector<byte>(vs, i);
                vMin = System.Numerics.Vector.Min(v, vMin);
                vMax = System.Numerics.Vector.Max(v, vMax);
            }
            byte min = byte.MaxValue;
            byte max = byte.MinValue;
            for (int i = 0; i < simdLength; i++)
            {
                if (min > vMin[i]) min = vMin[i];
                if (max < vMax[i]) max = vMax[i];
            }
            for (int i = lastIndex; i < vs.Length; i++)
            {
                if (min > vs[i]) min = vs[i];
                if (max < vs[i]) max = vs[i];
            }
            return (min, max);
        }

        private (byte min, byte max) Test4_MinMax_NumericsVector_Multi(byte[] vs)
        {
            var bag = new ConcurrentBag<byte>();
            var rangeSize = Partitioner.Create(0, vs.Length, vs.Length / Environment.ProcessorCount);
            Parallel.ForEach(rangeSize,
                (range) =>
                {
                    int simdLength = Vector<byte>.Count;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    var vMin = new Vector<byte>(byte.MaxValue);
                    var vMax = new Vector<byte>(byte.MinValue);
                    for (int i = range.Item1; i < lastIndex; i += simdLength)
                    {
                        var v = new Vector<byte>(vs, i);
                        vMin = System.Numerics.Vector.Min(v, vMin);
                        vMax = System.Numerics.Vector.Max(v, vMax);
                    }

                    byte min = byte.MaxValue;
                    byte max = byte.MinValue;
                    for (int i = 0; i < simdLength; i++)
                    {
                        if (min > vMin[i]) min = vMin[i];
                        if (max < vMax[i]) max = vMax[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        if (min > vs[i]) min = vs[i];
                        if (max < vs[i]) max = vs[i];
                    }
                    bag.Add(min);
                    bag.Add(max);
                });

            return (bag.Min(), bag.Max());
        }

        private (byte min, byte max) Test5_MinMax(byte[] vs)
        {
            var min = byte.MaxValue;
            var max = byte.MinValue;
            for (int i = 0; i < vs.Length; i++)
            {
                if (min > vs[i]) min = vs[i];
                if (max < vs[i]) max = vs[i];
            }
            return (min, max);
        }

        private (byte min, byte max) Test6_MinMax_Multi(byte[] vs)
        {
            var bag = new ConcurrentBag<byte>();
            Parallel.ForEach(
                Partitioner.Create(0, vs.Length, vs.Length / Environment.ProcessorCount),
                (range) =>
                {
                    var min = byte.MaxValue;
                    var max = byte.MinValue;
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        if (min > vs[i]) min = vs[i];
                        if (max < vs[i]) max = vs[i];
                    }
                    bag.Add(min);
                    bag.Add(max);
                });
            return (bag.Min(), bag.Max());
        }

        //普通の改変、ループの中の処理を4つにした、1.25倍速になった。8個にしたら逆に遅くなった
        private (byte min, byte max) Test7_MinMaxKai(byte[] vs)
        {
            var min = byte.MaxValue;
            var max = byte.MinValue;
            int lastIndex = vs.Length - vs.Length % 2;
            for (int i = 0; i < lastIndex; i += 2)
            {
                if (min > vs[i]) min = vs[i];
                if (min > vs[i + 1]) min = vs[i + 1];
                //if (min > vs[i + 2]) min = vs[i + 2];
                //if (min > vs[i + 3]) min = vs[i + 3];
                if (max < vs[i]) max = vs[i];
                if (max < vs[i + 1]) max = vs[i + 1];
                //if (max < vs[i + 2]) max = vs[i + 2];
                //if (max < vs[i + 3]) max = vs[i + 3];
            }
            for (int i = lastIndex; i < vs.Length; i++)
            {
                if (min > vs[i]) min = vs[i];
                if (max < vs[i]) max = vs[i];
            }
            return (min, max);
        }

        //普通＋マルチスレッドの改変、ループの中の処理を4つにした、1.3倍速になった？
        private (byte min, byte max) Test8_MinMax_Multi_Kai(byte[] vs)
        {
            var bag = new ConcurrentBag<byte>();
            Parallel.ForEach(
                Partitioner.Create(0, vs.Length, vs.Length / Environment.ProcessorCount),
                (range) =>
                {
                    var min = byte.MaxValue;
                    var max = byte.MinValue;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % 2;
                    for (int i = range.Item1; i < lastIndex; i += 2)
                    {
                        if (min > vs[i]) min = vs[i];
                        if (min > vs[i + 1]) min = vs[i + 1];
                        if (max < vs[i]) max = vs[i];
                        if (max < vs[i + 1]) max = vs[i + 1];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        if (min > vs[i]) min = vs[i];
                        if (max < vs[i]) max = vs[i];
                    }
                    bag.Add(min);
                    bag.Add(max);
                });
            return (bag.Min(), bag.Max());
        }






        private void MyInitialize()
        {
            MyArray = new byte[ELEMENT_COUNT];

            //指定値で埋める
            var span = new Span<byte>(MyArray);
            span.Fill(250);

            //最後の要素
            MyArray[ELEMENT_COUNT - 1] = 100;

            //ランダム値
            //var r = new Random();
            //r.NextBytes(MyArray);

            ////0～255までを連番で繰り返し
            //for (int i = 0; i < ELEMENT_COUNT; i++)
            //{
            //    MyArray[i] = (byte)i;
            //}


        }


        #region 時間計測
        private void MyExe(Func<byte[], (byte, byte)> func, TextBlock tb, byte[] vs)
        {
            (byte, byte) minmax = (0, 0);
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                minmax = func(vs);
            }
            sw.Stop();
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒 {minmax} | {func.Method.Name}");
        }

        //あんまり意味ない、シングルスレッドのminmaxメソッドを呼び出すループをマルチスレッドにしたもの
        private void MyExeMultiLoop(Func<byte[], (byte, byte)> func, TextBlock tb, byte[] vs)
        {
            (byte, byte) minmax = (0, 0);
            var sw = new Stopwatch();
            sw.Start();
            Parallel.For(0, LOOP_COUNT,
                new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                x => minmax = func(vs));
            sw.Stop();
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒 {minmax} | {func.Method.Name}(LoopMulti)");
        }
       

        //一斉テスト用
        private async void MyExeAll()
        {
            var sw = new Stopwatch();
            sw.Start();
            this.IsEnabled = false;
            await Task.Run(() => MyExe(Test1_MinMax_IntrinsicsVector, Tb1, MyArray));
            await Task.Run(() => MyExe(Test2_MinMax_IntrinsicsVector_Multi, Tb2, MyArray));
            await Task.Run(() => MyExe(Test3_MinMax_NumericsVector, Tb3, MyArray));
            await Task.Run(() => MyExe(Test4_MinMax_NumericsVector_Multi, Tb4, MyArray));
            await Task.Run(() => MyExeMultiLoop(Test1_MinMax_IntrinsicsVector, Tb5, MyArray));
            await Task.Run(() => MyExeMultiLoop(Test3_MinMax_NumericsVector, Tb6, MyArray));
            await Task.Run(() => MyExe(Test5_MinMax, Tb7, MyArray));
            await Task.Run(() => MyExe(Test6_MinMax_Multi, Tb8, MyArray));
            await Task.Run(() => MyExe(Test7_MinMaxKai, Tb9, MyArray));
            await Task.Run(() => MyExe(Test8_MinMax_Multi_Kai, Tb10, MyArray));
            this.IsEnabled = true;
            sw.Stop();
            TbAll.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒";
        }
        #endregion 時間計測
    }
}
