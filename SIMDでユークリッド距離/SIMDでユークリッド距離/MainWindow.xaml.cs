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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SIMDでユークリッド距離
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyR;
        private byte[] MyG;
        private byte[] MyB;
        private byte[] MyZ;
        private double[] MyDoubleArray;
        private float[] MyFloatArray;

        private const int LOOP_COUNT = 10;
        private const int ELEMENT_COUNT = 10_000_000;// 1_056_831;// 132_103;// 2071;//要素数

        public MainWindow()
        {
            InitializeComponent();

            MyInitialize();
            this.Title = this.ToString();


            MyTextBlock.Text = $"byte型配列要素数{ELEMENT_COUNT.ToString("N0")}のユークリッド距離を {LOOP_COUNT}回求める";
            MyTextBlockVectorCount.Text = $"Vector256<byte>.Count={Vector256<byte>.Count}  Vector<byte>.Count={Vector<byte>.Count}";
            MyTextBlockCpuThreadCount.Text = $"CPUスレッド数：{Environment.ProcessorCount}";



            ButtonAll.Click += (s, e) => MyExeAll();
            Button1.Click += (s, e) => MyExe(Test10, Tb1, MyR, MyG, MyB, MyDoubleArray);
            Button2.Click += (s, e) => MyExe(Test11_MTPF, Tb2, MyR, MyG, MyB, MyDoubleArray);
            Button3.Click += (s, e) => MyExe(Test12_MTPFE, Tb3, MyR, MyG, MyB, MyDoubleArray);
            Button4.Click += (s, e) => MyExe(Test13_MTPFE, Tb4, MyR, MyG, MyB, MyDoubleArray);
            Button5.Click += (s, e) => MyExe(Test20_V3_Distance, Tb5, MyR, MyG, MyB, MyFloatArray);
            Button6.Click += (s, e) => MyExe(Test99_V3_DistanceSquared, Tb6, MyR, MyG, MyB, MyFloatArray);
            Button7.Click += (s, e) => MyExe(Test21_V3_Distance_MTPF, Tb7, MyR, MyG, MyB, MyFloatArray);
            Button8.Click += (s, e) => MyExe(Test22_V3_Distance_MTPFE, Tb8, MyR, MyG, MyB, MyFloatArray);
            Button9.Click += (s, e) => MyExe(Test23_V3_Distance_MTPFE, Tb9, MyR, MyG, MyB, MyFloatArray);
            Button10.Click += (s, e) => MyExe(Test30_V3_Distance, Tb10, MyR, MyG, MyB, MyFloatArray);
            //Button11.Click += (s, e) => MyExe(Test11_Normal_MT, Tb11, MyArray);
            //Button12.Click += (s, e) => MyExe(Test12_Numerics_Dot_long_MT, Tb12, MyArray);
            //Button13.Click += (s, e) => MyExe(Test13_Intrinsics_FMA_MultiplyAdd_float_MT, Tb13, MyArray);
            //Button14.Click += (s, e) => MyExe(Test14_Intrinsics_FMA_MultiplyAdd_double_MT, Tb14, MyArray);
            //Button15.Click += (s, e) => MyExe(Test15_Intrinsics_AVX_Multiply_Add_long_MT, Tb15, MyArray);
            //Button16.Click += (s, e) => MyExe(Test16_Intrinsics_SSE2_MultiplyAddAdjacent_int_MT, Tb16, MyArray);
            //Button17.Click += (s, e) => MyExe(Test17_Intrinsics_SSE41_DotProduct_float_MT, Tb17, MyArray);
            //Button18.Click += (s, e) => MyExe(Test18_Intrinsics_SSE41_DotProduct_float_MT, Tb18, MyArray);
            ////Button19.Click += (s, e) => MyExe(Test19_Intrinsics_SSE41_DotProduct_float_MT, Tb19, MyArray);
            ////Button20.Click += (s, e) => MyExe(Test20, Tb20, MyArray);
            //Button21.Click += (s, e) => MyExe(Test23_Intrinsics_FMA_MultiplyAdd_float_MT_Kai, Tb21, MyArray);
            //Button22.Click += (s, e) => MyExe(Test26_Intrinsics_SSE2_MultiplyAddAdjacent_int_MT_Kai, Tb22, MyArray);
            //Button23.Click += (s, e) => MyExe(Test28_Intrinsics_SSE41_DotProduct_float_MT_Kai, Tb23, MyArray);
            //Button24.Click += (s, e) => MyExe(Test28_Intrinsics_SSE41_DotProduct_float_MT_Kai, Tb24, MyArray);
        }


        private void Test10(byte[] red, byte[] green, byte[] blue, double[] vv)
        {
            for (int i = 0; i < red.Length; i++)
            {
                vv[i] = Math.Sqrt(
                    ((0 - red[i]) * (0 - red[i])) +
                    ((0 - green[i]) * (0 - green[i])) +
                    ((0 - blue[i]) * (0 - blue[i])));
            }
        }

        private void Test11_MTPF(byte[] red, byte[] green, byte[] blue, double[] vv)
        {
            Parallel.For(0, red.Length, i =>
            {
                vv[i] = Math.Sqrt(
                    ((0 - red[i]) * (0 - red[i])) +
                    ((0 - green[i]) * (0 - green[i])) +
                    ((0 - blue[i]) * (0 - blue[i])));
            });
        }

        private void Test12_MTPFE(byte[] red, byte[] green, byte[] blue, double[] vv)
        {
            int rangeSize = red.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, red.Length, rangeSize),
                (range) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        vv[i] = Math.Sqrt(
                    ((0 - red[i]) * (0 - red[i])) +
                    ((0 - green[i]) * (0 - green[i])) +
                    ((0 - blue[i]) * (0 - blue[i])));
                    }
                });
        }

        private void Test13_MTPFE(byte[] red, byte[] green, byte[] blue, double[] vv)
        {
            Parallel.ForEach(Partitioner.Create(0, red.Length),
                (range) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        vv[i] = Math.Sqrt(
                     ((0 - red[i]) * (0 - red[i])) +
                     ((0 - green[i]) * (0 - green[i])) +
                     ((0 - blue[i]) * (0 - blue[i])));
                    }
                });
        }

        //Vector3のDistance
        private void Test20_V3_Distance(byte[] red, byte[] green, byte[] blue, float[] vv)
        {
            for (int i = 0; i < red.Length; i++)
            {
                vv[i] = Vector3.Distance(
                    new Vector3(0, 0, 0),
                    new Vector3(red[i], green[i], blue[i]));
            }
        }

        //ドット積？
        private void Test99_V3_DistanceSquared(byte[] red, byte[] green, byte[] blue, float[] vv)
        {
            for (int i = 0; i < red.Length; i++)
            {
                vv[i] = Vector3.DistanceSquared(
                    new Vector3(0, 0, 0),
                    new Vector3(red[i], green[i], blue[i]));
            }
        }

        private void Test21_V3_Distance_MTPF(byte[] red, byte[] green, byte[] blue, float[] vv)
        {
            Parallel.For(0, red.Length, i =>
            {
                vv[i] = Vector3.Distance(
                    new Vector3(0, 0, 0),
                    new Vector3(red[i], green[i], blue[i]));
            });
        }


        private void Test22_V3_Distance_MTPFE(byte[] red, byte[] green, byte[] blue, float[] vv)
        {
            Parallel.ForEach(Partitioner.Create(0, red.Length),
                (range) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        vv[i] = Vector3.Distance(
                            new Vector3(0, 0, 0),
                            new Vector3(red[i], green[i], blue[i]));
                    }
                });
        }

        private void Test23_V3_Distance_MTPFE(byte[] red, byte[] green, byte[] blue, float[] vv)
        {
            int rangeSize = red.Length / Environment.ProcessorCount;
            Parallel.ForEach(Partitioner.Create(0, red.Length, rangeSize),
                (range) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        vv[i] = Vector3.Distance(
                            new Vector3(0, 0, 0),
                            new Vector3(red[i], green[i], blue[i]));
                    }
                });
        }


        private void Test30_V3_Distance(byte[] red, byte[] green, byte[] blue, float[] vv)
        {
            int simdLength = Vector<float>.Count;
            int lastIndex = red.Length - (red.Length % simdLength);
            var ii = new float[simdLength];
            int p;
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                for (int k = 0; k < simdLength; k++)
                {
                    p = k + i;
                    ii[k] = (0 - red[p]) * (0 - red[p]) + (0 - green[p]) * (0 - green[p]) + (0 - blue[p]) * (0 - blue[p]);
                }
                var v = System.Numerics.Vector.SquareRoot(new Vector<float>(ii));
                for (int m = 0; m < simdLength; m++)
                {
                    vv[i + m] = v[m];
                }
            }
            Amari(lastIndex, red, green, blue, vv);
        }

        private void Amari(int lastIndex, byte[] red, byte[] green, byte[] blue, float[] vv)
        {
            for (int i = lastIndex; i < red.Length; i++)
            {
                vv[i] = (float)Math.Sqrt(
                    ((0 - red[i]) * (0 - red[i])) +
                    ((0 - green[i]) * (0 - green[i])) +
                    ((0 - blue[i]) * (0 - blue[i])));
            }
        }






























        private void MyInitialize()
        {
            MyR = new byte[ELEMENT_COUNT];
            MyG = new byte[ELEMENT_COUNT];
            MyB = new byte[ELEMENT_COUNT];
            MyZ = new byte[ELEMENT_COUNT];
            MyDoubleArray = new double[ELEMENT_COUNT];
            MyFloatArray = new float[ELEMENT_COUNT];


            ////指定値で埋める
            ////var span = new Span<byte>(MyArray);
            ////span.Fill(255);
            Span<byte> span;
            //span = new Span<byte>(MyArray1);//白
            //span.Fill(255);
            span = new Span<byte>(MyZ);
            span.Fill(0);
            //span = new Span<byte>(MyG);
            //span.Fill(0);
            //span = new Span<byte>(MyB);
            //span.Fill(0);




            //最後の要素
            //MyArray[ELEMENT_COUNT - 1] = 100;

            //ランダム値
            //var r = new Random();
            //r.NextBytes(MyArray);

            //0～255までを連番で繰り返し
            //for (int i = 0; i < ELEMENT_COUNT; i++)
            //{
            //    MyArray[i] = (byte)i;
            //}
            for (int i = 0; i < ELEMENT_COUNT; i++)
            {
                MyR[i] = (byte)i;
                MyG[i] = (byte)i;
                MyB[i] = (byte)i;
            }

        }

        #region 時間計測
        //private void MyExe(Action<byte[], byte[]> act, TextBlock tb, byte[] vs1, byte[] vs2)
        //{
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    for (int i = 0; i < LOOP_COUNT; i++)
        //    {
        //        act(vs1, vs2);
        //    }
        //    sw.Stop();
        //    this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒   {act.Method.Name}");
        //}

        private void MyExe(Action<byte[], byte[], byte[], double[]> act, TextBlock tb, byte[] red, byte[] green, byte[] blue, double[] vv)
        {
            var span = new Span<double>(vv);
            span.Fill(0);

            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                act(red, green, blue, vv);
            }
            sw.Stop();
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒 {vv[1]}  {act.Method.Name}");
        }
        private void MyExe(Action<byte[], byte[], byte[], float[]> act, TextBlock tb, byte[] red, byte[] green, byte[] blue, float[] vv)
        {
            var span = new Span<float>(vv);
            span.Fill(0);

            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                act(red, green, blue, vv);
            }
            sw.Stop();
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒 {vv[1]}  {act.Method.Name}");
        }

        //private void MyExe(Func<byte[], long> func, TextBlock tb, byte[] vs)
        //{
        //    long total = 0;
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    for (int i = 0; i < LOOP_COUNT; i++)
        //    {
        //        total = func(vs);
        //    }
        //    sw.Stop();
        //    this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒 {total.ToString("N0")}  {func.Method.Name}");
        //}


        //一斉テスト用
        private async void MyExeAll()
        {
            var sw = new Stopwatch();
            sw.Start();
            this.IsEnabled = false;
            //await Task.Run(() => MyExe(Test1_Normal, Tb1, MyArray));
            //await Task.Run(() => MyExe(Test2_Numerics_Dot_long, Tb2, MyArray));
            //await Task.Run(() => MyExe(Test3_Intrinsics_FMA_MultiplyAdd_float, Tb3, MyArray));
            //await Task.Run(() => MyExe(Test4_Intrinsics_FMA_MultiplyAdd_double, Tb4, MyArray));
            //await Task.Run(() => MyExe(Test5_Intrinsics_AVX_Multiply_Add_long, Tb5, MyArray));
            //await Task.Run(() => MyExe(Test6_Intrinsics_SSE2_MultiplyAddAdjacent_int, Tb6, MyArray));
            //await Task.Run(() => MyExe(Test7_Intrinsics_SSE41_DotProduct_float, Tb7, MyArray));
            //await Task.Run(() => MyExe(Test8_Intrinsics_SSE41_DotProduct_float, Tb8, MyArray));
            ////await Task.Run(() => MyExe(Test9_Nunerics_uint_MT, Tb9, MyArray));
            ////await Task.Run(() => MyExe(Test11_Normal_MT, Tb10, MyArray));
            //await Task.Run(() => MyExe(Test11_Normal_MT, Tb11, MyArray));
            //await Task.Run(() => MyExe(Test12_Numerics_Dot_long_MT, Tb12, MyArray));
            //await Task.Run(() => MyExe(Test13_Intrinsics_FMA_MultiplyAdd_float_MT, Tb13, MyArray));
            //await Task.Run(() => MyExe(Test14_Intrinsics_FMA_MultiplyAdd_double_MT, Tb14, MyArray));
            //await Task.Run(() => MyExe(Test15_Intrinsics_AVX_Multiply_Add_long_MT, Tb15, MyArray));
            //await Task.Run(() => MyExe(Test16_Intrinsics_SSE2_MultiplyAddAdjacent_int_MT, Tb16, MyArray));
            //await Task.Run(() => MyExe(Test17_Intrinsics_SSE41_DotProduct_float_MT, Tb17, MyArray));
            //await Task.Run(() => MyExe(Test18_Intrinsics_SSE41_DotProduct_float_MT, Tb18, MyArray));


            //await Task.Run(() => MyExe(Test23_Intrinsics_FMA_MultiplyAdd_float_MT_Kai, Tb21, MyArray));
            //await Task.Run(() => MyExe(Test26_Intrinsics_SSE2_MultiplyAddAdjacent_int_MT_Kai, Tb22, MyArray));
            //await Task.Run(() => MyExe(Test28_Intrinsics_SSE41_DotProduct_float_MT_Kai, Tb23, MyArray));


            this.IsEnabled = true;
            sw.Stop();
            TbAll.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒";
        }
        #endregion 時間計測
    }
}
