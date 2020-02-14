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
        private const int LOOP_COUNT = 10;
        private const int ELEMENT_COUNT = 1_000_003;
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
            Button5.Click += (s, e) => MyExe(Test01MT_double_ParallelFor, Tb5, MyByteAry);
            Button6.Click += (s, e) => MyExe(Test02MT_Integer_ParallelFor, Tb6, MyByteAry);
            Button7.Click += (s, e) => MyExe(Test021MT_double_ParallelForEach, Tb7, MyByteAry);
            Button8.Click += (s, e) => MyExe(Test022MT_Integer_ParallelForEach, Tb8, MyByteAry);
            //Button9.Click += (s, e) => MyExe(Test09_IntegerVectorDot, Tb9, MyByteAry);

            Button10.Click += (s, e) => MyExe(Test04_DoubleVectorSubtractDot, Tb10, MyByteAry);
            Button11.Click += (s, e) => MyExe(Test04MT_DoubleVectorDot, Tb11, MyByteAry);
            Button12.Click += (s, e) => MyExe(Test05MT_DoubleVectorDot, Tb12, MyByteAry);
            Button13.Click += (s, e) => MyExe(Test04MT_IntegerVectorDot, Tb13, MyByteAry);
            Button14.Click += (s, e) => MyExe(Test06MT_floatIntVectorDot, Tb14, MyByteAry);
            Button15.Click += (s, e) => MyExe(Test06MT_floatfloatVectorDot, Tb15, MyByteAry);
            Button16.Click += (s, e) => MyExe(Test07MT_Vector4SubtractDot, Tb16, MyByteAry);
            Button17.Click += (s, e) => MyExe(Test11_Double_ForLoop, Tb17, MyByteAry);
            Button18.Click += (s, e) => MyExe(Test11_Double_ParallelFor, Tb18, MyByteAry);

            Button19.Click += (s, e) => MyExe(Test11_Double_ParallelForEach, Tb19, MyByteAry);
            Button20.Click += (s, e) => MyExe(Test04MT_Double_ParallelForEach_VectorDot, Tb20, MyByteAry);
            Button21.Click += (s, e) => MyExe(Test04MT_Double_ParallelForEach_VectorWidenDot, Tb21, MyByteAry);
            Button22.Click += (s, e) => MyExe(Test04MT_Integer_ParallelForEach_VectorDot, Tb22, MyByteAry);
            //Button23.Click += (s, e) => MyExe(Test04MT_Double_ParallelForEach_VectorDot, Tb23, MyByteAry);
            //Button24.Click += (s, e) => MyExe(Test04MT_Double_ParallelForEach_VectorDot, Tb24, MyByteAry);
            //Button25.Click += (s, e) => MyExe(Test04MT_Double_ParallelForEach_VectorDot, Tb25, MyByteAry);
        }

        private void MyInitialize()
        {
            MyByteAry = new byte[ELEMENT_COUNT];
            var r = new Random();
            r.NextBytes(MyByteAry);
            //要素の平均値
            //MyByteAry = new byte[] { 20, 21, 7, 12 };
            //Span<byte> span = new Span<byte>(MyByteAry);
            //span.Fill(1);
            //MyByteAry = span.ToArray();
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
        //Parallel.For、すべてdouble型で計算
        private double Test01MT_double_ParallelFor(byte[] ary)
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

        //Parallel.For、整数で計算
        private double Test02MT_Integer_ParallelFor(byte[] ary)
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
                (x) => Interlocked.Add(ref total, x));//小計を合計、InterlockedAddは整数しか扱えない

            //合計 / 要素数 = 分散
            return (double)total / ary.Length;
        }


        //        c# — C＃で整数の配列を合計する方法
        //https://www.it-swarm.dev/ja/c%23/c%EF%BC%83%E3%81%A7%E6%95%B4%E6%95%B0%E3%81%AE%E9%85%8D%E5%88%97%E3%82%92%E5%90%88%E8%A8%88%E3%81%99%E3%82%8B%E6%96%B9%E6%B3%95/968057375/
        //Paralle.ForEach、double型
        private double Test021MT_double_ParallelForEach(byte[] ary)
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
                        subtotal += Math.Pow(ary[i] - MyAverage, 2.0);
                    }
                    myBag.Add(subtotal);//小計を追加
                });
            double total = myBag.Sum();//合計
            //合計 / 要素数 = 分散
            return total / ary.Length;
        }
        //Paralle.ForEach、整数型で計算
        private double Test022MT_Integer_ParallelForEach(byte[] ary)
        {
            //平均との差(偏差)の2乗を合計
            long total = 0;
            int average = (int)MyAverage;
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
            //配列とVectorCountの剰余分も加算
            total += MySuquareSumOfDeviation偏差の2乗和(ary, lastIndex, ary.Length, MyAverage);
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
                    //int surplus = (range.Item2 - range.Item1) % simdLength;//余り、剰余
                    int lastIndex=range.Item2 - (range.Item2 - range.Item1) % simdLength;
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
                    //排他処理でスレッドごとの小計を加算、InterlockedAddは整数しか扱えない
                    Interlocked.Add(ref total, (long)subtotal);
                });

            return (double)total / ary.Length;
        }

        //Vector<double>で計算、スレッドごとの集計もdouble型
        //ConcurentBagならdouble型も扱える
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
                    //int surplus = (range.Item2 - range.Item1) % simdLength;//余り、剰余
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
            double total = bag.Sum();
            return total / ary.Length;
        }

        //Vector<int>で計算、小計、合計はlongで計算、intだと小計でもオーバーフローした
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
                    MyVariance(range, simdLength, ary, average, ref total);
                    //int subtotal = 0;
                    //int surplus = (range.Item2 - range.Item1) % simdLength;//余り、剰余
                    //int[] aa = new int[simdLength];
                    //Vector<int> v;
                    //for (int i = range.Item1; i < range.Item2; i += simdLength)
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
                    //    subtotal += (int)MySuquareSum(ary, range.Item2 - surplus, range.Item2, MyAverage);
                    //}
                    ////排他処理でスレッドごとの小計を加算、InterlockedAddは整数しか扱えない
                    //Interlocked.Add(ref total, subtotal);
                });

            return (double)total / ary.Length;
        }
        private void MyVariance(Tuple<int, int> range, int simdLength, byte[] ary, int average, ref long total)
        {
            long subtotal = 0;
            //int surplus = (range.Item2 - range.Item1) % simdLength;//余り、剰余
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

        //Vector<float>で計算は小数点有り
        //集計はInterlockedなので整数
        private double Test06MT_floatIntVectorDot(byte[] ary)
        {
            int simdLength = Vector<float>.Count;
            long total = 0;
            float average = (float)MyAverage;
            //パーティションサイズ、要素数をCPUスレッド数で割った数値
            int rangeSize = ary.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, ary.Length, rangeSize),
                (range) =>
                {
                    double subtotal = 0;
                    //int surplus = (range.Item2 - range.Item1) % simdLength;//余り、剰余
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    float[] aa = new float[simdLength];
                    Vector<float> v;
                    for (int i = range.Item1; i < lastIndex; i += simdLength)
                    {
                        //偏差の配列作成
                        for (int j = 0; j < simdLength; j++)
                        {
                            aa[j] = ary[i + j] - average;//偏差
                        }
                        //偏差の配列からVector作成
                        v = new Vector<float>(aa);
                        subtotal += System.Numerics.Vector.Dot(v, v);
                        //VectorCountで割り切れなかった余りの2乗和も加算
                        subtotal += MySuquareSumOfDeviation偏差の2乗和(ary, lastIndex, range.Item2, MyAverage);
                    }
                    //排他処理でスレッドごとの小計を加算、InterlockedAddは整数しか扱えない
                    Interlocked.Add(ref total, (long)subtotal);
                });

            return (double)total / ary.Length;
        }
        //Vector<float>で計算、小計はdouble、合計もdouble
        //ConcurentBagで排他処理合計
        private double Test06MT_floatfloatVectorDot(byte[] ary)
        {
            int simdLength = Vector<float>.Count;
            var bag = new ConcurrentBag<double>();
            float average = (float)MyAverage;
            //パーティションサイズ、要素数をCPUスレッド数で割った数値
            int rangeSize = ary.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, ary.Length, rangeSize),
                (range) =>
                {
                    double subtotal = 0;
                    //int surplus = (range.Item2 - range.Item1) % simdLength;//余り、剰余
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    float[] aa = new float[simdLength];
                    Vector<float> v;
                    for (int i = range.Item1; i < lastIndex; i += simdLength)
                    {
                        //偏差の配列作成
                        for (int j = 0; j < simdLength; j++)
                        {
                            aa[j] = ary[i + j] - average;//偏差
                        }
                        //偏差の配列からVector作成
                        v = new Vector<float>(aa);
                        subtotal += System.Numerics.Vector.Dot(v, v);
                        //VectorCountで割り切れなかった余りの2乗和も加算
                        subtotal += MySuquareSumOfDeviation偏差の2乗和(ary, lastIndex, range.Item2, MyAverage);
                    }
                    //排他処理でスレッドごとの小計をコレクションに追加
                    bag.Add(subtotal);
                });
            double total = bag.Sum();
            return total / ary.Length;
        }

        //Vector4
        private double Test07MT_Vector4SubtractDot(byte[] ary)
        {
            int simdLength = 4;
            var bag = new ConcurrentBag<double>();
            var vAverage = new Vector4((float)MyAverage);
            //パーティションサイズ、要素数をCPUスレッド数で割った数値
            int rangeSize = ary.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, ary.Length, rangeSize),
                (range) =>
                {
                    double subtotal = 0;
                    //int surplus = (range.Item2 - range.Item1) % simdLength;//余り、剰余
                    int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                    float[] aa = new float[simdLength];
                    Vector4 v;
                    for (int i = range.Item1; i < lastIndex; i += simdLength)
                    {
                        v = new Vector4(ary[i], ary[i + 1], ary[i + 2], ary[i + 3]);
                        v = Vector4.Subtract(v, vAverage);//偏差
                        subtotal += Vector4.Dot(v, v);//ドット積
                        //VectorCountで割り切れなかった余りのドット積も加算
                        subtotal += MySuquareSumOfDeviation偏差の2乗和(ary, lastIndex, range.Item2, MyAverage);
                    }
                    //排他処理でスレッドごとの小計をコレクションに追加
                    bag.Add(subtotal);
                });
            double total = bag.Sum();
            return total / ary.Length;
        }













        //指定インデックスから最後までの偏差の2乗和を返す
        //VectorCountで割り切れなかった余り用
        private double MySuquareSumOfDeviation偏差の2乗和(byte[] ary, int beginIndex, int endIndex, double average)
        {
            double total = 0;
            for (int i = beginIndex; i < endIndex; i++)
            {
                total += Math.Pow(ary[i] - average, 2.0);
            }
            return total;
        }

        /// <summary>
        /// byte型配列用、 VectorCountで割り切れなかった余りの要素の2乗和を返す、要素数10でVectorCountが4のとき10%4=2なので、最後の2つの要素が対象になる
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
            int total = 0;
            for (int i = lastIndex; i < endIndex; i++)
            {
                total += ary[i] * ary[i];
            }
            return total;
        }







        #endregion 分散の求め方その1ここまで


        #region 分散の求め方その2ここから
        //シングルスレッド、double
        private double Test11_Double_ForLoop(byte[] ary)
        {
            //2乗和
            double total = 0;
            for (int i = 0; i < ary.Length; i++)
            {
                total += ary[i] * ary[i];// Math.Pow(ary[i], 2.0);
            }
            //2乗の平均
            total /= ary.Length;
            //2乗の平均 - 平均の2乗
            return total - Math.Pow(MyAverage, 2.0);
        }
        //シングルスレッド、int
        private double Test12_Integer_ForLoop(byte[] ary)
        {

            double total = 0;
            int ii;
            for (int i = 0; i < ary.Length; i++)
            {
                ii = ary[i];
                total += ii * ii;
            }
            total /= ary.Length;

            return total - (MyAverage * MyAverage);
        }


        //マルチスレッドここから
        private double Test11_Double_ParallelFor(byte[] ary)
        {
            //2乗和            
            var myBag = new ConcurrentBag<long>();
            Parallel.For<long>(0, ary.Length,
                () => 0,
                (j, state, subtotal) => { return subtotal += ary[j] * ary[j]; },
                (x) => myBag.Add(x));
            long total = myBag.Sum();
            return ((double)total / ary.Length) - Math.Pow(MyAverage, 2.0);//2乗の平均 - 平均の2乗
        }

        private double Test11_Double_ParallelForEach(byte[] ary)
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
            return ((double)total / ary.Length) - Math.Pow(MyAverage, 2.0);//2乗の平均 - 平均の2乗
        }

        //ここからVector
        //Vector<double>で計算、byte型の2乗和を求めるからdouble型はあんまり意味ない
        private double Test04MT_Double_ParallelForEach_VectorDot(byte[] ary)
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
        private double Test04MT_Integer_ParallelForEach_VectorDot(byte[] ary)
        {
            var myBag = new ConcurrentBag<long>();
            var partition = Partitioner.Create(0, ary.Length, ary.Length / Environment.ProcessorCount);
            int simdLength = Vector<int>.Count;
            Parallel.ForEach(partition, (range) =>
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
        //byteやushortだとオーバーフローするのでWidenでuintまで拡大してからドット積
        private double Test04MT_Double_ParallelForEach_VectorWidenDot(byte[] ary)
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






        #endregion 分散の求め方その2ここまで



















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
