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
        public MainWindow()
        {
            InitializeComponent();

            Test31();
            Test4();
            Test();
            Test2();
            Test3();
            butto1_Click();
            button2_Click();

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
