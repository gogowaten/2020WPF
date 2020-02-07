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
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading;
//c# — C＃で整数の配列を合計する方法
//https://www.it-swarm.dev/ja/c%23/c%EF%BC%83%E3%81%A7%E6%95%B4%E6%95%B0%E3%81%AE%E9%85%8D%E5%88%97%E3%82%92%E5%90%88%E8%A8%88%E3%81%99%E3%82%8B%E6%96%B9%E6%B3%95/968057375/


namespace _20200207_int配列の値の合計マルチスレッド
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
        private const int ELEMENT_COUNT = 1_000_001;

        public MainWindow()
        {
            InitializeComponent();

            MyTextBlock.Text = $"配列の値の合計、要素数{ELEMENT_COUNT.ToString("N0")}の合計を{LOOP_COUNT}回求める処理時間";
            MyTextBlockVectorCount.Text = $"Vector<long>.Count = {Vector<long>.Count}";
            MyIntAry = Enumerable.Range(1, ELEMENT_COUNT).ToArray();
            //MyIntAry = Enumerable.Range(int.MaxValue - 10, 2).ToArray();
            MyLongAry = new long[MyIntAry.Length];
            MyIntAry.CopyTo(MyLongAry, 0);


            Button1.Click += (s, e) => MyExe(Test1For, Tb1);
            Button2.Click += (s, e) => MyExe(Test2ParallelFor, Tb2);
            Button3.Click += (s, e) => MyExe(Test3ParallelForOverflow, Tb3);
            Button4.Click += (s, e) => MyExe(Test4ParallelFor, Tb4);
            Button5.Click += (s, e) => MyExe(Test5ParallelFor, Tb5);
            Button6.Click += (s, e) => MyExe(Test6ParallelForWithOptions, Tb6);
            Button7.Click += (s, e) => MyExe(Test7ParallelForEach, Tb7);
            Button8.Click += (s, e) => MyExe(Test8Task, Tb8);
            Button9.Click += (s, e) => MyExe(Test9ParallelForEachPartitioner, Tb9);
            Button10.Click += (s, e) => MyExe(Test10, Tb10);
            Button11.Click += (s, e) => MyExe(Test91ParallelForEachPartitioner, Tb11);
            Button12.Click += (s, e) => MyExe(Test11TaskVectorAdd, Tb12);
            Button13.Click += (s, e) => MyExe(Test12TaskVectorAdd, Tb13);
            Button14.Click += (s, e) => MyExe(Test13TaskVectorAdd, Tb14);
            Button15.Click += (s, e) => MyExe(Test92ParallelForEachPartitioner, Tb15);
            Button16.Click += (s, e) => MyExe(Test93ParallelForEachPartitioner, Tb16);
            Button17.Click += (s, e) => MyExe(Test94ParallelForEachPartitioner, Tb17);
            Button18.Click += (s, e) => MyExe(Test95ParallelForEachPartitioner, Tb18);
            Button19.Click += (s, e) => MyExe(Test99ParallelForEachPartitioner, Tb19);
            Button20.Click += (s, e) => MyExe(Test999ParallelForEachPartitioner, Tb20);
            Button21.Click += (s, e) => MyExe(Test10For, Tb21);

        }

        //for
        private long Test1For(int[] Ary)
        {
            long total = 0;
            for (int i = 0; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        private long Test10For(int[] Ary)
        {
            long total = 0;
            Parallel.For(0, Ary.Length, n =>
            {
                //total += Ary[n];//不正確
                Interlocked.Add(ref total, Ary[n]);//遅い
            });
            return total;
        }


        //        方法: スレッド ローカル変数を使用する Parallel.For ループを記述する | Microsoft Docs
        //https://docs.microsoft.com/ja-jp/dotnet/standard/parallel-programming/how-to-write-a-parallel-for-loop-with-thread-local-variables
        private long Test2ParallelFor(int[] ary)
        {
            long total = 0;
            Parallel.For<long>(0, ary.Length,
                () => 0,
                (j, loop, subtotal) => { return subtotal += ary[j]; },
                (x) => Interlocked.Add(ref total, x));
            return total;
        }
        private long Test3ParallelForOverflow(int[] ary)
        {
            long total = 0;
            Parallel.For(0, ary.Length,
                () => 0,
                (j, loop, subtotal) => { return subtotal += ary[j]; },
                (x) => Interlocked.Add(ref total, x));
            return total;
        }
        private long Test4ParallelFor(int[] ary)
        {
            long total = 0;
            Parallel.For(0, ary.Length,
                () => 0,
                (int j, ParallelLoopState loop, long subtotal) => { return subtotal += ary[j]; },
                (x) => Interlocked.Add(ref total, x));
            return total;
        }

        private long Test5ParallelFor(int[] ary)
        {
            long total = 0;
            Parallel.For(0, ary.Length,
                () => 0,
                (j, loop, subtotal) => { return subtotal += ary[j]; },
                (long x) => Interlocked.Add(ref total, x));
            return total;
        }
        private long Test6ParallelForWithOptions(int[] ary)
        {
            long total = 0;
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount;//8

            Parallel.For<long>(0, ary.Length,
                options,
                () => 0,
                (j, loop, subtotal) => { return subtotal += ary[j]; },
                (x) => Interlocked.Add(ref total, x));
            return total;
        }
        private long Test7ParallelForEach(int[] ary)
        {
            long total = 0;
            Parallel.ForEach(ary, () => 0,
                (item, state, subtotal) => { return subtotal += item; },
                (long x) => Interlocked.Add(ref total, x));
            return total;
        }

        //        方法: パーティション ローカル変数を使用する Parallel.ForEach ループを記述する | Microsoft Docs
        //https://docs.microsoft.com/ja-jp/dotnet/standard/parallel-programming/how-to-write-a-parallel-foreach-loop-with-partition-local-variables

        private long Test7ParallelForEach2(int[] ary)
        {
            long total = 0;
            Parallel.ForEach<int, long>(ary,
                () => 0,
                (item, state, subtotal) => { return subtotal += item; },
                (x) => Interlocked.Add(ref total, x));
            return total;
        }

        private long Test8Task(int[] ary)
        {
            long total = 0;
            int cpuThread = Environment.ProcessorCount;
            int windowSize = ary.Length / cpuThread;

            long[] neko = Task.WhenAll(Enumerable.Range(0, cpuThread).Select(x =>
                Task.Run(() =>
                {
                    var ii = ary.Skip(windowSize * x).Take(windowSize).ToArray();
                    return Test1For(ii);
                }))).GetAwaiter().GetResult();
            total = neko.Sum();
            int amari = ary.Length % windowSize;
            for (int i = 0; i < amari; i++)
            {
                total += ary[i];
            }
            return total;
        }

        //        方法: 小さいループ本体を高速化する | Microsoft Docs
        //https://docs.microsoft.com/ja-jp/dotnet/standard/parallel-programming/how-to-speed-up-small-loop-bodies
        //incorrect
        private long Test9ParallelForEachPartitioner(int[] ary)
        {
            long total = 0;
            var rangePartitioner = Partitioner.Create(0, ary.Length);
            //var options = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
            //over load no6
            Parallel.ForEach(rangePartitioner, (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    Interlocked.Add(ref total, ary[i]);//これなら正確だけど3倍くらい遅い
                    //total += ary[i];//不正確
                }
            });
            return total;
        }

        private long Test91ParallelForEachPartitioner(int[] ary)
        {
            long total = 0;
            int windowSize = ary.Length / Environment.ProcessorCount;
            var rangePartitioner = Partitioner.Create(0, ary.Length, windowSize);//

            //over load no6
            Parallel.ForEach(rangePartitioner, (range, loopState) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    Interlocked.Add(ref total, ary[i]);//これなら正確だけど3倍くらい遅い
                    //total += ary[i];//不正確
                }
            });
            return total;
        }

        //        c# — C＃で整数の配列を合計する方法
        //https://www.it-swarm.dev/ja/c%23/c%EF%BC%83%E3%81%A7%E6%95%B4%E6%95%B0%E3%81%AE%E9%85%8D%E5%88%97%E3%82%92%E5%90%88%E8%A8%88%E3%81%99%E3%82%8B%E6%96%B9%E6%B3%95/968057375/
        private long Test92ParallelForEachPartitioner(int[] ary)
        {
            long total = 0;
            int windowSize = ary.Length / Environment.ProcessorCount;
            var rangePartitioner = Partitioner.Create(0, ary.Length, windowSize);//

            //over load no6
            Parallel.ForEach(rangePartitioner, (range) =>
            {
                long subtotal = 0;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    subtotal += ary[i];
                }
                Interlocked.Add(ref total, subtotal);
            });
            return total;
        }
        private long Test93ParallelForEachPartitioner(int[] ary)
        {
            long total = 0;
            int windowSize = ary.Length / Environment.ProcessorCount;
            var rangePartitioner = Partitioner.Create(0, ary.Length, windowSize);//
            var options = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };

            //over load no6
            Parallel.ForEach(rangePartitioner, options, (range) =>
             {
                 long subtotal = 0;
                 for (int i = range.Item1; i < range.Item2; i++)
                 {
                     subtotal += ary[i];
                 }
                 Interlocked.Add(ref total, subtotal);
             });
            return total;
        }

        private long Test94ParallelForEachPartitioner(int[] ary)
        {
            long total = 0;
            var rangePartitioner = Partitioner.Create(0, ary.Length);//            

            //over load no6
            Parallel.ForEach(rangePartitioner, (range) =>
            {
                long subtotal = 0;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    subtotal += ary[i];
                }
                Interlocked.Add(ref total, subtotal);
            });
            return total;
        }
        private long Test95ParallelForEachPartitioner(int[] ary)
        {
            long total = 0;
            int windowSize = ary.Length / Environment.ProcessorCount;
            var rangePartitioner = Partitioner.Create(0, ary.Length);//
            var options = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };

            //over load no6
            Parallel.ForEach(rangePartitioner, options, (range) =>
             {
                 long subtotal = 0;
                 for (int i = range.Item1; i < range.Item2; i++)
                 {
                     subtotal += ary[i];
                 }
                 Interlocked.Add(ref total, subtotal);
             });
            return total;
        }

        private long Test99ParallelForEachPartitioner(int[] ary)
        {
            long total = 0;
            int windowSize = ary.Length / Environment.ProcessorCount;
            var rangePartitioner = Partitioner.Create(0, ary.Length, windowSize);//
            int simdLength = Vector<long>.Count;
            //over load no6
            Parallel.ForEach(rangePartitioner, (range) =>
            {
                var v = new Vector<long>();
                var l = new long[simdLength];
                int lastIndex = range.Item2 - (range.Item2 % simdLength);
                for (int i = range.Item1; i < lastIndex; i += simdLength)
                {
                    for (int j = 0; j < simdLength; j++)
                    {
                        l[j] = ary[i + j];
                    }
                    v = System.Numerics.Vector.Add(v, new Vector<long>(l));
                }
                long subtotal = 0;
                for (int i = 0; i < simdLength; i++)
                {
                    subtotal += v[i];
                }
                for (int i = lastIndex; i < range.Item2; i++)
                {
                    subtotal += ary[i];
                }
                Interlocked.Add(ref total, subtotal);
            });
            return total;
        }

        private long Test999ParallelForEachPartitioner(long[] ary)
        {
            long total = 0;
            int windowSize = ary.Length / Environment.ProcessorCount;
            var rangePartitioner = Partitioner.Create(0, ary.Length, windowSize);//
            int simdLength = Vector<long>.Count;
            //var options = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
            //over load no6
            //int ii = 0;
            Parallel.ForEach(rangePartitioner, (range) =>
            {
                var v = new Vector<long>();
                int lastIndex = range.Item2 - (range.Item2 % simdLength);
                for (int i = range.Item1; i < lastIndex; i += simdLength)
                {
                    v = System.Numerics.Vector.Add(v, new Vector<long>(ary, i));
                }
                long subtotal = 0;
                for (int i = 0; i < simdLength; i++)
                {
                    subtotal += v[i];
                }
                for (int i = lastIndex; i < range.Item2; i++)
                {
                    subtotal += ary[i];
                }
                Interlocked.Add(ref total, subtotal);
                //Interlocked.Increment(ref ii);
            });
            return total;
        }

     
        private long Test10(int[] ary)
        {
            return ary.AsParallel().Sum(i => (long)i);
        }

        //int型配列からlong型配列への変換は、計算する分だけをその都度変換
        //一括変換よりメモリ使用量が小さくて済む
        private long TestVectorAddEach(int[] Ary)
        {
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            var longAry = new long[simdLength];
            for (int i = 0; i < simdLength; i++)
            {
                longAry[i] = Ary[i];
            }
            var v = new Vector<long>(longAry);

            for (int j = simdLength; j < lastIndex; j += simdLength)
            {
                for (int i = 0; i < simdLength; i++)
                {
                    longAry[i] = Ary[j + i];
                }
                v = System.Numerics.Vector.Add(v, new Vector<long>(longAry));
            }

            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }
        private long Test11TaskVectorAdd(int[] ary)
        {
            long total = 0;
            int cpuThread = Environment.ProcessorCount;
            int windowSize = ary.Length / cpuThread;

            long[] neko = Task.WhenAll(Enumerable.Range(0, cpuThread).Select(x =>
                Task.Run(() =>
                {
                    var ii = ary.Skip(windowSize * x).Take(windowSize).ToArray();
                    return TestVectorAddEach(ii);
                }))).GetAwaiter().GetResult();
            total = neko.Sum();
            return total;
        }

        private long Test12TaskVectorAdd(int[] ary)
        {
            long total = 0;
            int cpuThread = Environment.ProcessorCount;
            int windowSize = ary.Length / cpuThread;

            long[] neko = Task.WhenAll(Enumerable.Range(0, cpuThread).Select(x =>
                Task.Run(() =>
                {
                    var ii = new int[windowSize];
                    for (int i = 0; i < windowSize; i++)
                    {
                        ii[i] = ary[i + (x * windowSize)];
                    }
                    return TestVectorAddEach(ii);
                }))).GetAwaiter().GetResult();
            total = neko.Sum();
            return total;
        }

        private long Test13TaskVectorAdd(int[] ary)
        {
            long total = 0;
            int cpuThread = Environment.ProcessorCount;
            int windowSize = ary.Length / cpuThread;

            long[] neko = Task.WhenAll(Enumerable.Range(0, cpuThread).Select(x =>
                Task.Run(() =>
                {
                    var ll = new long[windowSize];
                    var p = x * windowSize;
                    for (int i = 0; i < windowSize; i++)
                    {
                        ll[i] = ary[i + p];
                    }
                    return TestLongVectorAdd(ll);
                }))).GetAwaiter().GetResult();
            total = neko.Sum();
            return total;
        }
        private long TestLongVectorAdd(long[] ary)
        {
            int simdLength = Vector<long>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);

            var v = new Vector<long>();
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                v = System.Numerics.Vector.Add(v, new Vector<long>(ary, i));
            }
            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < ary.Length; i++)
            {
                total += ary[i];
            }
            return total;
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
