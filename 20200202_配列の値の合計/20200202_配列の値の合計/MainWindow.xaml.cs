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
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Numerics;

namespace _20200202_配列の値の合計
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BlockingCollection<double> FBlock;
        private int[] MyIntAry;
        private long[] MyLongAry;
        private const int LOOP_COUNT = 100;
        private const int ELEMENT_COUNT = 1_000_000;



        public MainWindow()
        {
            InitializeComponent();

            MyTextBlock.Text = $"配列の値の合計、要素数{ELEMENT_COUNT.ToString("N0")}の合計を{LOOP_COUNT}回求める処理時間";
            MyTextBlockVectorCount.Text = $"Vector<long>.Count = {Vector<long>.Count}";
            MyIntAry = Enumerable.Range(1, ELEMENT_COUNT).ToArray();
            MyLongAry = new long[MyIntAry.Length];
            MyIntAry.CopyTo(MyLongAry, 0);



            Test5();
            Test31();
            Test4();
            Test();
            Test2();
            Test3();
            butto1_Click();
            button2_Click();

        }

        //for
        private long TestFor(int[] Ary)
        {
            long total = 0;
            for (int i = 0; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }






        private void Test5()
        {
            int[] ii = Enumerable.Range(1, 1_000_000).ToArray();
            long total = 0;
            int simdLongLength = Vector<long>.Count;
            int simdIntLength = Vector<int>.Count;

            int[] inu = new ArraySegment<int>(ii, 0, simdIntLength).ToArray();
            var vInu = new Vector<int>(inu);
            var tako = new ArraySegment<int>(ii, 0, simdIntLength);
            var vTako = new Vector<int>(tako);

            //var neko = new ArraySegment<long>(nums, 0, simdLongLength);//エラー型が違う

            var neko = new long[simdLongLength];
            //Array.ConstrainedCopy(ii, 1, neko, 0, 8);//エラー
            for (int i = 0; i < simdLongLength; i++)
            {
                neko[i] = ii[i];
            }
            var uma = new long[ii.Length];
            ii.CopyTo(uma, 0);

            int[] iii = new ArraySegment<int>(ii, 10, simdLongLength).ToArray();
            long[] ll = new long[simdLongLength];
            iii.CopyTo(ll, 0);

            int[] mm = ii.Skip(10).Take(simdLongLength).ToArray();
            long[] nn = new long[simdLongLength];
            mm.CopyTo(nn, 0);

            int[] sp = new Span<int>(ii, 10, simdLongLength).ToArray();
            long[] oo = new long[simdLongLength];
            sp.CopyTo(oo, 0);

            long[] pp = new long[simdLongLength];
            new Span<int>(ii, 10, simdLongLength).ToArray().CopyTo(pp, 0);


        }



        //        方法: スレッド ローカル変数を使用する Parallel.For ループを記述する | Microsoft Docs
        //https://docs.microsoft.com/ja-jp/dotnet/standard/parallel-programming/how-to-write-a-parallel-for-loop-with-thread-local-variables

        private void Test()
        {
            int[] nums = Enumerable.Range(1, 100_000_000).ToArray();
            long total = 0;

            Parallel.For<long>(0, nums.Length, () => 0, (j, loop, subtotal) =>
            {
                subtotal += nums[j];
                return subtotal;
            },
            (x) =>
            {
                Interlocked.Add(ref total, x);
            });

            //var neko = nums.Sum();//エラー
        }
        private void Test2()
        {
            int[] ii = Enumerable.Range(1, 100_000_000).ToArray();
            long AAA排他制御無し合計値 = 0;
            long BBB排他制御有り合計値 = 0;
            Parallel.For<long>(0, ii.Length, () => 0,
                (j, loopState, subtotal) =>
                {
                    return subtotal += ii[j];
                },
                (long x) =>
                {
                    AAA排他制御無し合計値 += x;//排他制御なしなので、たまに間違った値になる
                    Interlocked.Add(ref BBB排他制御有り合計値, x);//排他制御あり
                });
        }
        private void Test3()
        {
            int[] ii = Enumerable.Range(1, 100_000_000).ToArray();
            long total = 0;
            for (int i = 0; i < ii.Length; i++)
            {
                total += ii[i];
            }
        }
        private void Test31()
        {
            int[] ii = Enumerable.Range(1, 100_000_000).ToArray();
            long total = 0;
            foreach (var item in ii)
            {
                total += item;
            }
        }

        private void Test4()
        {
            //            c# — C＃で整数の配列を合計する方法
            //https://www.it-swarm.dev/ja/c%23/c%EF%BC%83%E3%81%A7%E6%95%B4%E6%95%B0%E3%81%AE%E9%85%8D%E5%88%97%E3%82%92%E5%90%88%E8%A8%88%E3%81%99%E3%82%8B%E6%96%B9%E6%B3%95/968057375/

            int[] ii = Enumerable.Range(1, 100_000_000).ToArray();
            long total = 0;
            //var rangePartitioner = System.Collections.Concurrent.Partitioner.Create(0, ii.Length);            
            Parallel.ForEach(Partitioner.Create(0, ii.Length), range =>
            {
                long localSum = 0;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    localSum += ii[i];
                }
                Interlocked.Add(ref total, localSum);
            });
        }

        private void Test0()
        {
            int[] ii = Enumerable.Range(1, 100_000_000).ToArray();// 1_000_000_000).ToArray();
            //int[] ii = Enumerable.Repeat(int.MaxValue, 100_000_000).ToArray();
            //long[] ii = Enumerable.Repeat(long.MaxValue, 1_000_000).ToArray();
            //var r = new Random();
            //int[] ii = new int[100_000_000];
            //for (int i = 0; i < ii.Length; i++)
            //{
            //    ii[i] = r.Next(0, int.MaxValue);
            //}
            long total = 0;
            double dTotal = 0;
            BigInteger bTotal = 0;
            for (int i = 0; i < ii.Length; i++)
            {
                total += ii[i];
                dTotal += ii[i];
                bTotal += ii[i];
            }
            var iave = total / (double)ii.Length;
            var dave = dTotal / ii.Length;
            var bave = bTotal / ii.Length;
            //var neko = ii.Sum();//エラー
        }





        //---------------------------------------------------------------------
        private void butto1_Click()
        {
            FBlock = new BlockingCollection<double>(10000000);
            double grandTotal = 0;

            var stopWatch = Stopwatch.StartNew();

            ParallelLoopResult result = Parallel.For(1, 10000000,
              LocalInit,       // スレッドローカル変数を初期化するデリゲートで、1 回だけ呼び出される
              Body,            // Body デリゲートで、ループごとに呼び出される
              LocalFinally     // Body デリゲート終了後に実行するデリゲートで、1 回だけ呼び出される
            );

            if (result.IsCompleted)
            {
                FBlock.CompleteAdding(); // 必須

                // ループごとの localTotal を合計する
                foreach (var item in FBlock.GetConsumingEnumerable())
                {
                    grandTotal += item;
                }
            }

            stopWatch.Stop();
            MessageBox.Show(String.Format("GrandTotal={0}, Time={1}", grandTotal, stopWatch.ElapsedMilliseconds));
        }
        private double LocalInit()
        {
            return 0.0; // この例では localTotal の初期値 0.0 である
        }
        private double Body(int i, ParallelLoopState state, double localTotal)
        {
            //            return localTotal + Math.Sqrt(i);
            return localTotal + i;
        }
        private void LocalFinally(double localTotal)
        {
            FBlock.Add(localTotal);
        }

        private void button2_Click()
        {
            var block = new BlockingCollection<double>(10000000);
            double grandTotal = 0;

            var stopWatch = Stopwatch.StartNew();

            ParallelLoopResult result = Parallel.For(1, 10000000,
              () => 0.0,                                           // スレッドローカルデータの初期化
              (i, state, localTotal) => localTotal + Math.Sqrt(i), // body デリゲートで、localTotal を返す
              localTotal =>                                        // localTotal を合計する
              {
                  block.Add(localTotal); // 1 ～ 10000000 の値をコレクションに追加する
              }
            );

            if (result.IsCompleted)
            {
                block.CompleteAdding();

                foreach (var item in block.GetConsumingEnumerable())
                {
                    grandTotal += item;
                }
            }

            stopWatch.Stop();
            MessageBox.Show(String.Format("GrandTotal={0}, Time={1}", grandTotal, stopWatch.ElapsedMilliseconds));
        }




        private void MyExe(Func<int[], long> func, TextBlock tb)
        {
            long total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(MyIntAry);
            }
            sw.Stop();
            tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  合計値：{total}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
        }
        private void MyExe(Func<long[], long> func, TextBlock tb)
        {
            long total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(MyLongAry);
            }
            sw.Stop();
            tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  合計値：{total}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
        }
    }
}
//Parallel クラス
//http://www.kanazawa-net.ne.jp/~pmansato/parallel/parallel_parallel.htm
//方法: スレッド ローカル変数を使用する Parallel.For ループを記述する | Microsoft Docs
//https://docs.microsoft.com/ja-jp/dotnet/standard/parallel-programming/how-to-write-a-parallel-for-loop-with-thread-local-variables
//Interlocked クラス(System.Threading) | Microsoft Docs
//https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.interlocked?view=netframework-4.8
//第3回 マルチスレッドでデータの不整合を防ぐための排他制御 ― マルチスレッド・プログラミングにおける排他制御と同期制御（前編） ― (3/3)：連載.NETマルチスレッド・プログラミング入門 - ＠IT
// https://www.atmarkit.co.jp/ait/articles/0505/25/news113_3.html
//マルチスレッドで高速なC#を書くためのロック戦略 - Qiita
//https://qiita.com/tadokoro/items/28b3623a5ec58517d431
//TeraOmegaNetwork 2.1 「Parallel.For」
//http://teraomega.info/CommentSearch.aspx?categorykbn1=2001
