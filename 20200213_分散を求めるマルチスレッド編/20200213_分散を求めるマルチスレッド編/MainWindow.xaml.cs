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
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;


namespace _20200213_分散を求めるマルチスレッド編
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyByteAry;
        //private int[] MyIntAry;
        //private long[] MyLongAry;
        private const int LOOP_COUNT = 100;
        private const int ELEMENT_COUNT = 1_000_000;
        private double MyAverage;

        public MainWindow()
        {
            InitializeComponent();

            MyTextBlock.Text = $"byte型配列の値の分散、要素数{ELEMENT_COUNT.ToString("N0")}の分散を{LOOP_COUNT}回求める";
            MyTextBlockVectorCount.Text = $"Vector<long>.Count = {Vector<long>.Count}";
            string str = $"VectorCount : Long={Vector<long>.Count}, Double={Vector<double>.Count}, int={Vector<int>.Count}, flort={Vector<float>.Count}, short={Vector<short>.Count}, byte={Vector<byte>.Count}";
            MyTextBlockVectorCount.Text = str;
            MyTextBlockCpuThreadCount.Text = $"CPUスレッド数：{Environment.ProcessorCount.ToString()} thread";
            MyInitialize();


            Button1.Click += (s, e) => MyExe(Test01_Double_ForLoop, Tb1, MyByteAry);
            Button2.Click += (s, e) => MyExe(Test02_Integer_ForLoop, Tb2, MyByteAry);
            //Button3.Click += (s, e) => MyExe(Test03_FloatVectorSubtractDot, Tb3, MyByteAry);
            Button4.Click += (s, e) => MyExe(Test04_DoubleVectorSubtractDot, Tb4, MyByteAry);
            Button5.Click += (s, e) => MyExe(Test01MT_Integer_ForLoop, Tb5, MyByteAry);
            Button6.Click += (s, e) => MyExe(Test02MT_Integer_ForLoop, Tb6, MyByteAry);
            Button7.Click += (s, e) => MyExe(Test021MT_double_ForLoop, Tb7, MyByteAry);
            Button8.Click += (s, e) => MyExe(Test022MT_Integer_ForLoop, Tb8, MyByteAry);
            //Button9.Click += (s, e) => MyExe(Test09_IntegerVectorDot, Tb9, MyByteAry);

            Button10.Click += (s, e) => MyExe(Test04_DoubleVectorSubtractDot, Tb10, MyByteAry);
            Button11.Click += (s, e) => MyExe(Test04MT_DoubleVectorDot, Tb11, MyByteAry);
            Button12.Click += (s, e) => MyExe(Test05MT_DoubleVectorDot, Tb12, MyByteAry);
            Button13.Click += (s, e) => MyExe(Test04MT_IntegerVectorDot, Tb13, MyByteAry);
            //Button14.Click += (s, e) => MyExe(Test14_DoubleVectorDot, Tb14, MyByteAry);
            //Button15.Click += (s, e) => MyExe(Test15_IntegerVectorDot, Tb15, MyByteAry);
            //Button16.Click += (s, e) => MyExe(Test16_ByteVectorDot_Overflow, Tb16, MyByteAry);
            //Button17.Click += (s, e) => MyExe(Test17_ShortVectorDot_Overflow, Tb17, MyByteAry);
            //Button18.Click += (s, e) => MyExe(Test18_FloatVector4, Tb18, MyByteAry);

            //Button19.Click += (s, e) => MyExe(Test19_Byte_ushort_uintVectorDot, Tb19, MyByteAry);
            //Button20.Click += (s, e) => MyExe(Test20_Byte_ushort_uintVectorDot, Tb20, MyByteAry);
        }

        private void MyInitialize()
        {
            MyByteAry = new byte[ELEMENT_COUNT];
            var r = new Random();
            r.NextBytes(MyByteAry);
            //要素の平均値
            //MyByteAry = new byte[] { 20, 21, 7, 12 };
            MyAverage = GetAverage(MyByteAry);
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
        private double Test01_Double_ForLoop(byte[] ary)
        {
            //平均との差(偏差)の2乗を合計
            double total = 0;
            for (int i = 0; i < ary.Length; i++)
            {
                total += Math.Pow(ary[i] - MyAverage, 2.0);
            }
            //合計 / 要素数 = 分散
            return total / ary.Length;
        }

        //普通のForループ、int型
        private double Test02_Integer_ForLoop(byte[] ary)
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

        //        方法: スレッド ローカル変数を使用する Parallel.For ループを記述する | Microsoft Docs
        //https://docs.microsoft.com/ja-jp/dotnet/standard/parallel-programming/how-to-write-a-parallel-for-loop-with-thread-local-variables
        //Parallel.For、途中までdouble型
        private double Test01MT_Integer_ForLoop(byte[] ary)
        {
            //平均との差(偏差)の2乗を合計
            long total = 0;
            Parallel.For<double>(0, ary.Length,
                () => 0,
                (j, loopState, subtotal) =>
                {
                    double diff = ary[j] - MyAverage;//差
                    return subtotal += diff * diff;//差の2乗の小計
                },
                (x) => Interlocked.Add(ref total, (int)x));//小計を合計、InterlockedAddは整数しか扱えない

            //合計 / 要素数 = 分散
            return (double)total / ary.Length;
        }

        //Parallel.For、int型で計算
        private double Test02MT_Integer_ForLoop(byte[] ary)
        {
            //平均との差(偏差)の2乗を合計
            long total = 0;
            int average = (int)MyAverage;
            Parallel.For<int>(0, ary.Length,
                () => 0,
                (j, loopState, subtotal) =>
                {
                    int diff = ary[j] - average;//差
                    return subtotal += diff * diff;//差の2乗の小計
                },
                (x) => Interlocked.Add(ref total, x));//小計を合計、InterlockedAddは整数しか扱えない

            //合計 / 要素数 = 分散
            return (double)total / ary.Length;
        }


        //        c# — C＃で整数の配列を合計する方法
        //https://www.it-swarm.dev/ja/c%23/c%EF%BC%83%E3%81%A7%E6%95%B4%E6%95%B0%E3%81%AE%E9%85%8D%E5%88%97%E3%82%92%E5%90%88%E8%A8%88%E3%81%99%E3%82%8B%E6%96%B9%E6%B3%95/968057375/
        //Paralle.ForEach、途中までdouble型
        private double Test021MT_double_ForLoop(byte[] ary)
        {
            //平均との差(偏差)の2乗を合計
            long total = 0;
            var partitioner = Partitioner.Create(0, ary.Length);
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length),
                (range) =>
                {
                    double subtotal = 0;
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        subtotal += Math.Pow(ary[i] - MyAverage, 2.0);
                    }
                    Interlocked.Add(ref total, (int)subtotal);
                });

            //合計 / 要素数 = 分散
            return (double)total / ary.Length;
        }
        //Paralle.ForEach、int型で計算
        private double Test022MT_Integer_ForLoop(byte[] ary)
        {
            //平均との差(偏差)の2乗を合計
            long total = 0;
            int average = (int)MyAverage;
            var partitioner = Partitioner.Create(0, ary.Length);
            Parallel.ForEach(
                Partitioner.Create(0, ary.Length),
                (range) =>
                {
                    int subtotal = 0;
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
        private double Test04_DoubleVectorSubtractDot(byte[] ary)
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
            return total / ary.Length;
        }

        //Vector<double>で計算、スレッドごとの集計はlong
        //マルチスレッド
        //パーティション区切りはCPUスレッド数で分けている
        //分けた後の要素数がVectorCountより小さいとエラーになる
        //CPUスレッド数8、Vector<double>.Countが4の環境だと
        //8*4=32、要素数32以上の配列じゃないとエラーになる
        private double Test04MT_DoubleVectorDot(byte[] ary)
        {
            int simdLength = Vector<double>.Count;
            long total = 0;
            //パーティションサイズ、要素数をCPUスレッド数で割った数値
            int rangeSize = ary.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, ary.Length, rangeSize),
                (range) =>
                {
                    double subtotal = 0;
                    int r = range.Item2 - range.Item1;
                    int lastIndex = r - (r % simdLength);
                    double[] aa = new double[simdLength];
                    Vector<double> v;
                    for (int i = range.Item1; i < range.Item2; i += simdLength)
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
                        subtotal += MySuquareSum(ary, range.Item2 - (r % simdLength), range.Item2, MyAverage);
                    }
                    //排他処理でスレッドごとの小計を加算、InterlockedAddは整数しか扱えない
                    Interlocked.Add(ref total, (long)subtotal);
                });

            return (double)total / ary.Length;
        }

        //Vector<double>で計算、スレッドごとの集計もdouble型はCuncurrentBagを使用
        private double Test05MT_DoubleVectorDot(byte[] ary)
        {
            int simdLength = Vector<double>.Count;
            var bag = new ConcurrentBag<double>();
            //パーティションサイズ、要素数をCPUスレッド数で割った数値
            int rangeSize = ary.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, ary.Length, rangeSize),
                (range) =>
                {
                    double subtotal = 0;
                    int r = range.Item2 - range.Item1;
                    int lastIndex = r - (r % simdLength);
                    double[] aa = new double[simdLength];
                    Vector<double> v;
                    for (int i = range.Item1; i < range.Item2; i += simdLength)
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
                        subtotal += MySuquareSum(ary, range.Item2 - (r % simdLength), range.Item2, MyAverage);
                    }
                    //排他処理でスレッドごとの小計をコレクションに追加
                    bag.Add(subtotal);                    
                });
            double total = bag.Sum();
            return total / ary.Length;
        }

        //Vector<int>で計算
        private double Test04MT_IntegerVectorDot(byte[] ary)
        {
            int simdLength = Vector<int>.Count;
            long total = 0;
            int average = (int)MyAverage;
            //パーティションサイズ、要素数をCPUスレッド数で割った数値
            int rangeSize = ary.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, ary.Length, rangeSize),
                (range) =>
                {
                    int subtotal = 0;
                    int r = range.Item2 - range.Item1;
                    int lastIndex = r - (r % simdLength);
                    int[] aa = new int[simdLength];
                    Vector<int> v;
                    for (int i = range.Item1; i < range.Item2; i += simdLength)
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
                        subtotal += (int)MySuquareSum(ary, range.Item2 - (r % simdLength), range.Item2, MyAverage);
                    }
                    //排他処理でスレッドごとの小計を加算、InterlockedAddは整数しか扱えない
                    Interlocked.Add(ref total, subtotal);
                });

            return (double)total / ary.Length;
        }





        //指定インデックスから最後までの偏差の2乗和を返す
        //VectorCountで割り切れなかった余り用
        private double MySuquareSum(byte[] ary, int beginIndex, int endIndex, double average)
        {
            double total = 0;
            for (int i = beginIndex; i < endIndex; i++)
            {
                total += Math.Pow(ary[i] - average, 2.0);
            }
            return total;
        }








        #endregion 分散の求め方その1



















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
            tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  分散 = {total.ToString("F10")}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
        }

    }
}
