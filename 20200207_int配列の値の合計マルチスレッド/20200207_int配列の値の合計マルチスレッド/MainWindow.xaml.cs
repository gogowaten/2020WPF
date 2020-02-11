using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private int[] MyIntAry;
        private long[] MyLongAry;
        private const int LOOP_COUNT = 1000;
        private const int ELEMENT_COUNT = 1_000_001;

        public MainWindow()
        {
            InitializeComponent();


            MyTextBlock.Text = $"配列の値の合計、要素数{ELEMENT_COUNT.ToString("N0")}の合計を{LOOP_COUNT}回求める処理時間";
            MyTextBlockVectorCount.Text = $"Vector<long>.Count = {Vector<long>.Count}";
            MyIntAry = Enumerable.Range(1, ELEMENT_COUNT).ToArray();//連番値
            //MyIntAry = Enumerable.Repeat(1, ELEMENT_COUNT).ToArray();//全値1
            MyLongAry = new long[MyIntAry.Length];//long型配列作成
            MyIntAry.CopyTo(MyLongAry, 0);


            Button1.Click += (s, e) => MyExe(Test0_For, Tb1);//基準
            Button2.Click += (s, e) => MyExe(Test10_ParallelFor, Tb2);////遅い
            Button3.Click += (s, e) => MyExe(Test20_AsParallelSum, Tb3);//速くはないけど書くのがラク
            Button4.Click += (s, e) => MyExe(Test30_ParallelForEach, Tb4);
            Button5.Click += (s, e) => MyExe(Test40_ParallelForThreadLocalVariable, Tb5);
            Button6.Click += (s, e) => MyExe(Test4x_ParallelForThreadLocalVariable_Overflow, Tb6);//オーバーフロー
            Button7.Click += (s, e) => MyExe(Test41_ParallelForThreadLocalVariable, Tb7);
            Button8.Click += (s, e) => MyExe(Test42_ParallelForThreadLocalVariable, Tb8);
            Button9.Click += (s, e) => MyExe(Test43_ParallelForThreadLocalVariable, Tb9);
            Button10.Click += (s, e) => MyExe(Test50_TaskLinqSkipTake, Tb10);
            Button11.Click += (s, e) => MyExe(Test60_ParallelForEachPartitioner, Tb11);////遅い
            Button12.Click += (s, e) => MyExe(Test70_ParallelForEachPartitionerThreadLocalVariable, Tb12);//速い
            Button13.Click += (s, e) => MyExe(Test71_ParallelForEachPartitionerThreadLocalVariable, Tb13);//速い
            Button14.Click += (s, e) => MyExe(Test72_ParallelForEachPartitionerThreadLocalVariable, Tb14);//速い
            Button15.Click += (s, e) => MyExe(Test73_ParallelForEachPartitionerThreadLocalVariable, Tb15);//速い
            Button16.Click += (s, e) => MyExe(Test80_TaskLinqSkipTake_Vector, Tb16);
            Button17.Click += (s, e) => MyExe(Test81_TaskVectorAdd_ForInt, Tb17);
            Button18.Click += (s, e) => MyExe(Test82_TaskVectorAdd_ForLong, Tb18);
            Button19.Click += (s, e) => MyExe(Test90_ParallelForEachPartitioner_Vector, Tb19);
            Button20.Click += (s, e) => MyExe(Test91_ParallelForEachPartitioner_Vector, Tb20);
            Button21.Click += (s, e) => MyExe(Test9x_ParallelForEachPartitionerVector_Overflow, Tb21);//オーバーフロー
            Button22.Click += (s, e) => MyExe(Test99x_ParallelForEachPartitionerVector4_Incorect, Tb22);//誤差
            Button23.Click += (s, e) => MyExe(Test99_ParallelForEachPartitioner_VectorWiden, Tb23);//最速

        }

        //普通のfor、シングルスレッド
        private long Test0_For(int[] Ary)
        {
            long total = 0;
            for (int i = 0; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }
        //Parallel.For(マルチスレッド
        //かなり遅い、必要ない
        private long Test10_ParallelFor(int[] Ary)
        {
            long total = 0;
            Parallel.For(0, Ary.Length, n =>
            {
                //total += Ary[n];//不正確
                Interlocked.Add(ref total, Ary[n]);//遅い
            });
            return total;
        }

        //LINQのSumをマルチスレッドで
        private long Test20_AsParallelSum(int[] ary)
        {
            return ary.AsParallel().Sum(i => (long)i);
        }


        //        方法: パーティション ローカル変数を使用する Parallel.ForEach ループを記述する | Microsoft Docs
        //https://docs.microsoft.com/ja-jp/dotnet/standard/parallel-programming/how-to-write-a-parallel-foreach-loop-with-partition-local-variables
        //Parallel.ForEach、スレッドローカル変数使用版、最初に型宣言
        private long Test30_ParallelForEach(int[] ary)
        {
            long total = 0;
            Parallel.ForEach<int, long>(
                ary,
                () => 0,
                (item, state, subtotal) => { return subtotal += item; },
                (x) => Interlocked.Add(ref total, x));
            return total;
        }
        //Parallel.ForEach、スレッドローカル変数使用版、変数個別に型宣言
        //必要ない、未使用
        private long Test31_ParallelForEach(int[] ary)
        {
            long total = 0;
            Parallel.ForEach(ary,
                () => 0,
                (item, state, subtotal) => { return subtotal += item; },
                (long x) => Interlocked.Add(ref total, x));
            return total;
        }


        //        方法: スレッド ローカル変数を使用する Parallel.For ループを記述する | Microsoft Docs
        //https://docs.microsoft.com/ja-jp/dotnet/standard/parallel-programming/how-to-write-a-parallel-for-loop-with-thread-local-variables
        //Parallel.For、スレッドローカル変数使用版
        private long Test40_ParallelForThreadLocalVariable(int[] ary)
        {
            long total = 0;
            Parallel.For<long>(0, ary.Length,
                () => 0,
                (j, loop, subtotal) => { return subtotal += ary[j]; },
                (x) => Interlocked.Add(ref total, x));
            return total;
        }
        //Parallel.For、スレッドローカル変数使用版、int型のままなので不正確
        //必要ない
        private long Test4x_ParallelForThreadLocalVariable_Overflow(int[] ary)
        {
            long total = 0;
            Parallel.For(0, ary.Length,
                () => 0,
                (j, loop, subtotal) => { return subtotal += ary[j]; },
                (x) => Interlocked.Add(ref total, x));
            return total;
        }
        //Parallel.For、スレッドローカル変数使用版、変数個別に型宣言
        //必要ない
        private long Test41_ParallelForThreadLocalVariable(int[] ary)
        {
            long total = 0;
            Parallel.For(0, ary.Length,
                () => 0,
                (int j, ParallelLoopState loop, long subtotal) => { return subtotal += ary[j]; },
                (x) => Interlocked.Add(ref total, x));
            return total;
        }
        //Parallel.For、スレッドローカル変数使用版、変数個別に型宣言2
        //必要ない
        private long Test42_ParallelForThreadLocalVariable(int[] ary)
        {
            long total = 0;
            Parallel.For(0, ary.Length,
                () => 0,
                (j, loop, subtotal) => { return subtotal += ary[j]; },
                (long x) => Interlocked.Add(ref total, x));
            return total;
        }
        //Parallel.For、スレッドローカル変数使用版
        //ParallelOptionsでスレッド数指定したけど速度や結果に変化なし
        //必要ない
        private long Test43_ParallelForThreadLocalVariable(int[] ary)
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



        //Taskでマルチスレッド、CPUスレッド数で配列を分割、それぞれをシングルで処理
        private long Test50_TaskLinqSkipTake(int[] ary)
        {
            long total = 0;
            int cpuThread = Environment.ProcessorCount;
            int windowSize = ary.Length / cpuThread;

            long[] neko = Task.WhenAll(Enumerable.Range(0, cpuThread).Select(x =>
                Task.Run(() =>
                {
                    var ii = ary.Skip(windowSize * x).Take(windowSize).ToArray();//割当範囲作成
                    return Test0_For(ii);//シングルで処理
                }))).GetAwaiter().GetResult();
            total = neko.Sum();
            int lastIndex = ary.Length - (ary.Length % windowSize);
            for (int i = lastIndex; i < ary.Length; i++)
            {
                total += ary[i];
            }
            return total;
        }


        //        方法: 小さいループ本体を高速化する | Microsoft Docs
        //https://docs.microsoft.com/ja-jp/dotnet/standard/parallel-programming/how-to-speed-up-small-loop-bodies
        //Parallel.ForEach、Partitionerで配列を区切って各スレッドに割り当てる？
        //区切り位置指定したけど、しなくても速度に変化なかった
        //かなり遅い
        //必要ない
        private long Test60_ParallelForEachPartitioner(int[] ary)
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
        //最速
        //Parallel.ForEachとPartitionerとパーティションローカル変数？
        //Partitionerでの区切り位置指定
        private long Test70_ParallelForEachPartitionerThreadLocalVariable(int[] ary)
        {
            long total = 0;//(合計)
            int windowSize = ary.Length / Environment.ProcessorCount;
            var rangePartitioner = Partitioner.Create(0, ary.Length, windowSize);//

            //over load no6
            Parallel.ForEach(rangePartitioner, (range) =>
            {
                long subtotal = 0;//パーティションごとの集計用(小計)
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    subtotal += ary[i];
                }
                Interlocked.Add(ref total, subtotal);//合計する
            });
            return total;
        }
        //Parallel.ForEachとPartitionerとパーティションローカル変数？
        ////Partitionerでの区切り位置指定＋ParallelOptionsでスレッド数指定、これだけは少し遅いかな
        private long Test71_ParallelForEachPartitionerThreadLocalVariable(int[] ary)
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
        //Parallel.ForEachとPartitionerとパーティションローカル変数？
        //区切り位置指定もスレッド数指定もなし、にしたけど誤差
        private long Test72_ParallelForEachPartitionerThreadLocalVariable(int[] ary)
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
        //Parallel.ForEachとPartitionerとパーティションローカル変数？
        //区切り位置指定なし＋Optionsでスレッド数指定あり、誤差程度に最速かも？
        private long Test73_ParallelForEachPartitionerThreadLocalVariable(int[] ary)
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





        //VectorAdd、intからlongへの変換
        //int型配列からlong型配列への変換は、計算する分だけをその都度変換
        private long TestVectorAddEach(int[] Ary)
        {
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            var longAry = new long[simdLength];
            var v = new Vector<long>(longAry);

            for (int j = 0; j < lastIndex; j += simdLength)
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
        //TaskとVector
        //配列の分割はLINQのSkipとTakeを使用
        private long Test80_TaskLinqSkipTake_Vector(int[] ary)
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
            int lastIndex = ary.Length - (ary.Length % windowSize);
            for (int i = lastIndex; i < ary.Length; i++)
            {
                total += ary[i];
            }
            return total;
        }

        //TaskとVector
        //配列の分割はFor
        private long Test81_TaskVectorAdd_ForInt(int[] ary)
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
            int lastIndex = ary.Length - (ary.Length % windowSize);
            for (int i = lastIndex; i < ary.Length; i++)
            {
                total += ary[i];
            }
            return total;
        }

        //TaskとVector
        //Forで配列を分割時にlong型に変換
        private long Test82_TaskVectorAdd_ForLong(int[] ary)
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
            int lastIndex = ary.Length - (ary.Length % windowSize);
            for (int i = lastIndex; i < ary.Length; i++)
            {
                total += ary[i];
            }
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

        //
        //Parallel.ForEach＋Partitioner＋Vector.Addを使って計算
        //int型配列
        private long Test90_ParallelForEachPartitioner_Vector(int[] ary)
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

        //Parallel.ForEach＋Partitioner＋Vector.Addを使って計算
        //long型配列
        private long Test91_ParallelForEachPartitioner_Vector(long[] ary)
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

        //Parallel.ForEach＋Partitioner＋Vector.Addを使って計算
        //int型配列、int型で集計するからオーバーフローする版
        private long Test9x_ParallelForEachPartitionerVector_Overflow(int[] ary)
        {
            long total = 0;
            int windowSize = ary.Length / Environment.ProcessorCount;
            var rangePartitioner = Partitioner.Create(0, ary.Length, windowSize);//
            int simdLength = Vector<int>.Count;
            Parallel.ForEach(rangePartitioner, (range) =>
            {
                int lastIndex = range.Item2 - (range.Item2 % simdLength);
                var v = new Vector<int>();
                for (int i = range.Item1; i < lastIndex; i += simdLength)
                {
                    v = System.Numerics.Vector.Add(v, new Vector<int>(ary, i));
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

        //Vector4で計算、Vector4はfloat型なので誤差が出る
        private long Test99x_ParallelForEachPartitionerVector4_Incorect(int[] ary)
        {
            long total = 0;
            int windowSize = ary.Length / Environment.ProcessorCount;
            var rangePartitioner = Partitioner.Create(0, ary.Length, windowSize);//
            int simdLength = 4;
            Parallel.ForEach(rangePartitioner, (range) =>
            {
                int lastIndex = range.Item2 - (range.Item2 % simdLength);
                var v = new Vector4();
                for (int i = range.Item1; i < lastIndex; i += simdLength)
                {
                    v = Vector4.Add(v, new Vector4(ary[i], ary[i + 1], ary[i + 2], ary[i + 3]));// System.Numerics.Vector.Add(v, new Vector<int>(ary, i));
                }
                long subtotal = (long)(v.X + v.Y + v.Z + v.W);
                //long subtotal = (long)((long)v.X + (long)v.Y + (long)v.Z + (long)v.W);
                for (int i = lastIndex; i < range.Item2; i++)
                {
                    subtotal += ary[i];
                }

                Interlocked.Add(ref total, subtotal);
            });
            return total;
        }

        private long Test99_ParallelForEachPartitioner_VectorWiden(int[] ary)
        {
            long total = 0;
            int windowSize = ary.Length / Environment.ProcessorCount;
            var rangePartitioner = Partitioner.Create(0, ary.Length, windowSize);//
            int simdLength = Vector<int>.Count;

            Parallel.ForEach(rangePartitioner, (range) =>
            {
                int lastIndex = range.Item2 - (range.Item2 % simdLength);
                var v = new Vector<long>();

                for (int i = range.Item1; i < lastIndex; i += simdLength)
                {
                    Vector<long> v1;
                    Vector<long> v2;
                    System.Numerics.Vector.Widen(new Vector<int>(ary, i), out v1, out v2);
                    v = System.Numerics.Vector.Add(v, v1);
                    v = System.Numerics.Vector.Add(v, v2);
                }
                long subtotal = 0;

                for (int i = 0; i < Vector<long>.Count; i++)
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
