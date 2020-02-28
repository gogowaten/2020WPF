using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
        private const int ELEMENT_COUNT = 10_000_000;// 538_976_319;// 67_372_039;//要素数

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
            this.Title = this.ToString();

            //var neko = int.MaxValue;
            //var neko = uint.MaxValue;


            MyTextBlock.Text = $"byte型配列要素数{ELEMENT_COUNT.ToString("N0")}の合計値を {LOOP_COUNT}回求める";
            MyTextBlockVectorCount.Text = $"Vector256<byte>.Count={Vector256<byte>.Count}  Vector<byte>.Count={Vector<byte>.Count}";
            MyTextBlockCpuThreadCount.Text = $"CPUスレッド数：{Environment.ProcessorCount}";

            ButtonAll.Click += (s, e) => MyExeAll();
            Button1.Click += (s, e) => MyExe(Test1_Normal, Tb1, MyArray);
            Button2.Click += (s, e) => MyExe(Test2_Normal_MT, Tb2, MyArray);
            Button3.Click += (s, e) => MyExe(Test3_Normal4, Tb3, MyArray);
            Button4.Click += (s, e) => MyExe(Test4_Intrinsics_int, Tb4, MyArray);
            Button5.Click += (s, e) => MyExe(Test5_Intrinsics_int_MT, Tb5, MyArray);
            Button6.Click += (s, e) => MyExe(Test6_Intrinsics_int_MT2, Tb6, MyArray);
            Button7.Click += (s, e) => MyExe(Test7_Intrinsics_long_MT, Tb7, MyArray);
            Button8.Click += (s, e) => MyExe(Test8_Numerics_uint, Tb8, MyArray);
            Button9.Click += (s, e) => MyExe(Test9_Nunerics_uint_MT, Tb9, MyArray);
            Button10.Click += (s, e) => MyExe(Test10_Numerics_long_MT, Tb10, MyArray);
        }

        //普通に足し算
        private long Test1_Normal(byte[] vs)
        {
            long total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total;
        }

        //普通に足し算をマルチスレッド化
        private long Test2_Normal_MT(byte[] vs)
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
        private long Test3_Normal4(byte[] vs)
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
            for (int i = lastIndex; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total;
        }

        //Intrinsics + シングルスレッド、int
        //最大要素数67_372_039(約6737万)まで、これを超えると桁あふれの可能性
        //Vector256<int>.Countは8、それぞれでint型最大値の2147483647まで入る、合計すると2147483647*8=1.7179869e+10(171億…)
        //要素が全てbyte型最大値の255だった場合に171億に入る個数は、17179869176/255=67372035.98、小数点以下切り捨てて
        //これを8個づつ計算するから8で割ると67372035/8=8421504.4、小数点以下切り捨てて
        //8421504*8=67372032、これがVectorで桁あふれしないで計算する回数になる
        //余りはlongで計算するから、ここからさらにあまりの最大数の7を足して、
        //67372032+7=67372039、これが桁あふれしないで計算できる要素の最大数になる
        //8Kの画素数は7680*4320=33177600、約3317万
        private unsafe long Test4_Intrinsics_int(byte[] vs)
        {
            var vTotal = Vector256<int>.Zero;
            int simdLength = Vector256<int>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);

            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
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
            for (int i = lastIndex; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total;
        }

        //Intrinsics + マルチスレッド、int
        //最大要素数は配列の分割数分増えて約5億
        private unsafe long Test5_Intrinsics_int_MT(byte[] vs)
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

        //↑の変形、CPUスレッド数で割り切れる範囲と余りの範囲に分けて計算
        //Intrinsics + マルチスレッド2、int
        //
        private unsafe long Test6_Intrinsics_int_MT2(byte[] vs)
        {
            int simdLength = Vector256<int>.Count;
            var bag = new ConcurrentBag<Vector256<int>>();
            //割り切れる範囲
            int block = vs.Length - (vs.Length % (simdLength * Environment.ProcessorCount));

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
            #region bagの集計1、遅いし桁あふれも早く、67_372_096(6737万)で桁あふれ
            //Vector256<int> vv = Vector256<int>.Zero;
            //foreach (var item in bag)
            //{
            //    vv = Avx2.Add(vv, item);
            //}

            //int* ptr = stackalloc int[simdLength];
            //Avx.Store(ptr, vv);
            //long total = 0;
            //for (int i = 0; i < simdLength; i++)
            //{
            //    total += ptr[i];
            //}
            #endregion

            #region bagの集計2、こっちのほうがいい、最大要素数も配列の分割数分増えて約5億
            long total = 0;
            foreach (var item in bag)
            {
                int* pp = stackalloc int[simdLength];
                Avx.Store(pp, item);
                for (int i = 0; i < simdLength; i++)
                {
                    total += pp[i];
                }
            }
            #endregion

            for (int i = block; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total;
        }

        //Intrinsics + マルチスレッド3、long
        //longで計算、遅くなるけど桁数は大きくなる
        private unsafe long Test7_Intrinsics_long_MT(byte[] vs)
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


        //Numerics、シングルスレッド、uint
        private unsafe long Test8_Numerics_uint(byte[] vs)
        {

            int simdLength = Vector<byte>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            Vector<uint> v = new Vector<uint>();
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                System.Numerics.Vector.Widen(new Vector<byte>(vs, i), out Vector<ushort> vv1, out Vector<ushort> vv2);
                System.Numerics.Vector.Widen(vv1, out Vector<uint> ui1, out Vector<uint> ui2);
                System.Numerics.Vector.Widen(vv2, out Vector<uint> ui3, out Vector<uint> ui4);
                v = System.Numerics.Vector.Add(v, ui1);
                v = System.Numerics.Vector.Add(v, ui2);
                v = System.Numerics.Vector.Add(v, ui3);
                v = System.Numerics.Vector.Add(v, ui4);
            }
            long total = 0;
            for (int j = 0; j < Vector<uint>.Count; j++)
            {
                total += v[j];
            }
            for (int i = lastIndex; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total;
        }

        //Numerics、マルチスレッド、uint
        private long Test9_Nunerics_uint_MT(byte[] ary)
        {
            long total = 0;
            int simdLength = Vector<byte>.Count;
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount),
                (range) =>
                {
                    int lastIndex = range.Item2 - ((range.Item2 - range.Item1) % simdLength);
                    var v = new Vector<uint>();
                    for (int i = range.Item1; i < lastIndex; i += simdLength)
                    {
                        System.Numerics.Vector.Widen(new Vector<byte>(ary, i), out Vector<ushort> vv1, out Vector<ushort> vv2);
                        System.Numerics.Vector.Widen(vv1, out Vector<uint> ui1, out Vector<uint> ui2);
                        System.Numerics.Vector.Widen(vv2, out Vector<uint> ui3, out Vector<uint> ui4);
                        v = System.Numerics.Vector.Add(v, ui1);
                        v = System.Numerics.Vector.Add(v, ui2);
                        v = System.Numerics.Vector.Add(v, ui3);
                        v = System.Numerics.Vector.Add(v, ui4);
                    }
                    long subtotal = 0;
                    for (int i = 0; i < Vector<uint>.Count; i++)
                    {
                        subtotal += v[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += ary[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }

        //Numerics、マルチスレッド、long
        private long Test10_Numerics_long_MT(byte[] ary)
        {
            long total = 0;
            int simdLength = Vector<byte>.Count;
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount),
                (range) =>
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


        #region 未使用
        //Numerics、シングルスレッド、uint、Span
        private unsafe long Test81_Numerics_uint(Span<byte> span)
        {

            int simdLength = Vector<byte>.Count;
            int lastIndex = span.Length - (span.Length % simdLength);
            Vector<uint> v = new Vector<uint>();
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                System.Numerics.Vector.Widen(new Vector<byte>(span.Slice(i)), out Vector<ushort> vv1, out Vector<ushort> vv2);
                System.Numerics.Vector.Widen(vv1, out Vector<uint> ui1, out Vector<uint> ui2);
                System.Numerics.Vector.Widen(vv2, out Vector<uint> ui3, out Vector<uint> ui4);
                v = System.Numerics.Vector.Add(v, ui1);
                v = System.Numerics.Vector.Add(v, ui2);
                v = System.Numerics.Vector.Add(v, ui3);
                v = System.Numerics.Vector.Add(v, ui4);
            }
            long total = 0;
            for (int j = 0; j < Vector<uint>.Count; j++)
            {
                total += v[j];
            }
            for (int i = lastIndex; i < span.Length; i++)
            {
                total += span[i];
            }
            return total;
        }
        //↑と組み合わせて使う
        //Numerics、マルチスレッド、uint、Spanにしてシングルスレッドに渡して処理
        private long Test91_Nunerics_uint_MT(byte[] ary)
        {
            long total = 0;
            int simdLength = Vector<byte>.Count;
            //Span<byte> span = new Span<byte>(ary);
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount),
                (range) =>
                {
                    var s = new Span<byte>(ary);
                    long subtotal = Test81_Numerics_uint(s.Slice(range.Item1, range.Item2-range.Item1));
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }
        #endregion



        private void MyInitialize()
        {
            MyArray = new byte[ELEMENT_COUNT];

            //指定値で埋める
            var span = new Span<byte>(MyArray);
            span.Fill(255);

            //最後の要素
            //MyArray[ELEMENT_COUNT - 1] = 100;

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
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒 {total.ToString("N0")} | {func.Method.Name}");
        }


        //一斉テスト用
        private async void MyExeAll()
        {
            var sw = new Stopwatch();
            sw.Start();
            this.IsEnabled = false;
            await Task.Run(() => MyExe(Test1_Normal, Tb1, MyArray));
            await Task.Run(() => MyExe(Test2_Normal_MT, Tb2, MyArray));
            await Task.Run(() => MyExe(Test3_Normal4, Tb3, MyArray));
            await Task.Run(() => MyExe(Test4_Intrinsics_int, Tb4, MyArray));
            await Task.Run(() => MyExe(Test5_Intrinsics_int_MT, Tb5, MyArray));
            await Task.Run(() => MyExe(Test6_Intrinsics_int_MT2, Tb6, MyArray));
            await Task.Run(() => MyExe(Test7_Intrinsics_long_MT, Tb7, MyArray));
            await Task.Run(() => MyExe(Test8_Numerics_uint, Tb8, MyArray));
            await Task.Run(() => MyExe(Test9_Nunerics_uint_MT, Tb9, MyArray));
            await Task.Run(() => MyExe(Test10_Numerics_long_MT, Tb10, MyArray));

            this.IsEnabled = true;
            sw.Stop();
            TbAll.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒";
        }
        #endregion 時間計測

    }
}
