using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Numerics;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections.Generic;
//System.Numerics.Vector.Count AVX - Google 検索
//https://www.google.com/search?q=System.Numerics.Vector.Count+AVX&oq=System.Numerics.Vector.Count+AVX&aqs=chrome..69i57&sourceid=chrome&ie=UTF-8

//            Hardware Intrinsics in .NET Core | .NET Blog
//https://devblogs.microsoft.com/dotnet/hardware-intrinsics-in-net-core/

//            System.Runtime.Intrinsics.X86.Avx.
namespace _20200213_分散を求めるマルチスレッド編
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyByteAry;
        private const int LOOP_COUNT = 100;
        private const int ELEMENT_COUNT = 10_000_000;//1000万
        //private const int ELEMENT_COUNT = 3264 * 2448;//7,990,272、今使っているスマカメは800万
        //private const int ELEMENT_COUNT = 7680 * 4320;//33,177,600、8K解像度は、3300万
        private double MyAverage;



        public MainWindow()
        {
            InitializeComponent();


            MyInitialize();
            SetRandomByte();

            ButtonReset.Click += (s, e) => MyReset();
            ButtonAll.Click += (s, e) => MyExeAll();

            Button1.Click += (s, e) => MyExe(Test01_V1ST_ForLoop_double, Tb1, MyByteAry);
            Button2.Click += (s, e) => MyExe(Test02_V1ST_ForLoop_int, Tb2, MyByteAry);
            Button3.Click += (s, e) => MyExe(Test03_V1_ParallelFor_double_ConcurrentBag, Tb3, MyByteAry);
            Button4.Click += (s, e) => MyExe(Test04_V1_ParallelFor_Integer_ConcurrentBag, Tb4, MyByteAry);
            Button5.Click += (s, e) => MyExe(Test05_V1_ParallelFor_Integer_InterlockedAdd, Tb5, MyByteAry);
            Button6.Click += (s, e) => MyExe(Test06_V1_ParallelForE_double, Tb6, MyByteAry);
            Button7.Click += (s, e) => MyExe(Test07_V1_ParallelForE_Integer, Tb7, MyByteAry);
            //vector
            Button8.Click += (s, e) => MyExe(Test08_V1ST_VectorDouble, Tb8, MyByteAry);
            Button9.Click += (s, e) => MyExe(Test09_V1_ParallelForE_VectorDouble, Tb9, MyByteAry);
            Button10.Click += (s, e) => MyExe(Test10_V1_ParallelForE_VectorInteger, Tb10, MyByteAry);
            Button11.Click += (s, e) => MyExe(Test11_V1_ParallelForE_VectorFloat, Tb11, MyByteAry);
            Button12.Click += (s, e) => MyExe(Test12_V1_ParallelForE_Vector4, Tb12, MyByteAry);

            Button13.Click += (s, e) => MyExe(Test13_V2ST_ForLoop, Tb13, MyByteAry);
            Button14.Click += (s, e) => MyExe(Test14_V2_ParallelFor, Tb14, MyByteAry);
            Button15.Click += (s, e) => MyExe(Test15_V2_ParallelForE, Tb15, MyByteAry);
            Button16.Click += (s, e) => MyExe(Test16_V2ST_VectorDouble, Tb16, MyByteAry);
            Button17.Click += (s, e) => MyExe(Test17_V2_ParallelForE_VectorDouble, Tb17, MyByteAry);
            Button18.Click += (s, e) => MyExe(Test18_V2_ParallelForE_VectorInt, Tb18, MyByteAry);
            Button19.Click += (s, e) => MyExe(Test19_V2_ParallelForE_VectorByteWiden, Tb19, MyByteAry);
            Button20.Click += (s, e) => MyExe(Test20_V2_ParallelForE_Vector4, Tb20, MyByteAry);

            Button21.Click += (s, e) => MyExe(Test21_V1_ParallelLinq, Tb21, MyByteAry);
            Button22.Click += (s, e) => MyExe(Test22_V2_ParallelLinq, Tb22, MyByteAry);


            //Button24.Click += (s, e) => MyExe(Test22_V2_ParallelLinq, Tb24, MyByteAry);
            //Button25.Click += (s, e) => MyExe(Test21_V1_ParallelLinq, Tb25, MyByteAry);
        }

        private void MyInitialize()
        {
            MyTextBlock.Text = $"byte型配列の値の分散、要素数{ELEMENT_COUNT.ToString("N0")}の分散を{LOOP_COUNT}回求める";
            MyTextBlockVectorCount.Text = $"Vector<long>.Count = {Vector<long>.Count}";
            string str = $"VectorCount : Long={Vector<long>.Count}, Double={Vector<double>.Count}, int={Vector<int>.Count}, flort={Vector<float>.Count}, short={Vector<short>.Count}, byte={Vector<byte>.Count}";
            MyTextBlockVectorCount.Text = str;
            MyTextBlockCpuThreadCount.Text = $"CPUスレッド数：{Environment.ProcessorCount.ToString()} thread";

        }
        private void SetRandomByte()
        {
            MyByteAry = new byte[ELEMENT_COUNT];
            var r = new Random();
            r.NextBytes(MyByteAry);
            //MyByteAry = new byte[] { 20, 21, 7, 12 };

            //Span<byte> span = new Span<byte>(MyByteAry);
            //span.Fill(0);
            //MyByteAry = span.ToArray();
            //MyByteAry[0] = 255;

            //var span = new Span<byte>(MyByteAry);
            //span.Fill(2);
            //MyByteAry[ELEMENT_COUNT - 1] = 1;

            MyAverage = GetAverage(MyByteAry);//要素の平均値

        }

        //平均値
        private double GetAverage(byte[] ary)
        {
            long total = 0;
            Parallel.ForEach(Partitioner.Create(0, ary.Length),
                (range) =>
                {
                    long subtotal = 0;
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        subtotal += ary[i];
                    }
                    Interlocked.Add(ref total, subtotal);
                });
            return total / (double)ary.Length;
        }



        #region 分散の求め方その1
        //普通のForループ、double型
        private double Test01_V1ST_ForLoop_double(byte[] ary)
        {
            //平均との差(偏差)の2乗を合計
            double total = 0;
            for (int i = 0; i < ary.Length; i++)
            {
                //total += Math.Pow(ary[i] - MyAverage, 2.0);//遅い33秒
                double diff = ary[i] - MyAverage;//速い0.9秒
                total += diff * diff;
            }
            //合計 / 要素数 = 分散
            return total / ary.Length;
        }

        //普通のForループ、int型
        private double Test02_V1ST_ForLoop_int(byte[] ary)
        {
            //平均との差の2乗を合計
            long total = 0;
            int average = (int)MyAverage;
            int ii;//ループの外に出したほうが誤差程度に速い
            for (int i = 0; i < ary.Length; i++)
            {
                //total += (int)Math.Pow(ary[i] - average, 2);
                //total += (ary[i] - average) * (ary[i] - average);//こっちのほうが↑より10倍以上速い
                ii = ary[i] - average;
                total += ii * ii;
            }
            //合計 / 要素数 = 分散
            return total / (double)ary.Length;
        }

        //マルチスレッド
        //Parallel.For、すべてdouble型で計算
        private double Test03_V1_ParallelFor_double_ConcurrentBag(byte[] ary)
        {
            //平均との差(偏差)の2乗を合計
            var myBag = new ConcurrentBag<double>();
            Parallel.For<double>(0, ary.Length,
                () => 0,
                (j, loopState, subtotal) =>
                {
                    double diff = ary[j] - MyAverage;//差
                    return subtotal += diff * diff;//差の2乗の小計
                },
                (x) => myBag.Add(x));//CuncurrentBagは排他処理で要素を追加できる

            double total = myBag.Sum();//合計
            //合計 / 要素数 = 分散
            return total / ary.Length;
        }
        //Parallel.For、偏差を整数で計算してdouble型との差を見る
        private double Test04_V1_ParallelFor_Integer_ConcurrentBag(byte[] ary)
        {
            var myBag = new ConcurrentBag<long>();
            int average = (int)MyAverage;//平均値を整数に
            Parallel.For<long>(0, ary.Length,
                () => 0,
                (j, loopState, subtotal) =>
                {
                    long diff = ary[j] - average;//整数で計算
                    return subtotal += diff * diff;
                },
                (x) => myBag.Add(x));

            long total = myBag.Sum();
            //最後だけdouble型
            return (double)total / ary.Length;
        }

        //Parallel.For、整数で計算
        //排他処理での合計をInterlocked.Addにして、ConcurrentBagとの差を見る
        private double Test05_V1_ParallelFor_Integer_InterlockedAdd(byte[] ary)
        {
            //平均との差(偏差)の2乗を合計
            long total = 0;
            int average = (int)MyAverage;
            Parallel.For<long>(0, ary.Length,
                () => 0,
                (j, loopState, subtotal) =>
                {
                    int diff = ary[j] - average;//差
                    return subtotal += diff * diff;//差の2乗の小計
                },
                (x) => Interlocked.Add(ref total, x));//排他処理で合計、InterlockedAddは整数しか扱えない

            //合計 / 要素数 = 分散
            return (double)total / ary.Length;
        }



        //        c# — C＃で整数の配列を合計する方法
        //https://www.it-swarm.dev/ja/c%23/c%EF%BC%83%E3%81%A7%E6%95%B4%E6%95%B0%E3%81%AE%E9%85%8D%E5%88%97%E3%82%92%E5%90%88%E8%A8%88%E3%81%99%E3%82%8B%E6%96%B9%E6%B3%95/968057375/
        //Paralle.ForEach、double型
        private double Test06_V1_ParallelForE_double(byte[] ary)
        {
            //平均との差(偏差)の2乗を合計

            var myBag = new ConcurrentBag<double>();
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length),
                (range) =>
                {
                    double subtotal = 0;
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        //subtotal += Math.Pow(ary[i] - MyAverage, 2.0);
                        double diff = ary[i] - MyAverage;
                        subtotal += diff * diff;
                    }
                    myBag.Add(subtotal);//小計を追加
                });
            double total = myBag.Sum();//合計
            //合計 / 要素数 = 分散
            return total / ary.Length;
        }
        //Paralle.ForEach、整数型で計算
        private double Test07_V1_ParallelForE_Integer(byte[] ary)
        {
            //平均との差(偏差)の2乗を合計
            long total = 0;
            int average = (int)MyAverage;
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length),
                (range) =>
                {
                    long subtotal = 0;
                    int diff;
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        diff = ary[i] - average;
                        subtotal += diff * diff;
                    }
                    Interlocked.Add(ref total, subtotal);
                });

            //合計 / 要素数 = 分散
            return (double)total / ary.Length;
        }


        //ここからVector
        //Vector<double>で計算、シングルスレッド
        private double Test08_V1ST_VectorDouble(byte[] ary)
        {
            var vAverage = new Vector<double>(MyAverage);
            int simdLength = Vector<double>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<double> v;
            var ss = new double[simdLength];
            double total = 0;
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                for (int j = 0; j < simdLength; j++)
                {
                    ss[j] = ary[i + j];
                }
                //平均との差
                v = System.Numerics.Vector.Subtract(vAverage, new Vector<double>(ss));
                //差の2乗を合計
                total += System.Numerics.Vector.Dot(v, v);
            }
            //配列とVectorCountの剰余分も加算
            total += MySuquareSumOfDeviation偏差の2乗和(ary, lastIndex, ary.Length, MyAverage);
            return total / ary.Length;
        }

        //マルチスレッド
        //Vector<double>で計算、スレッドごとの集計もdouble型
        //ConcurentBagならdouble型も扱える
        //パーティション区切りはCPUスレッド数で分けている
        //分けた後の要素数がVectorCountより小さいとエラーになる
        //例えばCPUスレッド数8、Vector<double>.Countが4の環境だと
        //8*4=32、要素数32以上の配列じゃないとエラーになる
        private double Test09_V1_ParallelForE_VectorDouble(byte[] ary)
        {
            int simdLength = Vector<double>.Count;
            var bag = new ConcurrentBag<double>();
            //パーティションサイズ、要素数をCPUスレッド数で割ったサイズに設定した
            int rangeSize = ary.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, ary.Length, rangeSize),
                (range) =>
                {
                    double subtotal = 0;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    double[] aa = new double[simdLength];
                    Vector<double> v;
                    for (int i = range.Item1; i < lastIndex; i += simdLength)
                    {
                        //偏差の配列作成
                        for (int j = 0; j < simdLength; j++)
                        {
                            aa[j] = ary[i + j] - MyAverage;//偏差
                        }
                        //偏差の配列からVector作成
                        v = new Vector<double>(aa);
                        subtotal += System.Numerics.Vector.Dot(v, v);
                        //VectorCountで割り切れなかった余りの2乗和も加算
                        subtotal += MySuquareSumOfDeviation偏差の2乗和(ary, lastIndex, range.Item2, MyAverage);
                    }
                    //排他処理でスレッドごとの小計をコレクションに追加
                    bag.Add(subtotal);
                });

            return bag.Sum() / ary.Length;
        }



        //Vector<int>で計算、小計、合計はlongで計算、intだと小計でもオーバーフローした
        private double Test10_V1_ParallelForE_VectorInteger(byte[] ary)
        {
            int simdLength = Vector<int>.Count;
            long total = 0;
            int average = (int)MyAverage;
            int rangeSize = ary.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, ary.Length, rangeSize),
                (range) =>
                {
                    MyVariance(range, simdLength, ary, average, ref total);

                    //全く同じ処理なんだけど↓より↑のほうが速い

                    //int subtotal = 0;
                    //int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    //int[] aa = new int[simdLength];
                    //Vector<int> v;
                    //for (int i = range.Item1; i < lastIndex; i += simdLength)
                    //{
                    //    //偏差の配列作成
                    //    for (int j = 0; j < simdLength; j++)
                    //    {
                    //        aa[j] = ary[i + j] - average;//偏差
                    //    }
                    //    //偏差の配列からVector作成
                    //    v = new Vector<int>(aa);
                    //    subtotal += System.Numerics.Vector.Dot(v, v);
                    //    //VectorCountで割り切れなかった余りの2乗和も加算
                    //    subtotal += (int)MySuquareSumOfDeviation偏差の2乗和(ary, lastIndex, range.Item2, MyAverage);
                    //}
                    ////排他処理でスレッドごとの小計を加算、InterlockedAddは整数しか扱えない
                    //Interlocked.Add(ref total, subtotal);
                });

            return (double)total / ary.Length;
        }
        private void MyVariance(Tuple<int, int> range, int simdLength, byte[] ary, int average, ref long total)
        {
            long subtotal = 0;
            int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;

            int[] aa = new int[simdLength];
            Vector<int> v;
            for (int i = range.Item1; i < lastIndex; i += simdLength)
            {
                //偏差の配列作成
                for (int j = 0; j < simdLength; j++)
                {
                    aa[j] = ary[i + j] - average;//偏差
                }
                //偏差の配列からVector作成
                v = new Vector<int>(aa);
                subtotal += System.Numerics.Vector.Dot(v, v);
                //VectorCountで割り切れなかった余りの2乗和も加算
                subtotal += (int)MySuquareSumOfDeviation偏差の2乗和(ary, lastIndex, range.Item2, MyAverage);
            }
            //排他処理でスレッドごとの小計を加算、InterlockedAddは整数しか扱えない
            Interlocked.Add(ref total, subtotal);
        }


        //Vector<float>で計算、小計はdouble、合計もdouble
        //ConcurentBagで排他処理合計
        private double Test11_V1_ParallelForE_VectorFloat(byte[] ary)
        {
            int simdLength = Vector<float>.Count;
            var bag = new ConcurrentBag<double>();
            float average = (float)MyAverage;
            int rangeSize = ary.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, ary.Length, rangeSize),
                (range) =>
                {
                    double subtotal = 0;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    float[] aa = new float[simdLength];
                    Vector<float> v;
                    for (int i = range.Item1; i < lastIndex; i += simdLength)
                    {
                        for (int j = 0; j < simdLength; j++)
                        {
                            aa[j] = ary[i + j] - average;
                        }
                        v = new Vector<float>(aa);
                        subtotal += System.Numerics.Vector.Dot(v, v);
                        subtotal += MySuquareSumOfDeviation偏差の2乗和(ary, lastIndex, range.Item2, MyAverage);
                    }
                    //排他処理でスレッドごとの小計をコレクションに追加
                    bag.Add(subtotal);
                });
            double total = bag.Sum();
            return total / ary.Length;
        }

        //Vector4
        private double Test12_V1_ParallelForE_Vector4(byte[] ary)
        {
            int simdLength = 4;
            var bag = new ConcurrentBag<double>();
            var vAverage = new Vector4((float)MyAverage);
            int rangeSize = ary.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, ary.Length, rangeSize),
                (range) =>
                {
                    double subtotal = 0;
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    float[] aa = new float[simdLength];
                    Vector4 v;
                    for (int i = range.Item1; i < lastIndex; i += simdLength)
                    {
                        v = new Vector4(ary[i], ary[i + 1], ary[i + 2], ary[i + 3]);
                        v = Vector4.Subtract(v, vAverage);
                        subtotal += Vector4.Dot(v, v);
                        subtotal += MySuquareSumOfDeviation偏差の2乗和(ary, lastIndex, range.Item2, MyAverage);
                    }
                    bag.Add(subtotal);
                });
            double total = bag.Sum();
            return total / ary.Length;
        }














        #endregion 分散の求め方その1ここまで

        //-------------------------------------------------------------

        #region 分散の求め方その2ここから
        //分散 = 2乗の平均 - 平均の2乗

        //シングルスレッド
        private double Test13_V2ST_ForLoop(byte[] ary)
        {
            //2乗和
            double total = 0;
            for (int i = 0; i < ary.Length; i++)
            {
                total += ary[i] * ary[i];
            }
            //2乗の平均
            total /= ary.Length;
            //2乗の平均 - 平均の2乗
            return total - (MyAverage * MyAverage);
        }


        //マルチスレッドここから
        private double Test14_V2_ParallelFor(byte[] ary)
        {
            //2乗和            
            var myBag = new ConcurrentBag<long>();
            //var options = new ParallelOptions();
            //options.MaxDegreeOfParallelism = Environment.ProcessorCount;
            Parallel.For<long>(0, ary.Length,
                () => 0,
                (j, state, subtotal) => { return subtotal += ary[j] * ary[j]; },
                (x) => myBag.Add(x));
            long total = myBag.Sum();
            return ((double)total / ary.Length) - (MyAverage * MyAverage);//2乗の平均 - 平均の2乗
        }

        //整数で計算
        private double Test15_V2_ParallelForE(byte[] ary)
        {
            //2乗和            
            var myBag = new ConcurrentBag<long>();
            var partition = Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount);
            Parallel.ForEach(partition, (range) =>
            {
                long subtotal = 0;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    subtotal += ary[i] * ary[i];
                }
                myBag.Add(subtotal);//スレッドごとの小計
            });
            long total = myBag.Sum();//合計
            return ((double)total / ary.Length) - (MyAverage * MyAverage);////2乗の平均 - 平均の2乗
        }



        //ここからVector
        private double Test16_V2ST_VectorDouble(byte[] ary)
        {
            int simdLength = Vector<double>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<double> v;
            var ss = new double[simdLength];
            double total = 0;
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                for (int j = 0; j < simdLength; j++)
                {
                    ss[j] = ary[i + j];
                }
                v = new Vector<double>(ss);
                total += System.Numerics.Vector.Dot(v, v);
            }
            //配列とVectorCountの剰余分も加算
            total += MySuquareSum2乗和(ary, lastIndex, ary.Length, simdLength);
            return (total / ary.Length) - (MyAverage * MyAverage);
        }
        //Vector<double>で計算、byte型の2乗和を求めるからdouble型はあんまり意味ない
        private double Test17_V2_ParallelForE_VectorDouble(byte[] ary)
        {
            var myBag = new ConcurrentBag<double>();
            var partition = Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount);
            int simdLength = Vector<double>.Count;
            Parallel.ForEach(partition, (range) =>
            {
                double subtotal = 0;
                var aa = new double[simdLength];
                int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                Vector<double> v;
                for (int i = range.Item1; i < lastIndex; i += simdLength)
                {
                    for (int j = 0; j < simdLength; j++)
                    {
                        aa[j] = ary[i + j];
                    }
                    v = new Vector<double>(aa);
                    subtotal += System.Numerics.Vector.Dot(v, v);
                }
                //配列とVectorCountの剰余分も加算
                subtotal += MySuquareSum2乗和(ary, range.Item1, range.Item2, simdLength);
                myBag.Add(subtotal);
            });

            double total = myBag.Sum();
            return (total / ary.Length) - (MyAverage * MyAverage);
        }
        //Vector<int>で計算、整数で計算
        private double Test18_V2_ParallelForE_VectorInt(byte[] ary)
        {
            var myBag = new ConcurrentBag<long>();
            int simdLength = Vector<int>.Count;
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount),
                (range) =>
                {
                    long subtotal = 0;
                    var aa = new int[simdLength];
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    Vector<int> v;
                    for (int i = range.Item1; i < lastIndex; i += simdLength)
                    {
                        for (int j = 0; j < simdLength; j++)
                        {
                            aa[j] = ary[i + j];
                        }
                        v = new Vector<int>(aa);
                        subtotal += System.Numerics.Vector.Dot(v, v);
                    }
                    //配列とVectorCountの剰余分も加算
                    subtotal += MySuquareSum2乗和(ary, range.Item1, range.Item2, simdLength);
                    myBag.Add(subtotal);
                });

            double total = myBag.Sum();
            return (total / ary.Length) - (MyAverage * MyAverage);
        }


        //Widen
        //uint 4,294,967,295まで、ushort 65535まで
        //ushortでもオーバーフローするのでuintまで伸長してからドット積
        private double Test19_V2_ParallelForE_VectorByteWiden(byte[] ary)
        {
            var myBag = new ConcurrentBag<long>();
            var partition = Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount);
            int simdLength = Vector<byte>.Count;
            Parallel.ForEach(partition, (range) =>
            {
                long subtotal = 0;
                int lastIndex = range.Item2 - ((range.Item2 - range.Item1) % simdLength);
                for (int i = range.Item1; i < lastIndex; i += simdLength)
                {
                    System.Numerics.Vector.Widen(new Vector<byte>(ary, i), out Vector<ushort> v1, out Vector<ushort> v2);
                    System.Numerics.Vector.Widen(v1, out Vector<uint> vv1, out Vector<uint> vv2);
                    System.Numerics.Vector.Widen(v2, out Vector<uint> vv3, out Vector<uint> vv4);
                    subtotal += System.Numerics.Vector.Dot(vv1, vv1);
                    subtotal += System.Numerics.Vector.Dot(vv2, vv2);
                    subtotal += System.Numerics.Vector.Dot(vv3, vv3);
                    subtotal += System.Numerics.Vector.Dot(vv4, vv4);
                }
                //配列とVectorCountの剰余分も加算
                subtotal += MySuquareSum2乗和(ary, range.Item1, range.Item2, simdLength);
                myBag.Add(subtotal);
            });

            long total = myBag.Sum();
            return ((double)total / ary.Length) - (MyAverage * MyAverage);
        }

        //Vector4はfloat型で計算
        private double Test20_V2_ParallelForE_Vector4(byte[] ary)
        {
            var myBag = new ConcurrentBag<double>();
            var partition = Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount);
            int simdLength = 4;//Vector4は4固定
            Parallel.ForEach(partition, (range) =>
            {
                double subtotal = 0;
                int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                Vector4 v;
                for (int i = range.Item1; i < lastIndex; i += simdLength)
                {
                    v = new Vector4(ary[i], ary[i + 1], ary[i + 2], ary[i + 3]);
                    subtotal += Vector4.Dot(v, v);
                }
                //配列とVectorCountの剰余分も加算
                subtotal += MySuquareSum2乗和(ary, range.Item1, range.Item2, simdLength);
                myBag.Add(subtotal);
            });

            double total = myBag.Sum();
            return (total / ary.Length) - (MyAverage * MyAverage);
        }





        #endregion 分散の求め方その2ここまで


        //Linq
        private double Test21_V1_ParallelLinq(byte[] ary)
        {
            //return ary.AsParallel().Select(x => Math.Pow(x - MyAverage, 2)).Sum() / ary.Length;//遅い11秒
            return ary.AsParallel().Select(x => (x - MyAverage) * (x - MyAverage)).Sum() / ary.Length;//3.5秒
        }
        private double Test22_V2_ParallelLinq(byte[] ary)
        {
            return (ary.AsParallel().Select(x => x * x).Sum(x => (long)x) / (double)ary.Length) - (MyAverage * MyAverage);//こっちのほうが↓より3割速い、精度は同じ
            //return (ary.AsParallel().Select(x => x * x).Sum(x => (double)x) / ary.Length) - (MyAverage * MyAverage);
        }







        private void MyReset()
        {
            var neko = EnumerateDescendantObjects2<TextBlock>(this);
            foreach (var item in neko)
            {
                item.Text = "";
            }
            MyInitialize();
        }

        #region コントロールの列挙

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




        //指定インデックスから最後までの偏差の2乗和を返す
        //VectorCountで割り切れなかった余り用、分散の求め方その1用
        private double MySuquareSumOfDeviation偏差の2乗和(byte[] ary, int beginIndex, int endIndex, double average)
        {
            double total = 0;
            for (int i = beginIndex; i < endIndex; i++)
            {
                //total += Math.Pow(ary[i] - average, 2.0);//MathPowは遅い
                var deviation = ary[i] - average;//偏差
                total += deviation * deviation;
            }
            return total;
        }

        /// <summary>
        /// byte型配列用、分散の求め方その2用、VectorCountで割り切れなかった余りの要素の2乗和を返す、要素数10でVectorCountが4のとき10%4=2なので、最後の2つの要素が対象になる
        /// </summary>
        /// <param name="ary">配列</param>
        /// <param name="beginIndex">範囲の開始インデックス</param>
        /// <param name="endIndex">範囲の終了インデックス</param>
        /// <param name="simdLength">Vector[T].Count</param>
        /// <returns></returns>
        private int MySuquareSum2乗和(byte[] ary, int beginIndex, int endIndex, int simdLength)
        {
            //あまりの位置のインデックス
            int lastIndex = endIndex - (endIndex - beginIndex) % simdLength;
            //2乗して合計
            int total = 0;
            for (int i = lastIndex; i < endIndex; i++)
            {
                total += ary[i] * ary[i];
            }
            return total;
        }








        private void MyExe(Func<byte[], double> func, TextBlock tb, byte[] ary)
        {
            double total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(ary);
            }
            sw.Stop();
            //tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  分散 = {total.ToString("F10")}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
            this.Dispatcher.Invoke(() =>
            {
                tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  分散 = {total.ToString("F15")}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
            });
        }

        private async void MyExeAll()
        {
            var sw = new Stopwatch();
            sw.Start();
            this.IsEnabled = false;
            await Task.Run(() => MyExe(Test02_V1ST_ForLoop_int, Tb2, MyByteAry));
            await Task.Run(() => MyExe(Test03_V1_ParallelFor_double_ConcurrentBag, Tb3, MyByteAry));
            await Task.Run(() => MyExe(Test04_V1_ParallelFor_Integer_ConcurrentBag, Tb4, MyByteAry));
            await Task.Run(() => MyExe(Test05_V1_ParallelFor_Integer_InterlockedAdd, Tb5, MyByteAry));
            await Task.Run(() => MyExe(Test06_V1_ParallelForE_double, Tb6, MyByteAry));
            await Task.Run(() => MyExe(Test07_V1_ParallelForE_Integer, Tb7, MyByteAry));
            await Task.Run(() => MyExe(Test08_V1ST_VectorDouble, Tb8, MyByteAry));
            await Task.Run(() => MyExe(Test09_V1_ParallelForE_VectorDouble, Tb9, MyByteAry));
            await Task.Run(() => MyExe(Test10_V1_ParallelForE_VectorInteger, Tb10, MyByteAry));
            await Task.Run(() => MyExe(Test11_V1_ParallelForE_VectorFloat, Tb11, MyByteAry));
            await Task.Run(() => MyExe(Test12_V1_ParallelForE_Vector4, Tb12, MyByteAry));
            await Task.Run(() => MyExe(Test13_V2ST_ForLoop, Tb13, MyByteAry));
            await Task.Run(() => MyExe(Test14_V2_ParallelFor, Tb14, MyByteAry));
            await Task.Run(() => MyExe(Test15_V2_ParallelForE, Tb15, MyByteAry));
            await Task.Run(() => MyExe(Test16_V2ST_VectorDouble, Tb16, MyByteAry));
            await Task.Run(() => MyExe(Test17_V2_ParallelForE_VectorDouble, Tb17, MyByteAry));
            await Task.Run(() => MyExe(Test18_V2_ParallelForE_VectorInt, Tb18, MyByteAry));
            await Task.Run(() => MyExe(Test19_V2_ParallelForE_VectorByteWiden, Tb19, MyByteAry));
            await Task.Run(() => MyExe(Test20_V2_ParallelForE_Vector4, Tb20, MyByteAry));
            await Task.Run(() => MyExe(Test21_V1_ParallelLinq, Tb21, MyByteAry));
            await Task.Run(() => MyExe(Test22_V2_ParallelLinq, Tb22, MyByteAry));
            //await Task.Run(() => MyExe(Test08_V1ST_VectorDouble, Tb23, MyByteAry));
            //await Task.Run(() => MyExe(Test08_V1ST_VectorDouble, Tb24, MyByteAry));
            this.IsEnabled = true;
            sw.Stop();
            TbAll.Text = $"一斉テスト時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒";

        }

    }
}
