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


namespace _20200223_Intrinsics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyArray;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();

            MyArray = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                MyArray[i] = (byte)i;
            }

            Test1VectorCreate();
            Test2Load(MyArray);
            Test3AddOverflow(MyArray);
            Test4Convert(MyArray);
            Test5Add(MyArray);
            Test6Add(MyArray);
            Test7AvxStore();
            Test8AvxStore();
            tb1.Text = $"0から255まで連番の合計 = {Test9Add(MyArray).ToString("N0")}";
        }

        //byte型配列の合計値
        private unsafe int Test9Add(byte[] aa)
        {
            var v = Vector256<int>.Zero;
            int simdLength = Vector256<int>.Count;
            int i;
            fixed (byte* p = aa)
            {
                for (i = 0; i < aa.Length; i += simdLength)
                {
                    Vector256<int> vv = Avx2.ConvertToVector256Int32(p + i);
                    v = Avx2.Add(v, vv);
                }
            }
            //Vectorから配列にして合計
            int* temp = stackalloc int[simdLength];
            Avx.Store(temp, v);
            int total = 0;
            for (int k = 0; k < simdLength; k++)
            {
                total += temp[k];
            }
            //Vectorの要素数で割り切れなかった余りの配列要素も合計
            for (; i < aa.Length; i++)
            {
                total += aa[i];
            }
            return total;
        }

        //Vectorから配列に変換というか値をコピーその2
        private unsafe void Test8AvxStore()
        {
            Vector256<int> v = Vector256.Create(100);
            int simdLength = Vector256<int>.Count;
            int* temp = stackalloc int[simdLength];
            Avx.Store(temp, v);
            int total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += temp[i];
            }
        }

        //Vectorから配列に変換というか値をコピーその1
        private unsafe void Test7AvxStore()
        {
            Vector256<int> v = Vector256.Create(100);
            int simdLength = Vector256<int>.Count;
            int[] temp = new int[simdLength];
            fixed (int* p = temp)
            {
                Avx.Store(p, v);
            }
            int total = temp.Sum();
        }

        //配列の足し算
        private unsafe void Test6Add(byte[] aa)
        {
            Vector256<int> total = Vector256<int>.Zero;
            fixed (byte* ptrA = aa)
            {
                for (int i = 0; i < aa.Length; i += Vector256<int>.Count)
                {
                    Vector256<int> tempV = Avx2.ConvertToVector256Int32(ptrA + i);
                    total = Avx2.Add(total, tempV);
                }
            }
        }

        //オーバーフローしないように、byte型配列からint型Vector作成して足し算
        private unsafe void Test5Add(byte[] aa)
        {
            Vector256<int> v = Vector256.Create((int)250);
            Vector256<int> total;
            fixed (byte* ptrA = aa)
            {
                Vector256<int> tempV = Avx2.ConvertToVector256Int32(ptrA);
                total = Avx2.Add(v, tempV);
            }
        }

        //byte型配列から他の整数型Vectorへの変換
        private unsafe void Test4Convert(byte[] aa)
        {
            fixed (byte* ptrA = aa)
            {
                var v1 = Avx2.ConvertToVector256Int16(ptrA);//short
                var v2 = Avx2.ConvertToVector256Int32(ptrA);//int
                var v3 = Avx2.ConvertToVector256Int64(ptrA);//long
            }
        }

        //Vectorでの足し算、オーバーフロー編
        private unsafe void Test3AddOverflow(byte[] aa)
        {
            Vector256<byte> v = Vector256.Create((byte)250);
            Vector256<byte> total;
            fixed (byte* ptrA = aa)
            {
                Vector256<byte> tempV = Avx.LoadVector256(ptrA);
                total = Avx2.Add(v, tempV);
            }
        }


        //配列からVector作成
        private unsafe void Test2Load(byte[] aa)
        {
            fixed (byte* p = aa)
            {
                var vv = Avx.LoadVector256(p);
                vv = Avx.LoadVector256(p + 1);
                vv = Avx.LoadVector256(p + 2);
                vv = Avx.LoadVector256(p + 32);
            }
        }

        //値からVector作成
        private void Test1VectorCreate()
        {
            Vector256<long> a = Vector256.Create(111, 1, 1, 1);
            Vector256<int> b = Vector256.Create(1322, 1, 1, 1, 1, 1, 1, 1);
            Vector256<short> c = Vector256.Create(54, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 16);
            Vector256<sbyte> d = Vector256.Create(87, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 16, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 32);
            Vector256<double> e = Vector256.Create(1, 1, 1, 1f);
            Vector256<float> f = Vector256.Create(32, 1, 1, 1f, 1, 1, 1, 1);

            var g = Vector256<int>.Zero;//int 0
            var h = Vector256.Create(2);//int
            var i = Vector256.Create(2f);//floar
            var j = Vector256.Create(2d);//double
            var k = Vector256.Create((byte)2);//byte

            //var x = Vector256.Create(1, 1);//エラー
            //整数型なら要素数1,4,8,16,32以外はエラー
            //小数点有りなら1,4,8以外はエラー
        }




    }
}
