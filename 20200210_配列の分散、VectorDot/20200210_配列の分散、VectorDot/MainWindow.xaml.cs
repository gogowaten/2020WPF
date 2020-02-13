using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Collections.Concurrent;

namespace _20200210_配列の分散_VectorDot
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
            MyTextBlockCpuThreadCount.Text =$"CPUスレッド数：{Environment.ProcessorCount.ToString()} thread";
            //MyIntAry = Enumerable.Range(1, ELEMENT_COUNT).ToArray();//連番値
            //MyIntAry = Enumerable.Repeat(1, ELEMENT_COUNT).ToArray();//全値1
            //MyLongAry = new long[MyIntAry.Length];//long型配列作成
            //MyIntAry.CopyTo(MyLongAry, 0);
            MyInitialize();


            Button1.Click += (s, e) => MyExe(Test01_Double_ForLoop, Tb1, MyByteAry);
            Button2.Click += (s, e) => MyExe(Test02_Integer_ForLoop, Tb2, MyByteAry);
            Button3.Click += (s, e) => MyExe(Test03_FloatVectorSubtractDot, Tb3, MyByteAry);
            Button4.Click += (s, e) => MyExe(Test04_DoubleVectorSubtractDot, Tb4, MyByteAry);
            Button5.Click += (s, e) => MyExe(Test05_IntegerVectorSubtractDot, Tb5, MyByteAry);
            Button6.Click += (s, e) => MyExe(Test06_ByteVectorSubtractDot_Overflow, Tb6, MyByteAry);
            Button7.Click += (s, e) => MyExe(Test07_ShortVectorSubtractDot_Overflow, Tb7, MyByteAry);
            Button8.Click += (s, e) => MyExe(Test08_FloatVector4, Tb8, MyByteAry);
            Button9.Click += (s, e) => MyExe(Test09_IntegerVectorDot, Tb9, MyByteAry);
            Button10.Click += (s, e) => MyExe(Test10_DoubleVectorDot, Tb10, MyByteAry);

            Button11.Click += (s, e) => MyExe(Test11_Double_ForLoop, Tb11, MyByteAry);
            Button12.Click += (s, e) => MyExe(Test12_Integer_ForLoop, Tb12, MyByteAry);
            Button13.Click += (s, e) => MyExe(Test13_FloatVectorDot, Tb13, MyByteAry);
            Button14.Click += (s, e) => MyExe(Test14_DoubleVectorDot, Tb14, MyByteAry);
            Button15.Click += (s, e) => MyExe(Test15_IntegerVectorDot, Tb15, MyByteAry);
            Button16.Click += (s, e) => MyExe(Test16_ByteVectorDot_Overflow, Tb16, MyByteAry);
            Button17.Click += (s, e) => MyExe(Test17_ShortVectorDot_Overflow, Tb17, MyByteAry);
            Button18.Click += (s, e) => MyExe(Test18_FloatVector4, Tb18, MyByteAry);

            Button19.Click += (s, e) => MyExe(Test19_Byte_ushort_uintVectorDot, Tb19, MyByteAry);
            Button20.Click += (s, e) => MyExe(Test20_Byte_ushort_uintVectorDot, Tb20, MyByteAry);
        }
        private void MyInitialize()
        {
            MyByteAry = new byte[ELEMENT_COUNT];
            var r = new Random();
            r.NextBytes(MyByteAry);
            //要素の平均値
            //MyByteAry = new byte[] { 0, 21, 7, 255 };
            MyAverage = GetAverage(MyByteAry);
        }

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

        //Vector<float>で計算
        private double Test03_FloatVectorSubtractDot(byte[] ary)
        {
            var vAverage = new Vector<float>((float)MyAverage);
            int simdLength = Vector<float>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<float> v;
            var ss = new float[simdLength];
            double total = 0;
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                for (int j = 0; j < simdLength; j++)
                {
                    ss[j] = ary[i + j];
                }
                //平均との差
                v = System.Numerics.Vector.Subtract(vAverage, new Vector<float>(ss));
                //差の2乗を合計
                total += System.Numerics.Vector.Dot(v, v);
            }
            return total / ary.Length;
        }

        //Vector<double>で計算
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

        //Vector<int>で計算
        private double Test05_IntegerVectorSubtractDot(byte[] ary)
        {
            var vAverage = new Vector<int>((int)MyAverage);
            int simdLength = Vector<int>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<int> v;
            var ss = new int[simdLength];
            double total = 0;
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                for (int j = 0; j < simdLength; j++)
                {
                    ss[j] = ary[i + j];
                }
                //平均との差
                v = System.Numerics.Vector.Subtract(vAverage, new Vector<int>(ss));
                //差の2乗を合計
                total += System.Numerics.Vector.Dot(v, v);
            }
            return total / ary.Length;
        }

        //Vector<byte>で計算、ドット積でオーバーフロー
        private double Test06_ByteVectorSubtractDot_Overflow(byte[] ary)
        {
            var vAverage = new Vector<byte>((byte)MyAverage);
            int simdLength = Vector<byte>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<byte> v;
            double total = 0;

            for (int i = 0; i < lastIndex; i += simdLength)
            {
                //平均との差は、byte型にはマイナスがないから間違った値になる
                v = System.Numerics.Vector.Subtract(vAverage, new Vector<byte>(ary, i));
                //差の2乗を合計
                total += System.Numerics.Vector.Dot(v, v);//オーバーフロー
            }
            return total / ary.Length;
        }

        //Vector<short>で計算はドット積でオーバーフロー
        private double Test07_ShortVectorSubtractDot_Overflow(byte[] ary)
        {
            var vAverage = new Vector<short>((short)MyAverage);
            int simdLength = Vector<short>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<short> v;
            long total = 0;
            var ss = new short[simdLength];
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                for (int j = 0; j < simdLength; j++)
                {
                    ss[j] = ary[i + j];
                }
                //平均との差
                v = System.Numerics.Vector.Subtract(vAverage, new Vector<short>(ss));
                //差の2乗を合計
                total += System.Numerics.Vector.Dot(v, v);//オーバーフロー
            }
            return (double)total / ary.Length;
        }

        //Vector4
        private double Test08_FloatVector4(byte[] ary)
        {
            int lastIndex = ary.Length - (ary.Length % 4);
            var vAverage = new Vector4((float)MyAverage);
            Vector4 v;//= new Vector4();
            double total = 0;
            for (int i = 0; i < lastIndex; i += 4)
            {
                //平均との差
                v = Vector4.Subtract(new Vector4(ary[i], ary[i + 1], ary[i + 2], ary[i + 3]), vAverage);
                //差の2乗を合計
                total += Vector4.Dot(v, v);
            }
            return total / ary.Length;
        }

        //平均との差は普通のループで、int型
        private double Test09_IntegerVectorDot(byte[] ary)
        {
            int average = (int)MyAverage;
            int simdLength = Vector<int>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<int> v;
            long total = 0;
            int[] ii = new int[simdLength];
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                //平均との差を配列に入れる
                for (int j = 0; j < simdLength; j++)
                {
                    ii[j] = average - ary[i + j];
                }
                //差の配列からVector作成して2乗和を合計していく
                v = new Vector<int>(ii);
                total += System.Numerics.Vector.Dot(v, v);
            }
            return total / (double)ary.Length;
        }

        //平均との差は普通のループで、double型
        private double Test10_DoubleVectorDot(byte[] ary)
        {
            int simdLength = Vector<double>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<double> v;
            double total = 0;
            double[] ii = new double[simdLength];
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                //平均との差の配列
                for (int j = 0; j < simdLength; j++)
                {
                    ii[j] = MyAverage - ary[i + j];
                }
                //差の配列からVector作成してドット積
                v = new Vector<double>(ii);
                total += System.Numerics.Vector.Dot(v, v);
            }
            return total / ary.Length;
        }





        #region 分散 = 2乗和の平均 - 平均の2乗
        //分散の意味と求め方、分散公式の使い方
        //https://sci-pursuit.com/math/statistics/variance.html



        private double Test11_Double_ForLoop(byte[] ary)
        {
            //2乗和
            double total = 0;
            for (int i = 0; i < ary.Length; i++)
            {
                total += Math.Pow(ary[i], 2.0);
            }
            //2乗和の平均
            total /= ary.Length;
            //2乗和の平均 - 平均の2乗
            return total - Math.Pow(MyAverage, 2.0);
        }

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

        //float
        private double Test13_FloatVectorDot(byte[] ary)
        {
            int simdLength = Vector<float>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<float> v;
            var ss = new float[simdLength];
            double total = 0;
            for (int i = 0; i < lastIndex; i += simdLength)
            //配列を作成してVector作成してドット積
            {
                for (int j = 0; j < simdLength; j++)
                {
                    ss[j] = ary[i + j];
                }
                v = new Vector<float>(ss);
                total += System.Numerics.Vector.Dot(v, v);
            }
            //2乗和の平均 - 平均の2乗
            return (total / ary.Length) - (MyAverage * MyAverage);
        }


        //Vector<double>で計算
        private double Test14_DoubleVectorDot(byte[] ary)
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
            return (total / ary.Length) - (MyAverage * MyAverage);
        }

        //Vector<int>で計算
        private double Test15_IntegerVectorDot(byte[] ary)
        {
            int simdLength = Vector<int>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<int> v;
            var ss = new int[simdLength];
            double total = 0;
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                for (int j = 0; j < simdLength; j++)
                {
                    ss[j] = ary[i + j];
                }
                v = new Vector<int>(ss);
                total += System.Numerics.Vector.Dot(v, v);
            }
            return (total / ary.Length) - (MyAverage * MyAverage);
        }

        //Vector<byte>で計算、ドット積でオーバーフロー
        private double Test16_ByteVectorDot_Overflow(byte[] ary)
        {
            int simdLength = Vector<byte>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<byte> v;
            double total = 0;

            for (int i = 0; i < lastIndex; i += simdLength)
            {
                v = new Vector<byte>(ary, i);
                total += System.Numerics.Vector.Dot(v, v);//オーバーフロー
            }
            return (total / ary.Length) - (MyAverage * MyAverage);
        }

        //Vector<short>で計算はドット積でオーバーフロー
        private double Test17_ShortVectorDot_Overflow(byte[] ary)
        {
            int simdLength = Vector<short>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<short> v;
            long total = 0;
            var ss = new short[simdLength];
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                for (int j = 0; j < simdLength; j++)
                {
                    ss[j] = ary[i + j];
                }
                v = new Vector<short>(ss);
                total += System.Numerics.Vector.Dot(v, v);//オーバーフロー
            }
            return (total / ary.Length) - (MyAverage * MyAverage);
        }

        //Vector4
        private double Test18_FloatVector4(byte[] ary)
        {
            int lastIndex = ary.Length - (ary.Length % 4);;
            Vector4 v;
            double total = 0;
            for (int i = 0; i < lastIndex; i += 4)
            {
                v = new Vector4(ary[i], ary[i + 1], ary[i + 2], ary[i + 3]);
                total += Vector4.Dot(v, v);
            }
            return (total / ary.Length) - (MyAverage * MyAverage);
        }



        //Vector<byte>をWidenでVector<ushort>にしてドット積計算はオーバーフローだったので
        //Vector<ushort>からさらにVector<uint>にしてドット積
        private double Test19_Byte_ushort_uintVectorDot(byte[] ary)
        {
            int simdLength = Vector<byte>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<byte> v;
            double total = 0;
            Vector<ushort> v1; Vector<ushort> v2;
            Vector<uint> vv1; Vector<uint> vv2; Vector<uint> vv3; Vector<uint> vv4;
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                v = new Vector<byte>(ary, i);
                System.Numerics.Vector.Widen(v, out v1, out v2);
                System.Numerics.Vector.Widen(v1, out vv1, out vv2);
                System.Numerics.Vector.Widen(v2, out vv3, out vv4);
                total += System.Numerics.Vector.Dot(vv1, vv1);
                total += System.Numerics.Vector.Dot(vv2, vv2);
                total += System.Numerics.Vector.Dot(vv3, vv3);
                total += System.Numerics.Vector.Dot(vv4, vv4);
            }
            return (total / ary.Length) - (MyAverage * MyAverage);
        }

        //↑と同じ、インライン化しただけ
        private double Test20_Byte_ushort_uintVectorDot(byte[] ary)
        {
            int simdLength = Vector<byte>.Count;
            int lastIndex = ary.Length - (ary.Length % simdLength);
            Vector<byte> v;
            double total = 0;
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                v = new Vector<byte>(ary, i);
                System.Numerics.Vector.Widen(v, out Vector<ushort> v1, out Vector<ushort> v2);
                System.Numerics.Vector.Widen(v1, out Vector<uint> vv1, out Vector<uint> vv2);
                System.Numerics.Vector.Widen(v2, out Vector<uint> vv3, out Vector<uint> vv4);
                total += System.Numerics.Vector.Dot(vv1, vv1);
                total += System.Numerics.Vector.Dot(vv2, vv2);
                total += System.Numerics.Vector.Dot(vv3, vv3);
                total += System.Numerics.Vector.Dot(vv4, vv4);
            }
            return (total / ary.Length) - (MyAverage * MyAverage);
        }


        #endregion









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
            tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  分散 = {total.ToString("F4")}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
        }



    }

}
