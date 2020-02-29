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

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Numerics;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace _20200229_SIMDで掛け算
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyArray;
        private const int LOOP_COUNT = 1000;
        private const int ELEMENT_COUNT = 10_000_000;// 538_976_319;// 67_372_039;//要素数

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
            this.Title = this.ToString();


            MyTextBlock.Text = $"byte型配列要素数{ELEMENT_COUNT.ToString("N0")}の合計値を {LOOP_COUNT}回求める";
            MyTextBlockVectorCount.Text = $"Vector256<byte>.Count={Vector256<byte>.Count}  Vector<byte>.Count={Vector<byte>.Count}";
            MyTextBlockCpuThreadCount.Text = $"CPUスレッド数：{Environment.ProcessorCount}";

            //ButtonAll.Click += (s, e) => MyExeAll();
            //Button1.Click += (s, e) => MyExe(Test1_Normal, Tb1, MyArray);
            //Button2.Click += (s, e) => MyExe(Test2_Normal_MT, Tb2, MyArray);
            //Button3.Click += (s, e) => MyExe(Test3_Normal4, Tb3, MyArray);
            //Button4.Click += (s, e) => MyExe(Test4_Intrinsics_int, Tb4, MyArray);
            //Button5.Click += (s, e) => MyExe(Test5_Intrinsics_int_MT, Tb5, MyArray);
            //Button6.Click += (s, e) => MyExe(Test6_Intrinsics_int_MT2, Tb6, MyArray);
            //Button7.Click += (s, e) => MyExe(Test7_Intrinsics_long_MT, Tb7, MyArray);
            //Button8.Click += (s, e) => MyExe(Test8_Numerics_uint, Tb8, MyArray);
            //Button9.Click += (s, e) => MyExe(Test9_Nunerics_uint_MT, Tb9, MyArray);
            //Button10.Click += (s, e) => MyExe(Test10_Numerics_long_MT, Tb10, MyArray);

        }

        //普通に掛け算
        private void Test1_Normal(byte[] vs)
        {
           
        }

        //普通に掛け算をマルチスレッド化
        private long Test2_Normal_MT(byte[] vs)
        {
            long total = 0;
            Parallel.ForEach(
                Partitioner.Create(0, vs.Length, vs.Length / Environment.ProcessorCount),
                (range) =>
                {
                    long subtotal = 0;
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        subtotal += vs[i];
                    }
                    System.Threading.Interlocked.Add(ref total, subtotal);
                });
            return total;
        }


        private void MyInitialize()
        {
            MyArray = new byte[ELEMENT_COUNT];

            //指定値で埋める
            var span = new Span<byte>(MyArray);
            span.Fill(255);

            //最後の要素
            //MyArray[ELEMENT_COUNT - 1] = 100;

            //ランダム値
            //var r = new Random();
            //r.NextBytes(MyArray);

            ////0～255までを連番で繰り返し
            //for (int i = 0; i < ELEMENT_COUNT; i++)
            //{
            //    MyArray[i] = (byte)i;
            //}


        }

        #region 時間計測
        private void MyExe(Func<byte[], long> func, TextBlock tb, byte[] vs)
        {
            long total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(vs);
            }
            sw.Stop();
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒 {total.ToString("N0")} | {func.Method.Name}");
        }


        //一斉テスト用
        private async void MyExeAll()
        {
            var sw = new Stopwatch();
            sw.Start();
            this.IsEnabled = false;
            //await Task.Run(() => MyExe(Test1_Normal, Tb1, MyArray));
            //await Task.Run(() => MyExe(Test2_Normal_MT, Tb2, MyArray));
            //await Task.Run(() => MyExe(Test3_Normal4, Tb3, MyArray));
            //await Task.Run(() => MyExe(Test4_Intrinsics_int, Tb4, MyArray));
            //await Task.Run(() => MyExe(Test5_Intrinsics_int_MT, Tb5, MyArray));
            //await Task.Run(() => MyExe(Test6_Intrinsics_int_MT2, Tb6, MyArray));
            //await Task.Run(() => MyExe(Test7_Intrinsics_long_MT, Tb7, MyArray));
            //await Task.Run(() => MyExe(Test8_Numerics_uint, Tb8, MyArray));
            //await Task.Run(() => MyExe(Test9_Nunerics_uint_MT, Tb9, MyArray));
            //await Task.Run(() => MyExe(Test10_Numerics_long_MT, Tb10, MyArray));

            this.IsEnabled = true;
            sw.Stop();
            TbAll.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒";
        }
        #endregion 時間計測
    }
}
