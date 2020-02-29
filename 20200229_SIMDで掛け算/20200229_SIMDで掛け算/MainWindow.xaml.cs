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

namespace _20200229_SIMDで掛け算
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyArray;
        private const int LOOP_COUNT = 1;
        private const int ELEMENT_COUNT = 10;// 538_976_319;// 67_372_039;//要素数

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
            this.Title = this.ToString();




            MyTextBlock.Text = $"byte型配列要素数{ELEMENT_COUNT.ToString("N0")}の合計値を {LOOP_COUNT}回求める";
            MyTextBlockVectorCount.Text = $"Vector256<byte>.Count={Vector256<byte>.Count}  Vector<byte>.Count={Vector<byte>.Count}";
            MyTextBlockCpuThreadCount.Text = $"CPUスレッド数：{Environment.ProcessorCount}";

            Test1_Normal(MyArray);
            Test2_(MyArray);

            //ButtonAll.Click += (s, e) => MyExeAll();
            //Button1.Click += (s, e) => MyExe(Test1_Normal, Tb1, MyArray);
            //Button2.Click += (s, e) => MyExe(Test2_Normal_MT, Tb2, MyArray);
            //Button3.Click += (s, e) => MyExe(Test3_Normal4, Tb3, MyArray);
            //Button4.Click += (s, e) => MyExe(Test4_Intrinsics_int, Tb4, MyArray);
            //Button5.Click += (s, e) => MyExe(Test5_Intrinsics_int_MT, Tb5, MyArray);
            //Button6.Click += (s, e) => MyExe(Test6_Intrinsics_int_MT2, Tb6, MyArray);
            //Button7.Click += (s, e) => MyExe(Test7_Intrinsics_long_MT, Tb7, MyArray);
            //Button8.Click += (s, e) => MyExe(Test8_Numerics_uint, Tb8, MyArray);
            //Button9.Click += (s, e) => MyExe(Test9_Nunerics_uint_MT, Tb9, MyArray);
            //Button10.Click += (s, e) => MyExe(Test10_Numerics_long_MT, Tb10, MyArray);

        }

        //普通に掛け算
        private long Test1_Normal(byte[] vs)
        {
            long total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return total;
        }

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
                        subtotal += vs[i] * vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }




        //Numerics Dot
        private long Test3_(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector<byte>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                System.Numerics.Vector.Widen(new Vector<byte>(vs[i]), out Vector<ushort> v1, out Vector<ushort> v2);
                System.Numerics.Vector.Widen(v1, out Vector<uint> vv1, out Vector<uint> vv2);
                System.Numerics.Vector.Widen(v2, out Vector<uint> vv3, out Vector<uint> vv4);
                total += System.Numerics.Vector.Dot(vv1, vv1);
                total += System.Numerics.Vector.Dot(vv2, vv2);
                total += System.Numerics.Vector.Dot(vv3, vv3);
                total += System.Numerics.Vector.Dot(vv4, vv4);
            }
            return total;

        }

        //Intrinsics FMA MultiplyAdd
        private unsafe long Test4(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector256<byte>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            Vector256<float> ff = Vector256.Create(0f);
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    Vector256<int> v = Avx2.ConvertToVector256Int32(p + i);
                    Vector256<float> f = Avx.ConvertToVector256Single(v);
                    ff = Fma.MultiplyAdd(f, f, ff);//float,double
                }
            }

            float* pp = stackalloc float[Vector256<float>.Count];
            Avx.Store(pp, ff);
            for (int i = 0; i < Vector256<float>.Count; i++)
            {
                total += (long)pp[i];
            }
            return total;
        }

        //Intrinsics AVX Multiply + Add
        private unsafe long Test5(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector256<byte>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            Vector256<long> ff = Vector256<long>.Zero;
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    Vector256<int> vv = Avx2.ConvertToVector256Int32(p + i);
                    Vector256<int> v1 = Avx2.UnpackHigh(vv, vv);
                    Vector256<int> v2 = Avx2.UnpackLow(vv, vv);
                    Vector256<long> t1 = Avx2.Multiply(v1, v1);//double,float,int,uint
                    Vector256<long> t2 = Avx2.Multiply(v2, v2);
                    ff = Avx2.Add(ff, t1);
                    ff = Avx2.Add(ff, t2);
                }
            }
            simdLength = Vector256<long>.Count;
            long* pp = stackalloc long[simdLength];
            Avx.Store(pp, ff);
            for (int i = 0; i < simdLength; i++)
            {
                total += pp[i];
            }
            return total;
        }

        private unsafe long Test6(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector128<byte>.Count * 2;//2個同時のほうが速い？
            int lastIndex = vs.Length - (vs.Length % simdLength);

            Vector128<int> vi = Vector128<int>.Zero;
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    Vector128<short> sh1 = Sse41.ConvertToVector128Int16(p + i);
                    Vector128<short> sh2 = Sse41.ConvertToVector128Int16(p + i + Vector128<byte>.Count);
                    Vector128<int> ii1 = Sse2.MultiplyAddAdjacent(sh1, sh1);//byte + sbyte, short + short
                    Vector128<int> ii2 = Sse2.MultiplyAddAdjacent(sh2, sh2);//byte + sbyte, short + short
                    vi = Sse2.Add(vi, ii1);
                    vi = Sse2.Add(vi, ii2);
                }
            }
            simdLength = Vector128<int>.Count;
            int* pp = stackalloc int[simdLength];
            Avx.Store(pp, vi);
            for (int i = 0; i < simdLength; i++)
            {
                total += pp[i];
            }
            return total;
        }





        private void MyInitialize()
        {
            MyArray = new byte[ELEMENT_COUNT];

            ////指定値で埋める
            //var span = new Span<byte>(MyArray);
            //span.Fill(255);

            //最後の要素
            //MyArray[ELEMENT_COUNT - 1] = 100;

            //ランダム値
            //var r = new Random();
            //r.NextBytes(MyArray);

            //0～255までを連番で繰り返し
            for (int i = 0; i < ELEMENT_COUNT; i++)
            {
                MyArray[i] = (byte)i;
            }


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
            //await Task.Run(() => MyExe(Test1_Normal, Tb1, MyArray));
            //await Task.Run(() => MyExe(Test2_Normal_MT, Tb2, MyArray));
            //await Task.Run(() => MyExe(Test3_Normal4, Tb3, MyArray));
            //await Task.Run(() => MyExe(Test4_Intrinsics_int, Tb4, MyArray));
            //await Task.Run(() => MyExe(Test5_Intrinsics_int_MT, Tb5, MyArray));
            //await Task.Run(() => MyExe(Test6_Intrinsics_int_MT2, Tb6, MyArray));
            //await Task.Run(() => MyExe(Test7_Intrinsics_long_MT, Tb7, MyArray));
            //await Task.Run(() => MyExe(Test8_Numerics_uint, Tb8, MyArray));
            //await Task.Run(() => MyExe(Test9_Nunerics_uint_MT, Tb9, MyArray));
            //await Task.Run(() => MyExe(Test10_Numerics_long_MT, Tb10, MyArray));

            this.IsEnabled = true;
            sw.Stop();
            TbAll.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒";
        }
        #endregion 時間計測
    }
}
