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


namespace _20200224_Intrinsics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyArray;
        private byte[] MyArray0to255;

        private const int ELEMENT_COUNT = 100_000_001;// 538_976_319;
        //private const int ELEMENT_COUNT = 67_372_032;Test1の最大有効要素数
        //private const int ELEMENT_COUNT = 538_976_319;//Test2の最大有効要素数
        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();

            TestAddSum(MyArray0to255);

            var neko = int.MaxValue;
            //var t1 = Test1(MyArray);
            //var t2 = Test2(MyArray);
            //var t3 = Test3MinMax(MyArray);
            //var t4 = Test4MinMax(MyArray);
            var t5 = Test5(MyArray);
            var t6 = Test6Variance(MyArray);
            var t7 = Test7Variance(MyArray);
            var t51 = Test5M(MyArray);


        }

        /// <summary>
        /// 合計値を返す、要素数67_372_032(6737万)まで計算できる、それ以上だと桁あふれの可能性が出る、合計値だと17,179,868,160(171億)程度
        /// </summary>
        /// <param name="vs"></param>
        /// <returns></returns>
        private unsafe long Test1(byte[] vs)
        {
            var v = Vector256<int>.Zero;
            int simdLength = Vector256<int>.Count;
            int i;
            fixed (byte* p = vs)
            {
                for (i = 0; i < vs.Length; i += simdLength)
                {
                    var vv = Avx2.ConvertToVector256Int32(p + i);
                    v = Avx2.Add(v, vv);
                }
            }

            long total = 0;
            int* temp = stackalloc int[simdLength];
            Avx.Store(temp, v);
            for (int j = 0; j < simdLength; j++)
            {
                total += temp[j];
            }

            for (; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total;
        }

        /// <summary>
        /// 合計値を返す、要素数538_976_319(5億3897万)まで計算できる、合計値だと137,438,961,345(1374億)程度
        /// </summary>
        /// <param name="vs"></param>
        /// <returns></returns>
        //Add Intrinsics + MultiThread
        private unsafe long Test2(byte[] vs)
        {
            int simdLength = Vector256<int>.Count;
            long total = 0;
            var bag = new ConcurrentBag<long>();

            var partition = Partitioner.Create(0, vs.Length, vs.Length / Environment.ProcessorCount);
            Parallel.ForEach(partition,
                (range) =>
                {
                    fixed (byte* p = vs)
                    {
                        long subtotal = 0;
                        var v = Vector256<int>.Zero;
                        int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            var vv = Avx2.ConvertToVector256Int32(p + i);
                            v = Avx2.Add(v, vv);
                        }
                        int* temp = stackalloc int[simdLength];
                        Avx.Store(temp, v);
                        for (int j = 0; j < simdLength; j++)
                        {
                            subtotal += temp[j];
                        }
                        for (int k = lastIndex; k < range.Item2; k++)
                        {
                            subtotal += vs[k];
                        }
                        System.Threading.Interlocked.Add(ref total, subtotal);
                        bag.Add(subtotal);
                    }
                });
            return total;
        }

        //MinMax Intrinsics
        private unsafe (byte min, byte max) Test3MinMax(byte[] vs)
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
        //分散 = 2乗の平均 - 平均の2乗
        private unsafe (byte min, byte max) Test4MinMax(byte[] vs)
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
                    for (int i = lastIndex; i < vs.Length; i++)
                    {
                        if (min > vs[i]) min = vs[i];
                        if (max < vs[i]) max = vs[i];
                    }
                    bag.Add(min);
                    bag.Add(max);
                });

            return (bag.Min(), bag.Max());
        }

        //256intの掛け算だと1つ飛びでしか掛け算されない
        //256floatならできるけど合計で桁あふれする
        //floatからlongはコンバートない
        //floatで掛け算して、doubleで足し算
        //も良さそうだけどunpackメソッドでintを2つのintに分けて掛け算でいいかも
        //要素数1億でも計算できる
        private unsafe double Test5(byte[] vs)
        {

            int simdLength = Vector256<int>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            var vTotal = Vector256<long>.Zero;
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    Vector256<int> v = Avx2.ConvertToVector256Int32(p + i);//01234567
                    //Vector256<float> inu = Avx.ConvertToVector256Single(v);
                    //Vector256<float> vv = Avx.Multiply(inu, inu);
                    //var neko = Avx.ConvertToVector256Int32(vv);
                    //vTotal = Fma.MultiplyAdd(vv, vv, vTotal);
                    Vector256<int> neko = Avx2.UnpackHigh(v, v);//22336677
                    Vector256<int> uma = Avx2.UnpackLow(v, v);//  00114455
                    Vector256<long> nn = Avx2.Multiply(neko, neko);//4 9 36 49
                    Vector256<long> uu = Avx2.Multiply(uma, uma);//  0 1 16 25
                    vTotal = Avx2.Add(vTotal, nn);
                    vTotal = Avx2.Add(vTotal, uu);//                 4 10 52 74
                }
            }

            long total = 0;
            simdLength = Vector256<long>.Count;
            long* temp = stackalloc long[simdLength];
            Avx.Store(temp, vTotal);
            for (int j = 0; j < simdLength; j++) { total += temp[j]; }

            for (int i = lastIndex; i < vs.Length; i++) { total += vs[i] * vs[i]; }

            double average = (double)Test2(vs) / vs.Length;
            return ((double)total / vs.Length) - (average * average);
        }

        //floatで掛け算、足し算
        //これだと要素数10万程度でも誤差が出てくる
        private unsafe double Test6Variance(byte[] vs)
        {

            int simdLength = Vector256<int>.Count;
            int i;
            var vTotal = Vector256<float>.Zero;
            fixed (byte* p = vs)
            {
                for (i = 0; i < vs.Length; i += simdLength)
                {
                    Vector256<int> v = Avx2.ConvertToVector256Int32(p + i);//01234567
                    Vector256<float> inu = Avx.ConvertToVector256Single(v);
                    Vector256<float> vv = Avx.Multiply(inu, inu);
                    vTotal = Avx.Add(vTotal, vv);
                    //var neko = Avx.ConvertToVector256Int32(vv);
                    //vTotal = Fma.MultiplyAdd(vv, vv, vTotal);
                }
            }

            double total = 0;
            simdLength = Vector256<float>.Count;
            float* temp = stackalloc float[simdLength];
            Avx.Store(temp, vTotal);
            for (int j = 0; j < simdLength; j++) { total += temp[j]; }
            for (; i < vs.Length; i++) { total += vs[i]; }

            double average = (double)Test2(vs) / vs.Length;
            return (total / vs.Length) - (average * average);
        }

        //floatで掛け算して、intで足し算
        //これだと要素数100万程度で桁あふれする
        private unsafe double Test7Variance(byte[] vs)
        {

            int simdLength = Vector256<int>.Count;
            int i;
            var vTotal = Vector256<int>.Zero;
            fixed (byte* p = vs)
            {
                for (i = 0; i < vs.Length; i += simdLength)
                {
                    Vector256<int> v = Avx2.ConvertToVector256Int32(p + i);//01234567
                    Vector256<float> inu = Avx.ConvertToVector256Single(v);
                    Vector256<float> vv = Avx.Multiply(inu, inu);
                    v = Avx.ConvertToVector256Int32(vv);
                    vTotal = Avx2.Add(vTotal, v);
                }
            }

            long total = 0;
            simdLength = Vector256<int>.Count;
            int* temp = stackalloc int[simdLength];
            Avx.Store(temp, vTotal);
            for (int j = 0; j < simdLength; j++) { total += temp[j]; }
            for (; i < vs.Length; i++) { total += vs[i]; }

            double average = (double)Test2(vs) / vs.Length;
            return ((double)total / vs.Length) - (average * average);
        }

        //Test5のマルチスレッド化
        //要素数1億でも計算できるTest5のCPUスレッド数倍程度まで計算できるはず
        private unsafe double Test5M(byte[] vs)
        {
            long total = 0;
            OrderablePartitioner<Tuple<int, int>> rangeSize
                = Partitioner.Create(0, vs.Length, vs.Length / Environment.ProcessorCount);
            Parallel.ForEach(rangeSize,
                (range) =>
                {
                    int simdLength = Vector256<int>.Count;
                    int lastIndex = range.Item2 - ((range.Item2 - range.Item1) % simdLength);
                    var vTotal = Vector256<long>.Zero;
                    fixed (byte* p = vs)
                    {
                        for (int i = range.Item1; i < lastIndex; i += simdLength)
                        {
                            Vector256<int> v = Avx2.ConvertToVector256Int32(p + i);//01234567
                            Vector256<int> neko = Avx2.UnpackHigh(v, v);//22336677
                            Vector256<int> uma = Avx2.UnpackLow(v, v);//  00114455
                            Vector256<long> nn = Avx2.Multiply(neko, neko);//4 9 36 49
                            Vector256<long> uu = Avx2.Multiply(uma, uma);//  0 1 16 25
                            vTotal = Avx2.Add(vTotal, nn);
                            vTotal = Avx2.Add(vTotal, uu);//                 4 10 52 74
                        }
                    }
                    long subtotal = 0;
                    simdLength = Vector256<long>.Count;
                    long* temp = stackalloc long[simdLength];
                    Avx.Store(temp, vTotal);
                    for (int j = 0; j < simdLength; j++)
                    {
                        subtotal += temp[j];
                    }
                    for (int i = lastIndex; i < range.Item2; i++)
                    {
                        subtotal += vs[i] * vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });

            double average = (double)Test2(vs) / vs.Length;
            return ((double)total / vs.Length) - (average * average);
        }

        private unsafe void TestAddSum(byte[] vs)
        {

            fixed (byte* p = vs)
            {
                var v = Avx.LoadVector256(p);
                var v2 = Avx.LoadVector256(p + 32);
                //Avx.MultipleSumAbsoluteDifferences;
                Vector256<int> i1 = Avx2.ConvertToVector256Int32(p);
                Vector256<float> f1 = Avx.ConvertToVector256Single(i1);
                Vector256<float> m1 = Avx.Multiply(f1, f1);

                Vector128<int> i128 = Sse41.ConvertToVector128Int32(p);
                Vector256<double> d256 = Avx.ConvertToVector256Double(i128);
                var dZero = Vector256<double>.Zero;
                Vector256<double> ma1 = Fma.MultiplyAdd(d256, d256, dZero);

                var i256 = Avx2.ConvertToVector256Int32(p);
                var f256 = Avx.ConvertToVector256Single(i256);
                var fZero = Vector256<float>.Zero;
                var ma2 = Fma.MultiplyAdd(f256, f256, fZero);

                Vector128<float> s128 = Sse2.ConvertToVector128Single(i128);
                Vector128<float> ms = Sse.MultiplyScalar(s128, s128);

                var neko = 0;
                //Avx.MultiplyAddAdjacent;
                //Avx.MultiplyHigh;
                //Avx.MultiplyHighRoundScale;
                //Avx.MultiplyLow;
                //Avx.MultiplyScalar;
                //Fma.MultiplyAdd;
                //Fma.MultiplyAddNegated;
                //Fma.MultiplyAddNegatedScalar;
                //Fma.MultiplyAddScalar;
                //Fma.MultiplyAddSubtract;
                //Fma.MultiplySubtract;
                //Fma.MultiplySubtractAdd;
                //Fma.MultiplySubtractNegated;
                //Fma.MultiplySubtractNegatedScalar;
                //Fma.MultiplySubtractScalar;

            }

        }






        private void MyInitialize()
        {
            MyArray = new byte[ELEMENT_COUNT];

            //var span = new Span<byte>(MyArray);
            //span.Fill(255);

            //var r = new Random();
            //r.NextBytes(MyArray);

            //for (int i = 0; i < ELEMENT_COUNT; i++)
            //{
            //    MyArray[i] = (byte)i;
            //}

            byte b = 0;
            for (int i = 0; i < ELEMENT_COUNT; i++)
            {
                MyArray[i] = b;
                b++;
            }
            MyArray0to255 = new byte[ELEMENT_COUNT];
            for (int i = 0; i < ELEMENT_COUNT; i++)
            {
                MyArray0to255[i] = (byte)i;
            }
        }
    }
}
