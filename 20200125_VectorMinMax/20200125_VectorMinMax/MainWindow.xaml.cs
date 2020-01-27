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

//ブログ記事
//C#、配列から最大値を求めるMaxは、MathクラスよりSystem.Numerics.Vectorクラスのほうが速い - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2020/01/27/213744

namespace _20200125_VectorMinMax
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int[] MyArray;
        const int MY要素数 = 10_000_000;
        private int Myループ回数;
        public MainWindow()
        {
            InitializeComponent();



            //int simdLength = Vector<int>.Count;//8個 (Ryzen 5 2500Gの場合)


            //var va = new Vector<int>(123);


            ////var vb = new Vector<int>(new int[] { 1, 2, 2 });//要素数が合わない(8個未満)のでエラー
            //var vb2 = new Vector<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });//8個目以降は無視される

            //var vc = new Vector<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 2);


            //var vd = new Vector<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            //var ve = new Vector<int>(new int[] { 1, 1, 1, 1, 100, 1, 1, 1 });
            //var vf = System.Numerics.Vector.Max(vd, ve);
            //int[] ary = new int[simdLength];
            //for (int i = 0; i < simdLength; i++)
            //{
            //    ary[i] = vf[i];
            //}

            //int max = vf[0];
            //for (int i = 1; i < simdLength; i++)
            //{
            //    if (max < vf[i]) max = vf[i];
            //}









            SetLoopCount(100);
            MyArray = new int[MY要素数];
            SetMyArray(int.MinValue, int.MaxValue);
            //MyArray[MY要素数 - 1] = int.MaxValue;//テスト用、最後の要素を最大値にする

            ButtonRandomArray.Click += (s, e) => { SetMyArray(int.MinValue, int.MaxValue); };
            ButtonSetLoopCount1.Click += (s, e) => { SetLoopCount(1); };
            ButtonSetLoopCount10.Click += (s, e) => { SetLoopCount(10); };
            ButtonSetLoopCount100.Click += (s, e) => SetLoopCount(100);
            ButtonSetLoopCount1000.Click += (s, e) => SetLoopCount(1000);

            ButtonForIf.Click += (s, e) => { MyTime(ForIf, tbForIf); };
            ButtonMathMax.Click += (s, e) => { MyTime(MathMax, tbMathMax); };
            ButtonLinqMax.Click += (s, e) => { MyTime(LinqMax, tbLinqMax); };
            ButtonVectorMax.Click += (s, e) => { MyTime(VectorMax, tbVectorMax); };

            ButtonForIfMT.Click += (s, e) => { MyTime(ForIfMT, tbForIfMT); };
            ButtonMathMaxMT.Click += (s, e) => { MyTime(MathMaxMT, tbMathMaxMT); };
            ButtonLinqAsParallelMax.Click += (s, e) => { MyTime(LinqMaxAsParallel, tbLinqAsParallelMax); };
            ButtonVectorMaxMT.Click += (s, e) => { MyTime(VectorMaxMT, tbVectorMaxMT); };

            ButtonForIfMT2.Click += (s, e) => { MyTimeParallel(ForIf, tbForIfMT2); };
            ButtonMathMaxMT2.Click += (s, e) => { MyTimeParallel(MathMax, tbMathMaxMT2); };
            ButtonLinqAsParallelMax2.Click += (s, e) => { MyTimeParallel(LinqMax, tbLinqAsParallelMax2); };
            ButtonVectorMaxMT2.Click += (s, e) => { MyTimeParallel(VectorMax, tbVectorMaxMT2); };

            ButtonVectorMinMaxMT2.Click += (s, e) => MyTimeParallelMinMax(VectorMinMax, tbVectorMinMaxMT2);

            //ButtonTest.Click += (s, e) => { MyTime(ForIfMT2, tbTest); };
            //ButtonTest.Click += (s, e) => { MyTime(ForIfParallelFor, tbTest); };
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

        private void SetLoopCount(int loopCount)
        {
            Myループ回数 = loopCount;
            this.Title = $"int型配列(要素数10,000,000)から最大値を、{Myループ回数}回取得するまでの処理時間";
            //全TextBlockを取得してTextをなしにする
            //var neko = EnumerateDescendantObjects2<TextBlock>(this).Select(a =>  a.Text = null);
            foreach (var item in EnumerateDescendantObjects2<TextBlock>(this))
            {
                item.Text = "";
            }
        }

        #region コントロールの列挙
        private static IEnumerable<T> EnumerateDescendantObjects<T>(DependencyObject obj) where T : DependencyObject
        {
            foreach (var child in LogicalTreeHelper.GetChildren(obj))
            {
                if (child is T cobj)
                {
                    yield return cobj;
                }
                if (child is DependencyObject dobj)
                {
                    foreach (var cobj2 in EnumerateDescendantObjects<T>(dobj))
                    {
                        yield return cobj2;
                    }
                }
            }
        }
        private static List<T> EnumerateDescendantObjects2<T>(DependencyObject obj) where T : DependencyObject
        {
            var l = new List<T>();
            foreach (object child in LogicalTreeHelper.GetChildren(obj))
            {
                if (child is T)
                {
                    l.Add((T)child);
                }
                if (child is DependencyObject dobj)
                {
                    foreach (T cobj2 in EnumerateDescendantObjects2<T>(dobj))
                    {
                        l.Add(cobj2);
                    }
                }
            }
            return l;
        }
        #endregion

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

        #region 未使用
        private int ForIfMT最後の要素まできっちり処理(int[] intAry)
        {
            int cpuThread = Environment.ProcessorCount;
            int windowSize = intAry.Length / cpuThread;
            int[] mm = Task.WhenAll(Enumerable.Range(0, cpuThread).Select(n => Task.Run(() =>
            {
                var a = intAry.Skip(windowSize * n).Take(windowSize);
                return ForIf(a.ToArray());
            }))).GetAwaiter().GetResult();

            //スレッド数で割り切れなかった余りの要素との比較1
            int last = windowSize * cpuThread;
            int[] nokori = intAry.Skip(last).ToArray();
            return nokori.Concat(mm).Max();

            //処理速度は↑と変わらない
            //スレッド数で割り切れなかった余りの要素との比較2
            //int max = int.MinValue;
            //for (int i = windowSize * cpuThread; i < intAry.Length; i++)
            //{
            //    if (max < intAry[i]) max = intAry[i];
            //}
            //for (int i = 0; i < cpuThread; i++)
            //{
            //    if (max < mm[i]) max = mm[i];
            //}
            //return max;
        }

        //Parallel
        private int ForIfParallelFor(int[] intAry)
        {
            int cpuThread = Environment.ProcessorCount;
            int windowSize = intAry.Length / cpuThread;

            //配列を分割、CPUスレッド数分の配列を作る
            int[][] temp = new int[cpuThread][];
            for (int i = 0; i < cpuThread; i++)
            {
                temp[i] = intAry.Skip(i * windowSize).Take(windowSize).ToArray();
            }

            int[] neko = new int[cpuThread];
            //var op = new ParallelOptions();//あんまり意味ない、2でも4でも8でも誤差
            //op.MaxDegreeOfParallelism = 8;//2(5.18) 4(4.98) 8(4.88)
            //Parallel.For(0, cpuThread, op, i => { neko[i] = ForIf(temp[i]); });
            Parallel.For(0, cpuThread, i => { neko[i] = ForIf(temp[i]); });
            return neko.Max();
        }
        #endregion

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

        //最小値と最大値を返す
        private (int min, int max) VectorMinMax(int[] intAry)
        {
            int simdLength = Vector<int>.Count;
            int myLast = intAry.Length - simdLength;
            var vMin = new Vector<int>(int.MaxValue);
            var vMax = new Vector<int>(int.MinValue);
            for (int i = 0; i < myLast; i += simdLength)
            {
                var v = new Vector<int>(intAry, i);
                vMin = System.Numerics.Vector.Min(vMin, v);
                vMax = System.Numerics.Vector.Max(vMax, v);
            }
            int max = int.MinValue;
            int min = int.MaxValue;
            for (int i = 0; i < simdLength; i++)
            {
                if (max < vMax[i]) max = vMax[i];
                if (min > vMin[i]) min = vMin[i];
            }
            for (int i = myLast; i < intAry.Length; i++)
            {
                if (max < intAry[i]) max = intAry[i];
                if (min > intAry[i]) min = intAry[i];
            }
            return (min, max);
        }

        #region 時間計測
        private void MyTime(Func<int[], int> func, TextBlock textBlock)
        {
            var sw = new Stopwatch();
            int max = int.MinValue;
            sw.Start();
            for (int i = 0; i < Myループ回数; i++)
            {
                max = func(MyArray);
            }
            sw.Stop();
            textBlock.Text =
                $"{System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}" +
                $"\n最大値 = {max}\n処理時間：{sw.Elapsed.TotalSeconds.ToString("00.00")}秒";
        }
        private void MyTimeParallel(Func<int[], int> func, TextBlock textBlock)
        {
            var sw = new Stopwatch();
            int max = int.MinValue;
            int[] mm = new int[Myループ回数];
            var option = new ParallelOptions();
            option.MaxDegreeOfParallelism = Environment.ProcessorCount;
            sw.Start();
            Parallel.For(0, Myループ回数, option, n =>
             {
                 mm[n] = func(MyArray);
             });
            max = mm.Max();
            sw.Stop();
            textBlock.Text =
                $"{System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}" +
                $"\n最大値 = {max}\n処理時間：{sw.Elapsed.TotalSeconds.ToString("00.00")}秒";
        }

        private void MyTimeParallelMinMax(Func<int[], (int min, int max)> func, TextBlock textBlock)
        {
            var sw = new Stopwatch();
            int min = int.MaxValue;
            int max = int.MinValue;
            int[] mins = new int[Myループ回数];
            int[] maxs = new int[Myループ回数];
            var option = new ParallelOptions();
            option.MaxDegreeOfParallelism = Environment.ProcessorCount;
            sw.Start();
            Parallel.For(0, Myループ回数, option, n =>
            {
                (mins[n], maxs[n]) = func(MyArray);
            });
            min = mins.Min();
            max = maxs.Max();
            sw.Stop();
            textBlock.Text =
                $"{System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}" +
                $"\n最小値 = {min}\n最大値 = {max}\n処理時間：{sw.Elapsed.TotalSeconds.ToString("00.00")}秒";
        }

        #endregion

    }
}
//配列の中から指定した範囲の要素を抜き出す - .NET Tips(VB.NET, C#...)
//https://dobon.net/vb/dotnet/programing/arrayslice.html
//SIMDのVector処理を試してみる: 第十四工房
//http://c5d5e5.asablo.jp/blog/2017/09/27/8685145
//Vector, System.Numerics C# (CSharp) Code Examples - HotExamples
//https://csharp.hotexamples.com/examples/System.Numerics/Vector/-/php-vector-class-examples.html
//[WPF]DependencyObject の子孫要素を型指定ですべて列挙する
//https://mseeeen.msen.jp/wpf-enumerate-descendant-objects-of-dependency-object/
