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
using System.Diagnostics;
using System.Numerics;


namespace _20200125_VectorMinMax
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int[] MyArray;
        const int RMIN = 0;
        const int RMAX = int.MaxValue;
        const int MY要素数 = 100000;//100000
        const int MYループ回数 = 10000;//10000
        public MainWindow()
        {
            InitializeComponent();

            this.Title = $"int型配列、要素数：{MY要素数}から最大値を取得、ループ回数：{MYループ回数}の処理時間";
            MyArray = new int[MY要素数];
            SetMyArray(RMIN, RMAX);

            ButtonRandomArray.Click += (o, e) => { SetMyArray(RMIN, RMAX); };

            ButtonForIf.Click += (o, e) => { MyTime(ForIf, nameof(ForIf), tbForIf); };
            ButtonMathMax.Click += (o, e) => { MyTime(MathMax, nameof(MathMax), tbMathMax); };
            ButtonLinqMax.Click += (o, e) => { MyTime(LinqMax, nameof(LinqMax), tbLinqMax); };
            ButtonVectorMax.Click += (o, e) => { MyTime(VectorMax, nameof(VectorMax), tbVectorMax); };


            ButtonForIfMT.Click += (o, e) => { MyTime(ForIfMT, nameof(ForIfMT), tbForIfMT); };
            ButtonMathMaxMT.Click += (o, e) => { MyTime(MathMaxMT, nameof(MathMax), tbMathMaxMT); };
            ButtonLinqAsParallelMax.Click += (o, e) => { MyTime(LinqMaxAsParallel, nameof(LinqMaxAsParallel), tbLinqAsParallelMax); };
            ButtonVectorMaxMT.Click += (o, e) => { MyTime(VectorMaxMT, nameof(VectorMaxMT), tbVectorMaxMT); };

            ButtonTest.Click += (o, e) => { MyTime(ForIfMT2, nameof(ForIfMT2), tbTest); };

        }

        private void SetMyArray(int min, int max)
        {
            var r = new Random();
            for (int i = 0; i < MyArray.Length; i++)
            {
                MyArray[i] = r.Next(min, max);
            }
            r.Next(min, max);
        }


        private int ForIf(int[] intAry)
        {
            int max = intAry[0];
            for (int i = 1; i < intAry.Length; i++)
            {
                if (max < intAry[i]) max = intAry[i];
            }
            return max;
        }
        private int ForIfMT(int[] intAry)
        {
            int cpuThread = Environment.ProcessorCount;
            int windowSize = intAry.Length / cpuThread;
            int[] mm = Task.WhenAll(Enumerable.Range(0, cpuThread).Select(n => Task.Run(() =>
             {
                 var a = intAry.Skip(windowSize * n).Take(windowSize);
                 return ForIf(a.ToArray());
             }))).GetAwaiter().GetResult();

            return mm.Max();
        }
        private int ForIfMT2(int[] intAry)
        {
            int cpuThread = Environment.ProcessorCount;
            int windowSize = intAry.Length / cpuThread;
            Task<int[]> mm = Task.WhenAll(Enumerable.Range(0, cpuThread).Select(n => Task.Run(() =>
            {
                var a = intAry.Skip(windowSize * n).Take(windowSize);
                return ForIf(a.ToArray());
            })));
            int[] neko = mm.Result;
            
            return neko.Max();
        }

        private int MathMax(int[] intAry)
        {
            int max = intAry[0];
            for (int i = 1; i < intAry.Length; i++)
            {
                max = Math.Max(max, intAry[i]);
            }
            return max;
        }
        private int MathMaxMT(int[] intAry)
        {
            int cpuThread = Environment.ProcessorCount;
            int windowSize = intAry.Length / cpuThread;
            int[] mm = Task.WhenAll(Enumerable.Range(0, cpuThread).Select(n => Task.Run(() =>
            {
                var a = intAry.Skip(windowSize * n).Take(windowSize);
                return MathMax(a.ToArray());
            }))).GetAwaiter().GetResult();

            return mm.Max();
        }

        private int LinqMax(int[] intAry)
        {
            return intAry.Max();
        }
        private int LinqMaxAsParallel(int[] intAray)
        {
            return intAray.AsParallel().Max();
        }
        private int VectorMax(int[] intAry)
        {
            int simdLength = Vector<int>.Count;
            int myLast = intAry.Length - simdLength;
            var vMax = new Vector<int>(int.MinValue);
            for (int i = 0; i < myLast; i += simdLength)
            {
                vMax = System.Numerics.Vector.Max(vMax, new Vector<int>(intAry, i));
            }
            int max = int.MinValue;
            for (int i = 0; i < simdLength; i++)
            {
                if (max < vMax[i]) max = vMax[i];
            }
            for (int i = myLast; i < intAry.Length; i++)
            {
                if (max < intAry[i]) max = intAry[i];
            }
            return max;
        }

        private int VectorMaxMT(int[] intAry)
        {
            int cpuThread = Environment.ProcessorCount;
            int windowSize = intAry.Length / cpuThread;

            int[] mm = Task.WhenAll(Enumerable.Range(0, cpuThread).Select(n => Task.Run(() =>
             {
                 //int[] a = new int[windowSize];
                 //Array.Copy(intAry, n * windowSize, a, 0, windowSize);
                 int[] a = new int[windowSize];
                 int typeSize = System.Runtime.InteropServices.Marshal.SizeOf(
                     intAry.GetType().GetElementType());
                 Buffer.BlockCopy(intAry, n * windowSize * typeSize, a, 0, windowSize * typeSize);
                 return VectorMax(a);
             }))).GetAwaiter().GetResult();

            return mm.Max();
        }


        private void MyTime(Func<int[], int> func, string funcName, TextBlock textBlock)
        {
            var sw = new Stopwatch();
            sw.Start();
            int max = 0;
            for (int i = 0; i < MYループ回数; i++)
            {
                max = func(MyArray);
            }
            sw.Stop();
            textBlock.Text = $"{funcName}\n最大値 = {max}\n処理時間：{sw.Elapsed.TotalSeconds.ToString("00.00")}秒";
        }
    }
}
//配列の中から指定した範囲の要素を抜き出す - .NET Tips(VB.NET, C#...)
//https://dobon.net/vb/dotnet/programing/arrayslice.html
//SIMDのVector処理を試してみる: 第十四工房
//http://c5d5e5.asablo.jp/blog/2017/09/27/8685145
//Vector, System.Numerics C# (CSharp) Code Examples - HotExamples
//https://csharp.hotexamples.com/examples/System.Numerics/Vector/-/php-vector-class-examples.html
