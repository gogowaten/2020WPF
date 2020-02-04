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
using System.Diagnostics;


namespace _20200203_int配列から4要素のlong配列作成
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int[] MyAry;
        private int LongVectorCount;
        private long[] MyLongAry;
        private int LastIndex;
        private Vector<long> MyVector;
        public MainWindow()
        {
            InitializeComponent();

            MyAry = Enumerable.Range(1, 1_000_000).ToArray();
            LongVectorCount = Vector<long>.Count;
            MyLongAry = new long[LongVectorCount];
            LastIndex = MyAry.Length - (MyAry.Length % LongVectorCount);
            MyVector = new Vector<long>();
            this.Title = $"Vector<long>.Count = {LongVectorCount}  |  {MyAry.Length.ToString("N0")}要素、ループ10回";
            

            Button1.Click += (s, e) =>
            {
                Measure(Test1, Tb1);//一番早いけどメモリ消費が2倍                
            };
            Button2.Click += (s, e) => Measure(Test11, Tb2);
            Button3.Click += (s, e) => Measure(Test111, Tb3);
            Button4.Click += (s, e) => Measure(Test2, Tb4);
            Button5.Click += (s, e) => Measure(Test21, Tb5);
            Button6.Click += (s, e) => Measure(Test3, Tb6);
            Button7.Click += (s, e) => Measure(Test4, Tb7);
            Button8.Click += (s, e) => Measure(Test41, Tb8);
            Button9.Click += (s, e) => Measure(Test5, Tb9);

            System.Collections.IEnumerator neko = MyAry.GetEnumerator();


        }

        //普通
        private void Test1()
        {
            var ll = new long[MyAry.Length];
            MyAry.CopyTo(ll, 0);
            for (int i = 0; i < LastIndex; i += LongVectorCount)
            {
                MyVector = new Vector<long>(ll, i);
            }
        }
        private void Test11()
        {
            for (int j = 0; j < LastIndex; j += LongVectorCount)
            {
                for (int i = 0; i < LongVectorCount; i++)
                {
                    MyLongAry[i] = MyAry[j + i];
                }
                MyVector = new Vector<long>(MyLongAry);
            }
        }
        //最速
        private void Test111()
        {
            for (int j = 0; j < LastIndex; j += LongVectorCount)
            {
                MyLongAry[0] = MyAry[j];
                MyLongAry[1] = MyAry[j + 1];
                MyLongAry[2] = MyAry[j + 2];
                MyLongAry[3] = MyAry[j + 3];

                MyVector = new Vector<long>(MyLongAry);
            }
        }

        //ArraySegment
        private void Test2()
        {            
            for (int i = 0; i < LastIndex; i += LongVectorCount)
            {                
                new ArraySegment<int>(MyAry, i, LongVectorCount).ToArray().CopyTo(MyLongAry, 0);
                MyVector = new Vector<long>(MyLongAry);
            }
        }
        private void Test21()
        {
            var aSeg = new ArraySegment<int>(MyAry);
            for (int i = 0; i < LastIndex; i += LongVectorCount)
            {
                aSeg.Slice(i, LongVectorCount).ToArray().CopyTo(MyLongAry, 0);
                MyVector = new Vector<long>(MyLongAry);
            }
        }

        //Linq、最遅
        private void Test3()
        {
            for (int i = 0; i < LastIndex; i += LongVectorCount)
            {
                MyAry.Skip(i).Take(LongVectorCount).ToArray().CopyTo(MyLongAry, 0);
                MyVector = new Vector<long>(MyLongAry);
            }
        }

        //Span
        private void Test4()
        {
            for (int i = 0; i < LastIndex; i += LongVectorCount)
            {
                new Span<int>(MyAry, i, LongVectorCount).ToArray().CopyTo(MyLongAry, 0);
                MyVector = new Vector<long>(MyLongAry);
            }
        }
        private void Test41()
        {
            var sp = new Span<int>(MyAry);
            for (int i = 0; i < LastIndex; i += LongVectorCount)
            {
                sp.Slice(i, LongVectorCount).ToArray().CopyTo(MyLongAry, 0);
                MyVector = new Vector<long>(MyLongAry);
            }
        }

        //Buffer.BlockCopy
        private void Test5()
        {
            int typeSize = System.Runtime.InteropServices.Marshal.SizeOf(MyAry.GetType().GetElementType());
            var ia = new int[LongVectorCount];
            for (int i = 0; i < LastIndex; i+=LongVectorCount)
            {
                Buffer.BlockCopy(MyAry, i * typeSize, ia, 0, LongVectorCount * typeSize);
                ia.CopyTo(MyLongAry, 0);
                MyVector = new Vector<long>(MyLongAry);
            }
        }

        //処理時間測定
        private void Measure(Action action, TextBlock textBlock)
        {
            MyVector = new Vector<long>();
            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 10; i++)
            {
                action();
            }

            sw.Stop();
            string str = "";
            for (int i = 0; i < LongVectorCount; i++)
            {
                str += "  " + MyVector[i].ToString("N0");
            }
            textBlock.Text = $"{sw.Elapsed.TotalSeconds.ToString("00.000")}秒" +
                $"  { System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(action).Name}  {str}";
            
        }
    }
}
