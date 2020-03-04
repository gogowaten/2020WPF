using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace _20200301_SIMDでドット積
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyArray;
        private const int LOOP_COUNT = 1000;
        private const int ELEMENT_COUNT = 10_000_000;// 1_056_831;// 132_103;// 2071;//要素数

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
            this.Title = this.ToString();



            MyTextBlock.Text = $"byte型配列要素数{ELEMENT_COUNT.ToString("N0")}のドット積を {LOOP_COUNT}回求める";
            MyTextBlockVectorCount.Text = $"Vector256<byte>.Count={Vector256<byte>.Count}  Vector<byte>.Count={Vector<byte>.Count}";
            MyTextBlockCpuThreadCount.Text = $"CPUスレッド数：{Environment.ProcessorCount}";



            ButtonAll.Click += (s, e) => MyExeAll();
            Button1.Click += (s, e) => MyExe(Test1_Normal, Tb1, MyArray);
            Button2.Click += (s, e) => MyExe(Test2_Numerics_Dot_long, Tb2, MyArray);
            Button3.Click += (s, e) => MyExe(Test3_Intrinsics_FMA_MultiplyAdd_float, Tb3, MyArray);
            Button4.Click += (s, e) => MyExe(Test4_Intrinsics_FMA_MultiplyAdd_double, Tb4, MyArray);
            Button5.Click += (s, e) => MyExe(Test5_Intrinsics_AVX_Multiply_Add_long, Tb5, MyArray);
            Button6.Click += (s, e) => MyExe(Test6_Intrinsics_SSE2_MultiplyAddAdjacent_int, Tb6, MyArray);
            Button7.Click += (s, e) => MyExe(Test7_Intrinsics_SSE41_DotProduct_float, Tb7, MyArray);
            Button8.Click += (s, e) => MyExe(Test8_Intrinsics_SSE41_DotProduct_float, Tb8, MyArray);
            //Button9.Click += (s, e) => MyExe(Test9_Intrinsics_SSE41_DotProduct_float, Tb9, MyArray);
            //Button10.Click += (s, e) => MyExe(Test11_Normal_MT, Tb10, MyArray);
            Button11.Click += (s, e) => MyExe(Test11_Normal_MT, Tb11, MyArray);
            Button12.Click += (s, e) => MyExe(Test12_Numerics_Dot_long_MT, Tb12, MyArray);
            Button13.Click += (s, e) => MyExe(Test13_Intrinsics_FMA_MultiplyAdd_float_MT, Tb13, MyArray);
            Button14.Click += (s, e) => MyExe(Test14_Intrinsics_FMA_MultiplyAdd_double_MT, Tb14, MyArray);
            Button15.Click += (s, e) => MyExe(Test15_Intrinsics_AVX_Multiply_Add_long_MT, Tb15, MyArray);
            Button16.Click += (s, e) => MyExe(Test16_Intrinsics_SSE2_MultiplyAddAdjacent_int_MT, Tb16, MyArray);
            Button17.Click += (s, e) => MyExe(Test17_Intrinsics_SSE41_DotProduct_float_MT, Tb17, MyArray);
            Button18.Click += (s, e) => MyExe(Test18_Intrinsics_SSE41_DotProduct_float_MT, Tb18, MyArray);
            //Button19.Click += (s, e) => MyExe(Test19_Intrinsics_SSE41_DotProduct_float_MT, Tb19, MyArray);
            //Button20.Click += (s, e) => MyExe(Test20, Tb20, MyArray);
            Button21.Click += (s, e) => MyExe(Test23_Intrinsics_FMA_MultiplyAdd_float_MT_Kai, Tb21, MyArray);
            Button22.Click += (s, e) => MyExe(Test26_Intrinsics_SSE2_MultiplyAddAdjacent_int_MT_Kai, Tb22, MyArray);
            Button23.Click += (s, e) => MyExe(Test28_Intrinsics_SSE41_DotProduct_float_MT_Kai, Tb23, MyArray);
            //Button24.Click += (s, e) => MyExe(Test28_Intrinsics_SSE41_DotProduct_float_MT_Kai, Tb24, MyArray);
        }

        //
        private long Test1_Normal(byte[] vs)
        {
            long total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return total;
        }

        //↑をマルチスレッド化
        private long Test11_Normal_MT(byte[] vs)
        {
            long total = 0;
            int rangeSize = vs.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, vs.Length, rangeSize),
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
        private long Test2_Numerics_Dot_long(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector<byte>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                System.Numerics.Vector.Widen(new Vector<byte>(vs, i),
                    out Vector<ushort> v1, out Vector<ushort> v2);
                System.Numerics.Vector.Widen(v1,
                    out Vector<uint> vv1, out Vector<uint> vv2);
                System.Numerics.Vector.Widen(v2,
                    out Vector<uint> vv3, out Vector<uint> vv4);
                total += System.Numerics.Vector.Dot(vv1, vv1);
                total += System.Numerics.Vector.Dot(vv2, vv2);
                total += System.Numerics.Vector.Dot(vv3, vv3);
                total += System.Numerics.Vector.Dot(vv4, vv4);
            }
            for (int i = lastIndex; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return total;
        }

        //↑をマルチスレッド化
        private long Test12_Numerics_Dot_long_MT(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector<byte>.Count;
            int rangeSize = vs.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, vs.Length, rangeSize),
                (range) =>
                {
                    long subtotal = 0;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    for (int i = range.Item1; i < lastIndex; i += simdLength)
                    {
                        System.Numerics.Vector.Widen(new Vector<byte>(vs, i),
                            out Vector<ushort> v1, out Vector<ushort> v2);
                        System.Numerics.Vector.Widen(v1,
                            out Vector<uint> vv1, out Vector<uint> vv2);
                        System.Numerics.Vector.Widen(v2,
                            out Vector<uint> vv3, out Vector<uint> vv4);
                        subtotal += System.Numerics.Vector.Dot(vv1, vv1);
                        subtotal += System.Numerics.Vector.Dot(vv2, vv2);
                        subtotal += System.Numerics.Vector.Dot(vv3, vv3);
                        subtotal += System.Numerics.Vector.Dot(vv4, vv4);
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i] * vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });

            return total;
        }



        #region ここからIntrinsics

        //誤差無しで計算できる最大要素数は2064まで。
        //これはVector256<float>でbyte型配列を計算する場合で、
        //floatの誤差なし最大値が16777215(24bit)とbyte配列が最大の255ってことで
        //16777215/255/255=258.01176
        //小数点以下切り捨てて258個、これにVectorCountの8をかけて
        //258*8=2064、これが限界。
        //あとはおまけでVectorCountで割り切れなかった余りの最大数7を足して
        //2064+7=2071
        //FMA MultiplyAddはVector256Double型でも計算できる
        //最大要素数は増えるけどVectorCountが半減するから遅くなるので
        //配列を分割してfloat型で計算するほうが効率が良さそう
        //Intrinsics FMA MultiplyAdd float
        private unsafe long Test3_Intrinsics_FMA_MultiplyAdd_float(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector256<int>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            Vector256<float> ff = Vector256.Create(0f);
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    Vector256<int> v = Avx2.ConvertToVector256Int32(p + i);
                    Vector256<float> f = Avx.ConvertToVector256Single(v);
                    ff = Fma.MultiplyAdd(f, f, ff);//float
                }
            }

            float* pp = stackalloc float[Vector256<float>.Count];
            Avx.Store(pp, ff);
            for (int i = 0; i < Vector256<float>.Count; i++)
            {
                total += (long)pp[i];
            }
            //割り切れなかった余り要素用
            for (int i = lastIndex; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return total;
        }

        //↑をマルチスレッド化
        //Intrinsics FMA MultiplyAdd float
        private unsafe long Test13_Intrinsics_FMA_MultiplyAdd_float_MT(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector256<int>.Count;
            int rangeSize = vs.Length / Environment.ProcessorCount;//1区分のサイズ
            Parallel.ForEach(Partitioner.Create(0, vs.Length, rangeSize),
                (range) =>
                {
                    long subtotal = 0;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    Vector256<float> ff = Vector256.Create(0f);
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            Vector256<int> v = Avx2.ConvertToVector256Int32(p + i);
                            Vector256<float> f = Avx.ConvertToVector256Single(v);
                            ff = Fma.MultiplyAdd(f, f, ff);//float
                        }
                    }
                    float* pp = stackalloc float[Vector256<float>.Count];
                    Avx.Store(pp, ff);
                    for (int i = 0; i < Vector256<float>.Count; i++)
                    {
                        subtotal += (long)pp[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i] * vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }

        //↑を改変
        //集計用のVector256<float>で誤差が出ないように配列を分割して計算
        //Intrinsics FMA MultiplyAdd float
        private unsafe long Test23_Intrinsics_FMA_MultiplyAdd_float_MT_Kai(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector256<int>.Count;
            //集計用のVector256<float>で扱える最大要素数 = 2064
            //これを1区分あたりの要素数(分割サイズ)にする
            //floatの仮数部24bit(16777215) * 8 / (255 * 255) = 2064.0941
            int rangeSize = ((1 << 24) - 1)
                            * Vector256<float>.Count
                            / (byte.MaxValue * byte.MaxValue);
            Parallel.ForEach(Partitioner.Create(0, vs.Length, rangeSize),
                (range) =>
                {
                    long subtotal = 0;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    Vector256<float> vTotal = Vector256.Create(0f);//集計用
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            Vector256<int> v = Avx2.ConvertToVector256Int32(p + i);
                            Vector256<float> f = Avx.ConvertToVector256Single(v);
                            vTotal = Fma.MultiplyAdd(f, f, vTotal);//float
                        }
                    }
                    float* pp = stackalloc float[Vector256<float>.Count];
                    Avx.Store(pp, vTotal);
                    for (int i = 0; i < Vector256<float>.Count; i++)
                    {
                        subtotal += (long)pp[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i] * vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }


        //Intrinsics FMA MultiplyAdd double
        private unsafe long Test4_Intrinsics_FMA_MultiplyAdd_double(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector128<int>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            Vector256<double> vTotal = Vector256.Create(0d);
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    Vector128<int> v = Sse41.ConvertToVector128Int32(p + i);
                    Vector256<double> f = Avx.ConvertToVector256Double(v);
                    vTotal = Fma.MultiplyAdd(f, f, vTotal);//double
                }
            }

            double* pp = stackalloc double[Vector256<double>.Count];
            Avx.Store(pp, vTotal);
            for (int i = 0; i < Vector256<double>.Count; i++)
            {
                total += (long)pp[i];
            }
            for (int i = lastIndex; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return total;
        }

        //↑をマルチスレッド化
        //Intrinsics FMA MultiplyAdd double
        private unsafe long Test14_Intrinsics_FMA_MultiplyAdd_double_MT(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector128<int>.Count;
            int rangeSize = vs.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, vs.Length, rangeSize),
                (range) =>
                {
                    long subtotal = 0;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    Vector256<double> vTotal = Vector256.Create(0d);
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            Vector128<int> v = Avx2.ConvertToVector128Int32(p + i);
                            Vector256<double> f = Avx.ConvertToVector256Double(v);
                            vTotal = Fma.MultiplyAdd(f, f, vTotal);//float
                        }
                    }
                    double* pp = stackalloc double[Vector256<double>.Count];
                    Avx.Store(pp, vTotal);
                    for (int i = 0; i < Vector256<double>.Count; i++)
                    {
                        subtotal += (long)pp[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i] * vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }


        //Intrinsics AVX Multiply + Add
        private unsafe long Test5_Intrinsics_AVX_Multiply_Add_long(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector256<int>.Count;
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
            for (int i = lastIndex; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return total;
        }

        //↑をマルチスレッド化
        //Intrinsics AVX Multiply + Add
        private unsafe long Test15_Intrinsics_AVX_Multiply_Add_long_MT(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector256<int>.Count;
            int rangeSize = vs.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, vs.Length, rangeSize),
                (range) =>
                {
                    long subtotal = 0;
                    int lastIndex =
                    range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    Vector256<long> vTotal = Vector256<long>.Zero;
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            Vector256<int> vv = Avx2.ConvertToVector256Int32(p + i);
                            Vector256<int> v1 = Avx2.UnpackHigh(vv, vv);
                            Vector256<int> v2 = Avx2.UnpackLow(vv, vv);
                            Vector256<long> t1 = Avx2.Multiply(v1, v1);//double,float,int,uint
                            Vector256<long> t2 = Avx2.Multiply(v2, v2);
                            vTotal = Avx2.Add(vTotal, t1);
                            vTotal = Avx2.Add(vTotal, t2);
                        }
                    }
                    long* pp = stackalloc long[Vector256<long>.Count];
                    Avx.Store(pp, vTotal);
                    for (int i = 0; i < Vector256<long>.Count; i++)
                    {
                        subtotal += pp[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i] * vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }


        //まともに計算できる最大要素数は132103まで。
        //これはVectorの各4要素の最大値がint.MaxValueの2147483647までだからで
        //byte配列が最大の255だった場合
        //2147483647/255/255=33025.508
        //小数点以下切り捨てて33025個、これにVectorCountの4をかけて
        //  33025*4=132100、これに余りの最大数3を足して、132100+3=132103。
        //Intrinsics SSE2 MultiplyAddAdjacent
        private unsafe long Test6_Intrinsics_SSE2_MultiplyAddAdjacent_int(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector128<short>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);

            Vector128<int> vTotal = Vector128<int>.Zero;
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    Vector128<short> v = Sse41.ConvertToVector128Int16(p + i);
                    Vector128<int> vv = Sse2.MultiplyAddAdjacent(v, v);// short + short                    
                    vTotal = Sse2.Add(vTotal, vv);
                }
            }
            simdLength = Vector128<int>.Count;
            int* pp = stackalloc int[simdLength];
            Sse2.Store(pp, vTotal);
            for (int i = 0; i < simdLength; i++)
            {
                total += pp[i];
            }
            for (int i = lastIndex; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return total;
        }

        //↑をマルチスレッド化
        //最大要素数は1_056_831まで(8スレッドCPU)
        //Intrinsics SSE2 MultiplyAddAdjacent
        private unsafe long Test16_Intrinsics_SSE2_MultiplyAddAdjacent_int_MT(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector128<short>.Count;//8
            int rangeSize = vs.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, vs.Length, rangeSize),
                (range) =>
                {
                    long subtotal = 0;
                    int lastIndex =
                    range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    Vector128<int> vTotal = Vector128<int>.Zero;
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            Vector128<short> v = Sse41.ConvertToVector128Int16(p + i);
                            Vector128<int> vv = Sse2.MultiplyAddAdjacent(v, v);// short + short                    
                            vTotal = Sse2.Add(vTotal, vv);
                        }
                    }

                    int* pp = stackalloc int[Vector128<int>.Count];
                    Sse2.Store(pp, vTotal);
                    for (int i = 0; i < Vector128<int>.Count; i++)
                    {
                        subtotal += pp[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i] * vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }

        //↑を改変
        //集計用のVector128<int>がオーバーフローしないように配列を分割して計算
        //Intrinsics SSE2 MultiplyAddAdjacent
        private unsafe long Test26_Intrinsics_SSE2_MultiplyAddAdjacent_int_MT_Kai(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector128<short>.Count;//8            
            //集計用のVector128<int>で
            //オーバーフローすることなく扱える最大要素数 = 132102
            //int.MaxValue / (byte.MaxValue * byte.MaxValue) * Vector128<int>.Count
            //2147483647 / (255 * 255) * 4 = 132102.03 小数点以下切り捨てで132102            
            int rangeSize =
                int.MaxValue / (byte.MaxValue * byte.MaxValue) * Vector128<int>.Count;

            Parallel.ForEach(Partitioner.Create(0, vs.Length, rangeSize),
                (range) =>
                {
                    long subtotal = 0;
                    int lastIndex =
                    range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    Vector128<int> vTotal = Vector128<int>.Zero;//集計用
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            Vector128<short> v = Sse41.ConvertToVector128Int16(p + i);
                            Vector128<int> vv = Sse2.MultiplyAddAdjacent(v, v);//short + short                    
                            vTotal = Sse2.Add(vTotal, vv);
                        }
                    }

                    int* pp = stackalloc int[Vector128<int>.Count];
                    Sse2.Store(pp, vTotal);
                    for (int i = 0; i < Vector128<int>.Count; i++)
                    {
                        subtotal += pp[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i] * vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }


        //        x86/x64 SIMD命令一覧表　（SSE～AVX2）
        //https://www.officedaytime.com/tips/simd.html
        //算術演算 ドット積 DPPS
        //Intrinsics SSE41 DotProduct
        private unsafe long Test7_Intrinsics_SSE41_DotProduct_float(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector128<int>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    Vector128<int> v = Sse41.ConvertToVector128Int32(p + i);
                    var vv = Sse2.ConvertToVector128Single(v);
                    //4要素全てを掛け算(5~8bit目を1)して、足し算した結果を0番目に入れる(1bit目を1)
                    Vector128<float> dp = Sse41.DotProduct(vv, vv, 0b11110001);
                    total += (long)dp.GetElement(0);
                }
            }
            for (int i = lastIndex; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return total;
        }

        //↑をマルチスレッド化
        private unsafe long Test17_Intrinsics_SSE41_DotProduct_float_MT(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector128<int>.Count;
            int rangeSize = vs.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, vs.Length, rangeSize),
                (range) =>
                {
                    long subtotal = 0;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            Vector128<int> v = Sse41.ConvertToVector128Int32(p + i);
                            var vv = Sse2.ConvertToVector128Single(v);
                            //4要素全てを掛け算(5~8bit目を1)して、足し算した結果を0番目に入れる(1bit目を1)
                            Vector128<float> dp = Sse41.DotProduct(vv, vv, 0b11110001);
                            //vTotal = Sse.Add(vTotal, dp);
                            subtotal += (long)dp.GetElement(0);
                        }
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i] * vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }


        //Intrinsics SSE41 DotProduct、ループの中で4個づつ処理
        private unsafe long Test8_Intrinsics_SSE41_DotProduct_float(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector128<int>.Count * 4;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            var vTotal = Vector128<float>.Zero;
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    Vector128<int> v = Sse41.ConvertToVector128Int32(p + i);
                    var vv = Sse2.ConvertToVector128Single(v);
                    //4要素全てを掛け算(5~8bit目を1)して、足し算した結果を0番目に入れる(1bit目を1)
                    Vector128<float> dp = Sse41.DotProduct(vv, vv, 0b11110001);
                    vTotal = Sse.Add(vTotal, dp);

                    v = Sse41.ConvertToVector128Int32(p + i + 4);
                    vv = Sse2.ConvertToVector128Single(v);
                    dp = Sse41.DotProduct(vv, vv, 0b11110010);//結果を1番目に入れる
                    vTotal = Sse.Add(vTotal, dp);

                    v = Sse41.ConvertToVector128Int32(p + i + 8);
                    vv = Sse2.ConvertToVector128Single(v);
                    dp = Sse41.DotProduct(vv, vv, 0b11110100);//結果を2番目に入れる
                    vTotal = Sse.Add(vTotal, dp);

                    v = Sse41.ConvertToVector128Int32(p + i + 12);
                    vv = Sse2.ConvertToVector128Single(v);
                    dp = Sse41.DotProduct(vv, vv, 0b11111000);//結果を3番目に入れる
                    vTotal = Sse.Add(vTotal, dp);

                }
            }
            float* f = stackalloc float[Vector128<int>.Count];
            Sse.Store(f, vTotal);
            for (int i = 0; i < Vector128<int>.Count; i++)
            {
                total += (long)f[i];
            }
            for (int i = lastIndex; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return total;
        }

        //↑をマルチスレッド化
        private unsafe long Test18_Intrinsics_SSE41_DotProduct_float_MT(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector128<int>.Count * 4;

            int rangeSize = vs.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, vs.Length, rangeSize),
                (range) =>
                {
                    var vTotal = Vector128<float>.Zero;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            Vector128<int> v = Sse41.ConvertToVector128Int32(p + i);
                            var vv = Sse2.ConvertToVector128Single(v);
                            //4要素全てを掛け算(5~8bit目を1)して、足し算した結果を0番目に入れる(1bit目を1)
                            Vector128<float> dp = Sse41.DotProduct(vv, vv, 0b11110001);
                            vTotal = Sse.Add(vTotal, dp);

                            v = Sse41.ConvertToVector128Int32(p + i + 4);//結果を1番目に入れる
                            vv = Sse2.ConvertToVector128Single(v);
                            dp = Sse41.DotProduct(vv, vv, 0b11110010);
                            vTotal = Sse.Add(vTotal, dp);

                            v = Sse41.ConvertToVector128Int32(p + i + 8);//結果を2番目に入れる
                            vv = Sse2.ConvertToVector128Single(v);
                            dp = Sse41.DotProduct(vv, vv, 0b11110100);
                            vTotal = Sse.Add(vTotal, dp);

                            v = Sse41.ConvertToVector128Int32(p + i + 12);//結果を3目に入れる
                            vv = Sse2.ConvertToVector128Single(v);
                            dp = Sse41.DotProduct(vv, vv, 0b11111000);
                            vTotal = Sse.Add(vTotal, dp);
                        }
                    }
                    long subtotal = 0;
                    float* f = stackalloc float[Vector128<float>.Count];
                    Sse.Store(f, vTotal);
                    for (int i = 0; i < Vector128<float>.Count; i++)
                    {
                        subtotal += (long)f[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i] * vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }
        //↑をオーバーフローしない程度に配列を分割して計算
        private unsafe long Test28_Intrinsics_SSE41_DotProduct_float_MT_Kai(byte[] vs)
        {
            long total = 0;
            int simdLength = Vector128<int>.Count * 4;

            //集計用のVector128<float> vTotalで扱える最大要素数 = 1032
            //floatの仮数部24bit / byte型最大値 * byte型最大値
            //16777215 / (255 * 255) * 4 = 1032.0471 これの小数点以下切り捨てを
            //1区分あたりの要素数(分割サイズ)
            int rangeSize =
                ((1 << 24) - 1) / (byte.MaxValue * byte.MaxValue) * Vector128<float>.Count;//1032

            Parallel.ForEach(
                Partitioner.Create(0, vs.Length, rangeSize),
                (range) =>
                {
                    var vTotal = Vector128<float>.Zero;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            Vector128<int> v = Sse41.ConvertToVector128Int32(p + i);
                            var vv = Sse2.ConvertToVector128Single(v);
                            //4要素全てを掛け算(5~8bit目を1)して、足し算した結果を0番目に入れる(1bit目を1)
                            Vector128<float> dp = Sse41.DotProduct(vv, vv, 0b11110001);
                            vTotal = Sse.Add(vTotal, dp);

                            v = Sse41.ConvertToVector128Int32(p + i + 4);
                            vv = Sse2.ConvertToVector128Single(v);
                            dp = Sse41.DotProduct(vv, vv, 0b11110010);//結果を1番目に入れる
                            vTotal = Sse.Add(vTotal, dp);

                            v = Sse41.ConvertToVector128Int32(p + i + 8);
                            vv = Sse2.ConvertToVector128Single(v);
                            dp = Sse41.DotProduct(vv, vv, 0b11110100);//結果を2番目に入れる
                            vTotal = Sse.Add(vTotal, dp);

                            v = Sse41.ConvertToVector128Int32(p + i + 12);
                            vv = Sse2.ConvertToVector128Single(v);
                            dp = Sse41.DotProduct(vv, vv, 0b11111000);//結果を3番目に入れる
                            vTotal = Sse.Add(vTotal, dp);
                        }
                    }
                    long subtotal = 0;
                    float* f = stackalloc float[Vector128<float>.Count];
                    Sse.Store(f, vTotal);
                    for (int i = 0; i < Vector128<float>.Count; i++)
                    {
                        subtotal += (long)f[i];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i] * vs[i];
                    }
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

            //0～255までを連番で繰り返し
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
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒 {total.ToString("N0")}  {func.Method.Name}");
        }


        //一斉テスト用
        private async void MyExeAll()
        {
            var sw = new Stopwatch();
            sw.Start();
            this.IsEnabled = false;
            await Task.Run(() => MyExe(Test1_Normal, Tb1, MyArray));
            await Task.Run(() => MyExe(Test2_Numerics_Dot_long, Tb2, MyArray));
            await Task.Run(() => MyExe(Test3_Intrinsics_FMA_MultiplyAdd_float, Tb3, MyArray));
            await Task.Run(() => MyExe(Test4_Intrinsics_FMA_MultiplyAdd_double, Tb4, MyArray));
            await Task.Run(() => MyExe(Test5_Intrinsics_AVX_Multiply_Add_long, Tb5, MyArray));
            await Task.Run(() => MyExe(Test6_Intrinsics_SSE2_MultiplyAddAdjacent_int, Tb6, MyArray));
            await Task.Run(() => MyExe(Test7_Intrinsics_SSE41_DotProduct_float, Tb7, MyArray));
            await Task.Run(() => MyExe(Test8_Intrinsics_SSE41_DotProduct_float, Tb8, MyArray));
            //await Task.Run(() => MyExe(Test9_Nunerics_uint_MT, Tb9, MyArray));
            //await Task.Run(() => MyExe(Test11_Normal_MT, Tb10, MyArray));
            await Task.Run(() => MyExe(Test11_Normal_MT, Tb11, MyArray));
            await Task.Run(() => MyExe(Test12_Numerics_Dot_long_MT, Tb12, MyArray));
            await Task.Run(() => MyExe(Test13_Intrinsics_FMA_MultiplyAdd_float_MT, Tb13, MyArray));
            await Task.Run(() => MyExe(Test14_Intrinsics_FMA_MultiplyAdd_double_MT, Tb14, MyArray));
            await Task.Run(() => MyExe(Test15_Intrinsics_AVX_Multiply_Add_long_MT, Tb15, MyArray));
            await Task.Run(() => MyExe(Test16_Intrinsics_SSE2_MultiplyAddAdjacent_int_MT, Tb16, MyArray));
            await Task.Run(() => MyExe(Test17_Intrinsics_SSE41_DotProduct_float_MT, Tb17, MyArray));
            await Task.Run(() => MyExe(Test18_Intrinsics_SSE41_DotProduct_float_MT, Tb18, MyArray));


            await Task.Run(() => MyExe(Test23_Intrinsics_FMA_MultiplyAdd_float_MT_Kai, Tb21, MyArray));
            await Task.Run(() => MyExe(Test26_Intrinsics_SSE2_MultiplyAddAdjacent_int_MT_Kai, Tb22, MyArray));
            await Task.Run(() => MyExe(Test28_Intrinsics_SSE41_DotProduct_float_MT_Kai, Tb23, MyArray));


            this.IsEnabled = true;
            sw.Stop();
            TbAll.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒";
        }
        #endregion 時間計測
    }
}
