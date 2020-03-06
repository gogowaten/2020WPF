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

//MathのSqrtとIntrinsicsのSqrt、double型で計算
//Intrinsicsのほうが3～4倍速い

namespace _20200306_SIMD平方根
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyArray;
        private const int LOOP_COUNT = 10;
        private const int ELEMENT_COUNT = 10_000_000;// 1_056_831;// 132_103;// 2071;//要素数

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
            this.Title = this.ToString();



            MyTextBlock.Text = $"byte型配列要素数{ELEMENT_COUNT.ToString("N0")}のドット積を {LOOP_COUNT}回求める";
            MyTextBlockVectorCount.Text = $"Vector256<byte>.Count={Vector256<byte>.Count}  Vector<byte>.Count={Vector<byte>.Count}";
            MyTextBlockCpuThreadCount.Text = $"CPUスレッド数：{Environment.ProcessorCount}";



            ButtonAll.Click += (s, e) => MyExeAll();
            Button1.Click += (s, e) => MyExe(Test1, Tb1);
            Button2.Click += (s, e) => MyExe(Test2, Tb2);
            Button3.Click += (s, e) => MyExe(Test3, Tb3, MyArray);
            Button4.Click += (s, e) => MyExe(Test4, Tb4, MyArray);
            Button5.Click += (s, e) => MyExe(Test5, Tb5, MyArray);
            Button6.Click += (s, e) => MyExe(Test6, Tb6, MyArray);
            Button7.Click += (s, e) => MyExe(Test1_MT, Tb7);
            Button8.Click += (s, e) => MyExe(Test2_MT, Tb8);
            Button9.Click += (s, e) => MyExe(Test3_MT, Tb9, MyArray);
            Button10.Click += (s, e) => MyExe(Test6_MT, Tb10, MyArray);
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

        //基準1倍速
        private void Test1()
        {
            for (int i = 0; i < ELEMENT_COUNT; i++)
            {
                _ = Math.Sqrt(2);
            }
        }
        //4倍速、Vectorで4つづつ処理だから？
        private void Test2()
        {
            for (int i = 0; i < ELEMENT_COUNT / 4; i++)
            {
                var v = Vector256.Create(2d, 2d, 2d, 2d);
                _ = Avx.Sqrt(v);
            }
        }
        //ここから配列の値
        //0.1倍速、異様に遅い、原因不明
        private void Test3(byte[] vs)
        {
            for (int i = 0; i < vs.Length; i++)
            {
                _ = Math.Sqrt(vs[i]);
            }
        }
        //1倍速、VectorのCreateメソッドでVector作成
        private void Test4(byte[] vs)
        {
            int simdLength = Vector256<double>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            for (int i = 0; i < lastIndex; i += simdLength)
            {
                _ = Avx.Sqrt(Vector256.Create((double)vs[i], vs[i + 1], vs[i + 2], vs[i + 3]));
            }
        }
        //1.3倍速、VectorのCreateメソッドでVector作成、ポインタ使用
        private unsafe void Test5(byte[] vs)
        {
            int simdLength = Vector256<double>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    _ = Avx.Sqrt(Vector256.Create((double)p[i], p[i + 1], p[i + 2], p[i + 3]));
                }
            }
        }
        //4倍速、コンバーターを使ってVector作成
        private unsafe void Test6(byte[] vs)
        {
            int simdLength = Vector256<double>.Count;
            int lastIndex = vs.Length - (vs.Length % simdLength);
            fixed (byte* p = vs)
            {
                for (int i = 0; i < lastIndex; i += simdLength)
                {
                    _ = Avx.Sqrt(Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(p)));
                }
            }
        }

        //ここからマルチスレッド使用
        //2.9倍速
        private void Test1_MT()
        {
            Parallel.ForEach(Partitioner.Create(0, ELEMENT_COUNT),
                range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        _ = Math.Sqrt(2);
                    }
                });
        }

        //10.8倍速
        private void Test2_MT()
        {
            Parallel.ForEach(Partitioner.Create(0, ELEMENT_COUNT / 4),
                range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        _ = Avx.Sqrt(Vector256.Create(2d, 2, 2, 2));
                    }
                });
        }

        //0.4倍速
        private void Test3_MT(byte[] vs)
        {
            Parallel.ForEach(Partitioner.Create(0, ELEMENT_COUNT),
                range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        _ = Math.Sqrt(vs[i]);
                    }
                });
        }

        //12倍速、やっぱりVectorのSqrtは速い
        private unsafe void Test6_MT(byte[] vs)
        {
            Parallel.ForEach(Partitioner.Create(0, ELEMENT_COUNT), range =>
            {
                int simdLength = Vector256<double>.Count;
                int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                fixed (byte* p = vs)
                {
                    for (int i = range.Item1; i < range.Item2; i += simdLength)
                    {
                        _ = Avx.Sqrt(Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(p)));
                    }
                }
            });
        }



        private void MyInitialize()
        {
            MyArray = new byte[ELEMENT_COUNT];

            //指定値で埋める
            var span = new Span<byte>(MyArray);
            span.Fill(2);

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


        }

        #region 時間計測
        private void MyExe(Action action, TextBlock tb)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                action();
            }
            sw.Stop();
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒  {action.Method.Name}");
        }

        private void MyExe(Action<byte[]> action, TextBlock tb, byte[] vs)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                action(vs);
            }
            sw.Stop();
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒  {action.Method.Name}");
        }

        //一斉テスト用
        private async void MyExeAll()
        {
            var sw = new Stopwatch();
            sw.Start();
            this.IsEnabled = false;
            await Task.Run(() => MyExe(Test1, Tb1));
            await Task.Run(() => MyExe(Test2, Tb2));
            await Task.Run(() => MyExe(Test3, Tb3, MyArray));
            await Task.Run(() => MyExe(Test4, Tb4, MyArray));
            await Task.Run(() => MyExe(Test5, Tb5, MyArray));
            await Task.Run(() => MyExe(Test6, Tb6, MyArray));
            await Task.Run(() => MyExe(Test1_MT, Tb7));
            await Task.Run(() => MyExe(Test2_MT, Tb8));
            await Task.Run(() => MyExe(Test3_MT, Tb9, MyArray));
            await Task.Run(() => MyExe(Test6_MT, Tb10, MyArray));
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
