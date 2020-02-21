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

namespace _20200220_Vector256
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

         
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

        private void MyTest1()
        {
            byte[] aa = new byte[1000];
            for (int i = 0; i < aa.Length; i++)
            {
                aa[i] = 200;
            }

            //var r = new Random();
            //r.NextBytes(aa);

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
                    var uma= Avx2.ConvertToVector256Int32(ptrA + i);
                    
                }
            }
            var neko= v.ToScalar();
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




    }
}
