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
using System.Runtime.InteropServices;
using System.Collections.Immutable;
using System.Diagnostics;



namespace _20200220_Vector256
{

    public partial class MainWindow : Window
    {
        //[StructLayout(LayoutKind.Sequential)]
        //struct MyStruct
        //{
        //    public int[] vs;
        //}
        byte[] MyByteArray;
        int[] MyIntArray;
        const int ElEMENT_COUNT = 1_000_001;
        const int LOOP_COUNT = 100;
        public MainWindow()
        {
            InitializeComponent();

            MyInitialize();

            MyTest1();

            float[] sum;
            float[] a = new float[100];
            float[] b = new float[100];
            float[] c = new float[100];
            for (int i = 0; i < 100; i++)
            {
                a[i] = i;
                b[i] = i;
                c[i] = 2f;
            }

            if (Avx.IsSupported)
            {
                sum = SimdAdd(a, b, 100);
                SimdAdd(c, c);
            }

            var v = Vector256.Create(256, 1, 23, 3, 3, 3, 3, 3);
            var bv = Vector256.Create(2, 2, 2, 2f);
            var vv = Vector256.Create(255, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 11, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
            byte[] bb = new byte[] {
                1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1 };
            var span = new ReadOnlySpan<byte>(bb);
            var neko = MemoryMarshal.Cast<byte, int>(span);


            var inu = a;


        }
        //        Hardware intrinsic in .NET Core 3.0 - Introduction
        //https://fiigii.com/2019/03/03/Hardware-intrinsic-in-NET-Core-3-0-Introduction/

        static unsafe float[] SimdAdd(float[] a, float[] b, int n)
        {
            float[] result = new float[n];
            fixed (float* ptr_a = a, ptr_b = b, ptr_res = result)
            {
                for (int i = 0; i < n; i += Vector256<float>.Count)
                {
                    Vector256<float> v1 = Avx.LoadVector256(ptr_a + i);
                    Vector256<float> v2 = Avx.LoadVector256(ptr_b + i);
                    Vector256<float> res = Avx.Add(v1, v2);
                    Avx.Store(ptr_res + i, res);
                }
            }
            return result;
        }
        static unsafe void SimdAdd(float[] a, float[] b)
        {
            int lastIndex = a.Length - (a.Length % Vector256<float>.Count);
            Vector256<float> v = Vector256.Create(0f);// new Vector256<float>();
            fixed (float* ptrA = a, ptrB = b)
            {
                for (int i = 0; i < lastIndex; i += Vector256<float>.Count)
                {
                    v = Avx.Add(v, Avx.LoadVector256(ptrA + i));
                }
            }
        }

        private unsafe void MyInitialize()
        {
            MyByteArray = new byte[ElEMENT_COUNT];
            var r = new Random();
            r.NextBytes(MyByteArray);

            MyIntArray = Enumerable.Range(1, ElEMENT_COUNT).ToArray();

            //MyStruct myStruct = new MyStruct();
            //myStruct.vs = Enumerable.Range(1, ElEMENT_COUNT).ToArray();



            Button1.Click += (s, e) => MyExe(MyTestAddByte, Tb1);
            Button2.Click += (s, e) => MyExe(MyTestAddIntLong, Tb2, MyIntArray);
            Button3.Click += (s, e) => MyExe(MyTestAddIntInt, Tb3, MyIntArray);
            


        }
        private void MyTest1()
        {
            byte[] aa = new byte[1000];
            for (int i = 0; i < aa.Length; i++)
            {
                aa[i] = 200;
            }

            //var r = new Random();
            //r.NextBytes(aa);

            var temp = Enumerable.Range(1, 32).ToArray().Select(x => (byte)x).ToArray();

            Vector256<int> neko = MyTestVectorAdd(temp);
            MyTestVectorTotalAddArray(neko);
            MyTestVectorTotalAddArrayLinq(neko);
            MyTestVectorTotalAddStackalloc(neko);

            MyTestToIntVector(temp);
            SimdAdd(aa);
            SimdAddByte2Int(aa);

        }
        //        A small overview of SIMD in .NET/C# / Habr
        //https://habr.com/en/post/467689/

        static unsafe void SimdAdd(byte[] a)
        {
            int simdLength = Vector256<byte>.Count;
            int lastIndex = a.Length - (a.Length % simdLength);
            Vector256<byte> v = new Vector256<byte>();
            int i;
            fixed (byte* ptrA = a)
            {
                for (i = 0; i < lastIndex; i += simdLength)
                {
                    var vv = Avx.LoadVector256(ptrA + i);
                    v = Avx2.Add(v, vv);

                    var inu = Avx2.ConvertToVector128Int32(ptrA + i);
                    var uma = Avx2.ConvertToVector256Int32(ptrA + i);

                }
            }
            var neko = v.ToScalar();
            int result = 0;
            var temp = stackalloc byte[simdLength];
            Avx.Store(temp, v);
            for (int j = 0; j < simdLength; j++)
            {
                result += temp[j];
            }
            for (; i < a.Length; i++)
            {
                result += a[i];
            }
        }

        //byte配列の合計、int型Vectorに変換してからAdd
        static unsafe void SimdAddByte2Int(byte[] a)
        {
            int simdLength = Vector256<int>.Count;
            int lastIndex = a.Length - (a.Length % simdLength);
            Vector256<int> iv = new Vector256<int>();
            int i;
            fixed (byte* ptrA = a)
            {
                for (i = 0; i < lastIndex; i += simdLength)
                {
                    Vector256<int> vv = Avx2.ConvertToVector256Int32(ptrA + i);
                    iv = Avx2.Add(iv, vv);
                }
            }

            int result = 0;
            var temp = stackalloc int[simdLength];
            Avx.Store(temp, iv);
            for (int j = 0; j < simdLength; j++)
            {
                result += temp[j];
            }
            for (; i < a.Length; i++)
            {
                result += a[i];
            }
        }

        //byte型配列からint型Vector
        private unsafe void MyTestToIntVector(byte[] a)
        {
            fixed (byte* ptrA = a)
            {
                //byte型配列のポインタからint型のVector256作成
                Vector256<short> v1 = Avx2.ConvertToVector256Int16(ptrA);
                Vector256<int> v2 = Avx2.ConvertToVector256Int32(ptrA);
                Vector256<long> v3 = Avx2.ConvertToVector256Int64(ptrA);
            }
        }

        private unsafe Vector256<int> MyTestVectorAdd(byte[] a)
        {
            var v = new Vector256<int>();//これも↓も同じ結果みたい
            var v0 = Vector256.Create(0);
            var vz = Vector256<int>.Zero;//これがいい、Zeroがかっこいい

            int simdLength = Vector256<int>.Count;
            fixed (byte* ptrA = a)
            {
                for (int i = 0; i < a.Length; i += simdLength)
                {
                    //byte型配列のポインタからint型のVector256作成
                    var v2 = Avx2.ConvertToVector256Int32(ptrA + i);
                    //Vectorで足し算
                    v = Avx2.Add(v, v2);
                    v0 = Avx2.Add(v0, v2);
                    vz = Avx2.Add(vz, v2);
                }
            }
            return v;
        }

        private unsafe int MyTestVectorTotalAddArrayLinq(Vector256<int> v)
        {
            var ii = new int[Vector256<int>.Count];
            fixed (int* ptrI = ii)
            {
                Avx2.Store(ptrI, v);
            }
            return ii.Sum();
        }

        private unsafe int MyTestVectorTotalAddArray(Vector256<int> v)
        {
            int simdLength = Vector256<int>.Count;
            var ii = new int[simdLength];
            fixed (int* ptrI = ii)
            {
                Avx2.Store(ptrI, v);
            }
            int total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += ii[i];
            }
            return total;

        }

        private unsafe int MyTestVectorTotalAddStackalloc(Vector256<int> v)
        {
            int total = 0;
            int simdLength = Vector256<int>.Count;
            int* temp = stackalloc int[simdLength];
            Avx2.Store(temp, v);
            for (int i = 0; i < simdLength; i++)
            {
                total += temp[i];
            }
            return total;
        }

        private unsafe long MyTestAdd(byte[] a)
        {
            var vz = Vector256<int>.Zero;//0で初期化したVector256
            int simdLength = Vector256<int>.Count;
            fixed (byte* ptrA = a)
            {
                for (int i = 0; i < a.Length; i += simdLength)
                {
                    //byte型配列のポインタからint型のVector256作成
                    var v2 = Avx2.ConvertToVector256Int32(ptrA + i);
                    //Vectorで足し算
                    vz = Avx2.Add(vz, v2);
                }
            }
            var temp = new int[simdLength];
            //Vectorの値を配列にコピー？
            fixed (int* ptrI = temp)
            {
                Avx.Store(ptrI, vz);
            }
            return temp.Sum();//LINQで配列の合計
        }

        private unsafe long MyTestAddByte(byte[] a)
        {
            var vz = Vector256<int>.Zero;//0で初期化したVector256
            int simdLength = Vector256<int>.Count;
            int i = 0;
            fixed (byte* ptrA = a)
            {
                for (i = 0; i < a.Length; i += simdLength)
                {
                    //byte型配列のポインタからint型のVector256作成
                    var v2 = Avx2.ConvertToVector256Int32(ptrA + i);
                    //Vectorで足し算
                    vz = Avx2.Add(vz, v2);
                }
            }

            var temp = new int[simdLength];
            //Vectorの値を配列にStore(コピー？)
            fixed (int* ptrI = temp)
            {
                Avx.Store(ptrI, vz);
            }
            long total = temp.Sum();//LINQで配列の合計

            //SIMDLengthで割り切れなかった余りの要素を合計
            for (; i < a.Length; i++)
            {
                total += a[i];
            }

            return total;
        }

        private unsafe long MyTestAddIntLong(int[] a)
        {
            var vz = Vector256<long>.Zero;//0で初期化したVector256
            int simdLength = Vector256<long>.Count;
            int i = 0;
            fixed (int* ptrA = a)
            {
                for (i = 0; i < a.Length; i += simdLength)
                {

                    //int型配列のポインタからint型のVector256作成
                    //var v2 = Avx.LoadVector256(ptrA + i);//合計値がint型で収まるならこれでいい
                    //int型配列のポインタからlong型のVector256作成
                    var v2 = Avx2.ConvertToVector256Int64(ptrA + i);
                    //Vectorで足し算
                    vz = Avx2.Add(vz, v2);
                }
            }

            var temp = new long[simdLength];
            //Vectorの値を配列にStore(コピー？)
            fixed (long* ptrI = temp)
            {
                Avx.Store(ptrI, vz);
            }

            //long型だと要素数は4つだから合計はLINQのSumで十分、どれも速度は同じだった
            //LINQで合計
            long total = temp.Sum();

            //Forで合計
            //long total = 0;
            //for (int j = 0; j < simdLength; j++)
            //{
            //    total += temp[j];
            //}

            //決め打ちで合計
            //long total = 0;
            //total += temp[0];
            //total += temp[1];
            //total += temp[2];
            //total += temp[3];

            //SIMDLengthで割り切れなかった余りの要素を合計
            for (; i < a.Length; i++)
            {
                total += a[i];
            }

            return total;
        }

        private unsafe long MyTestAddIntInt(int[] a)
        {
            var vz = Vector256<int>.Zero;//0で初期化したVector256
            int simdLength = Vector256<int>.Count;
            int i = 0;
            fixed (int* ptrA = a)
            {
                for (i = 0; i < a.Length; i += simdLength)
                {
                    //int型配列のポインタからint型のVector256作成
                    var v2 = Avx.LoadVector256(ptrA + i);//合計値がint型で収まるならこれでいい
                    //var v2 = Avx.LoadAlignedVector256(ptrA + i);
                    //Vectorで足し算
                    vz = Avx2.Add(vz, v2);
                }
            }

            var temp = new int[simdLength];
            //Vectorの値を配列にStore(コピー？)
            fixed (int* ptrI = temp)
            {
                Avx.Store(ptrI, vz);
            }


            //LINQで合計
            long total = temp.Sum(x => (long)x);

            //Forで合計
            //long total = 0;
            //for (int j = 0; j < simdLength; j++)
            //{
            //    total += temp[j];
            //}

            //決め打ちで合計
            //long total = 0;
            //total += temp[0];
            //total += temp[1];
            //total += temp[2];
            //total += temp[3];
            //total += temp[4];
            //total += temp[5];
            //total += temp[6];
            //total += temp[7];


            //SIMDLengthで割り切れなかった余りの要素を合計
            for (; i < a.Length; i++)
            {
                total += a[i];
            }

            return total;
        }

     







        private void MyExe(Func<byte[], long> func, TextBlock tb)
        {
            long total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(MyByteArray);
            }
            sw.Stop();
            tb.Text = $"合計値={total} {sw.Elapsed.TotalSeconds.ToString("00.000")}秒";
        }
        private void MyExe(Func<int[], long> func, TextBlock tb, int[] vs)
        {
            long total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(vs);
            }
            sw.Stop();
            tb.Text = $"合計値={total} {sw.Elapsed.TotalSeconds.ToString("00.000")}秒";
        }

       


    }
}
