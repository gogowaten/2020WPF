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

namespace _20200227_IntrinsicsAdd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyArray;
        private const int LOOP_COUNT = 1000;
        private const int ELEMENT_COUNT = 10_000_001;//要素数

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();

            // int simdLength = Vector256<int>.Count;
            // var aa = simdLength * Environment.ProcessorCount;
            // var bb = MyArray.Length % aa;
            // var cc = MyArray.Length - bb;
            // var dd = Partitioner.Create(0, cc,cc/ Environment.ProcessorCount);
            //var dp= dd.GetDynamicPartitions();

            // var op = dd.GetOrderableDynamicPartitions();
            //int block = vs.Length - (vs.Length % (simdLength * Environment.ProcessorCount));

            MyTextBlock.Text = $"byte型配列要素数{ELEMENT_COUNT.ToString("N0")} 合計値を {LOOP_COUNT}回求める";
            MyTextBlockVectorCount.Text = $"Vector256<byte>.Count={Vector256<byte>.Count} Vector<byte>.Count={Vector<byte>.Count}";
            MyTextBlockCpuThreadCount.Text = $"CPUスレッド数：{Environment.ProcessorCount}";

            ////var mm1 = Test1_MinMax_IntrinsicsVector(MyArray);
            ////var mm2 = Test2_MinMax_IntrinsicsVector_Multi(MyArray);
            ////var mm3 = Test3_MinMax_NumericsVector(MyArray);
            ////var mm4 = Test4_MinMax_NumericsVector_Multi(MyArray);
            //ButtonAll.Click += (s, e) => MyExeAll();
            Button1.Click += (s, e) => MyExe(Test1, Tb1, MyArray);
            Button2.Click += (s, e) => MyExe(Test2, Tb2, MyArray);
            Button3.Click += (s, e) => MyExe(Test3, Tb3, MyArray);
            Button4.Click += (s, e) => MyExe(Test4, Tb4, MyArray);
            Button5.Click += (s, e) => MyExe(Test5, Tb5, MyArray);
            Button6.Click += (s, e) => MyExe(Test6, Tb6, MyArray);
            Button7.Click += (s, e) => MyExe(Test7, Tb7, MyArray);
            Button8.Click += (s, e) => MyExe(Test8, Tb8, MyArray);
            //Button9.Click += (s, e) => MyExe(Test7_MinMaxKai, Tb9, MyArray);
            //Button10.Click += (s, e) => MyExe(Test8_MinMax_Multi_Kai, Tb10, MyArray);
        }

        //普通に足し算
        private long Test1(byte[] vs)
        {
            long total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total;
        }

        //普通に足し算をマルチスレッド化
        private long Test2(byte[] vs)
        {
            long total = 0;
            Parallel.ForEach(
                Partitioner.Create(0, vs.Length, vs.Length / Environment.ProcessorCount),
                (range) =>
                {
                    long subtotal = 0;
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        subtotal += vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }


        //普通に足し算のforの中の足し算を4個
        private long Test3(byte[] vs)
        {
            long total = 0;
            int lastIndex = vs.Length - (vs.Length % 4);
            for (int i = 0; i < lastIndex; i += 4)
            {
                total += vs[i];
                total += vs[i + 1];
                total += vs[i + 2];
                total += vs[i + 3];
            }
            return total;
        }

        //Intrinsics + シングルスレッド
        private unsafe long Test4(byte[] vs)
        {
            var vTotal = Vector256<int>.Zero;
            int simdLength = Vector256<int>.Count;
            int i;
            fixed (byte* p = vs)
            {
                for (i = 0; i < vs.Length; i += simdLength)
                {
                    vTotal = Avx2.Add(vTotal, Avx2.ConvertToVector256Int32(p + i));
                }
            }
            long total = 0;
            int* ip = stackalloc int[simdLength];
            Avx.Store(ip, vTotal);
            for (int j = 0; j < simdLength; j++)
            {
                total += ip[j];
            }
            for (; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total;
        }

        //Intrinsics + マルチスレッド
        private unsafe long Test5(byte[] vs)
        {
            int simdLength = Vector256<int>.Count;
            long total = 0;
            Parallel.ForEach(
                Partitioner.Create(0, vs.Length, vs.Length / Environment.ProcessorCount),
                (range) =>
                {
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    var vTotal = Vector256<int>.Zero;
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            vTotal = Avx2.Add(vTotal, Avx2.ConvertToVector256Int32(p + i));
                        }
                    }
                    int* pp = stackalloc int[simdLength];
                    Avx.Store(pp, vTotal);
                    long subtotal = 0;
                    for (int i = 0; i < simdLength; i++)
                    {
                        subtotal += pp[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }

        //↑より少し遅いし、桁あふれも速い
        //Intrinsics + マルチスレッド2
        //
        private unsafe long Test6(byte[] vs)
        {
            int simdLength = Vector256<int>.Count;
            int block = vs.Length - (vs.Length % (simdLength * Environment.ProcessorCount));
            var bag = new ConcurrentBag<Vector256<int>>();

            Parallel.ForEach(
                Partitioner.Create(0, block, block / Environment.ProcessorCount),
                (range) =>
                {
                    var vTotal = Vector256<int>.Zero;
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < range.Item2; i += simdLength)
                        {
                            vTotal = Avx2.Add(vTotal, Avx2.ConvertToVector256Int32(p + i));
                        }
                    }
                    bag.Add(vTotal);

                });

            Vector256<int> vv = Vector256<int>.Zero;
            foreach (var item in bag)
            {
                vv = Avx2.Add(vv, item);
            }

            int* ptr = stackalloc int[simdLength];
            Avx.Store(ptr, vv);
            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += ptr[i];
            }
            for (int i = block; i < vs.Length; i++)
            {
                total += vs[i];
            }


            return total;
        }

        //Intrinsics + マルチスレッド3
        //longで計算、遅くなるけど桁数は大きくなる
        private unsafe long Test7(byte[] vs)
        {
            int simdLength = Vector256<long>.Count;
            long total = 0;
            Parallel.ForEach(
                Partitioner.Create(0, vs.Length, vs.Length / Environment.ProcessorCount),
                (range) =>
                {
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    var vTotal = Vector256<long>.Zero;
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            vTotal = Avx2.Add(vTotal, Avx2.ConvertToVector256Int64(p + i));
                        }
                    }
                    long* pp = stackalloc long[simdLength];
                    Avx.Store(pp, vTotal);
                    long subtotal = 0;
                    for (int i = 0; i < simdLength; i++)
                    {
                        subtotal += pp[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }

        private long Test8(byte[] ary)
        {
            long total = 0;
            int windowSize = ary.Length / Environment.ProcessorCount;
            var rangePartitioner = Partitioner.Create(0, ary.Length, windowSize);//
            int simdLength = Vector<byte>.Count;
            Parallel.ForEach(rangePartitioner, (range) =>
            {
                int lastIndex = range.Item2 - ((range.Item2 - range.Item1) % simdLength);
                var v = new Vector<ulong>();
                for (int i = range.Item1; i < lastIndex; i += simdLength)
                {
                    System.Numerics.Vector.Widen(new Vector<byte>(ary, i), out Vector<ushort> vv1, out Vector<ushort> vv2);
                    System.Numerics.Vector.Widen(vv1, out Vector<uint> ui1, out Vector<uint> ui2);
                    System.Numerics.Vector.Widen(vv2, out Vector<uint> ui3, out Vector<uint> ui4);
                    System.Numerics.Vector.Widen(ui1, out Vector<ulong> ul1, out Vector<ulong> ul2);
                    System.Numerics.Vector.Widen(ui2, out Vector<ulong> ul3, out Vector<ulong> ul4);
                    System.Numerics.Vector.Widen(ui3, out Vector<ulong> ul5, out Vector<ulong> ul6);
                    System.Numerics.Vector.Widen(ui4, out Vector<ulong> ul7, out Vector<ulong> ul8);

                    v = System.Numerics.Vector.Add(v, ul1);
                    v = System.Numerics.Vector.Add(v, ul2);
                    v = System.Numerics.Vector.Add(v, ul3);
                    v = System.Numerics.Vector.Add(v, ul4);
                    v = System.Numerics.Vector.Add(v, ul5);
                    v = System.Numerics.Vector.Add(v, ul6);
                    v = System.Numerics.Vector.Add(v, ul7);
                    v = System.Numerics.Vector.Add(v, ul8);

                }
                ulong subtotal = 0;
                for (int i = 0; i < Vector<ulong>.Count; i++)
                {
                    subtotal += v[i];
                }
                for (int i = lastIndex; i < range.Item2; i++)
                {
                    subtotal += ary[i];
                }
                System.Threading.Interlocked.Add(ref total, (long)subtotal);
            });
            return total;
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
        private void MyExe(Func<byte[], long> func, TextBlock tb, byte[] vs)
        {
            long total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(vs);
            }
            sw.Stop();
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒 {total} | {func.Method.Name}");
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
            await Task.Run(() => MyExe(Test1, Tb1, MyArray));
            await Task.Run(() => MyExe(Test2, Tb2, MyArray));
            await Task.Run(() => MyExe(Test3, Tb3, MyArray));
            await Task.Run(() => MyExe(Test4, Tb4, MyArray));
            await Task.Run(() => MyExe(Test5, Tb5, MyArray));
            await Task.Run(() => MyExe(Test6, Tb6, MyArray));
            //await Task.Run(() => MyExe(Test5_MinMax, Tb7, MyArray));
            //await Task.Run(() => MyExe(Test6_MinMax_Multi, Tb8, MyArray));
            //await Task.Run(() => MyExe(Test7_MinMaxKai, Tb9, MyArray));
            //await Task.Run(() => MyExe(Test8_MinMax_Multi_Kai, Tb10, MyArray));
            this.IsEnabled = true;
            sw.Stop();
            TbAll.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒";
        }
        #endregion 時間計測

    }
}
