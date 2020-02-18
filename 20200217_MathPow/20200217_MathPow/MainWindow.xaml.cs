using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

//C#.NETのMathPowは速くない？べき乗計算は掛け算を繰り返したほうが速い場合もあった - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2020/02/18/130738

namespace _20200217_MathPow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int MY_COUNT = 10_000;
        private const int MY_INTEGER = 3;
        private const double MY_DOUBLE = 3.3;
        private double[] MyArray;
        public MainWindow()
        {
            InitializeComponent();

            this.Title = this.ToString();
            MyTextBlock.Text = $"Math.Powは速くない？\nべき乗計算{MY_COUNT.ToString("N0")} * {MY_COUNT.ToString("N0")}回の処理時間";
            MyArray = new double[MY_COUNT];
            var span = new Span<double>(MyArray);
            span.Fill(10.0);

            ButtonAll.Click += async (s, e) => await MyExeAll();
            ButtonReset.Click += (s, e) => MyReset();

            Button1.Click += (s, e) => MyExe(Test01_2乗_int, MY_INTEGER, MY_COUNT, Tb1);
            //Button2.Click += (s, e) => MyExe((a, b) => { for (int i = 0; i < b; i++) { _ = a * a; } }, MY_INTEGER, LOOP_COUNT,Tb2);
            //Button3.Click += (s, e) => MyExe((a, b) => Test01_int_普通の掛け算で2乗(a, b), MY_INTEGER, LOOP_COUNT,Tb3);
            Button2.Click += (s, e) => MyExe(Test02_2乗_int_MathPow, MY_INTEGER, MY_COUNT, Tb2);
            Button3.Click += (s, e) => MyExe(Test03_2乗その2_int, MY_INTEGER, MY_COUNT, Tb3);
            Button4.Click += (s, e) => MyExe(Test04_2乗, MY_DOUBLE, MY_COUNT, Tb4);
            Button5.Click += (s, e) => MyExe(Test05_2乗MathPow, MY_DOUBLE, MY_COUNT, Tb5);
            Button6.Click += (s, e) => MyExe(Test06_10乗, MY_DOUBLE, MY_COUNT, Tb6);
            Button7.Click += (s, e) => MyExe(Test07_10乗MathPow, MY_DOUBLE, MY_COUNT, Tb7);
            Button8.Click += (s, e) => MyExe(Test08_ループで10乗, MY_DOUBLE, MY_COUNT, Tb8);
            Button9.Click += (s, e) => MyExe(Test09_ループで9回掛け算, MY_DOUBLE, MY_COUNT, Tb9);
            Button10.Click += (s, e) => MyExe(Test10_配列2乗和, MyArray, MY_COUNT, Tb10);
            Button11.Click += (s, e) => MyExe(Test11_配列2乗和_MathPow, MyArray, MY_COUNT, Tb11);
            Button12.Click += (s, e) => MyExe(Test12_配列2乗値破棄, MyArray, MY_COUNT, Tb12);
            Button13.Click += (s, e) => MyExe(Test13_配列2乗値破棄_MathPow, MyArray, MY_COUNT, Tb13);
            Button14.Click += (s, e) => Test14_Span2乗和_MathPow(MyArray, Tb14);
            Button15.Click += (s, e) => MyExe(Test15_2乗和その2, MyArray, MY_COUNT, Tb15);
            Button16.Click += (s, e) => MyExe(Test16_2乗和その2_MathPow, MyArray, MY_COUNT, Tb16);
            Button17.Click += (s, e) => MyExe(Test17_配列2乗値破棄その2, MyArray, MY_COUNT, Tb17);
            Button18.Click += (s, e) => MyExe(Test18_配列2乗値破棄その2_MathPow, MyArray, MY_COUNT, Tb18);

        }

        private void Test01_2乗_int(int num, int count)
        {
            for (int i = 0; i < count; i++) { _ = num * num; }
        }
        private void Test02_2乗_int_MathPow(int num, int count)
        {
            for (int i = 0; i < count; i++) { _ = Math.Pow(num, 2); }
        }
        private void Test03_2乗その2_int(int num, int count)
        {
            for (int i = 0; i < count; i++) { var temp = num * num; }
        }

        private void Test04_2乗(double num, int count)
        {
            for (int i = 0; i < count; i++) { _ = num * num; }
        }
        private void Test05_2乗MathPow(double num, int count)
        {
            for (int i = 0; i < count; i++) { _ = Math.Pow(num, 2); }
        }

        private void Test06_10乗(double num, int count)
        {
            for (int i = 0; i < count; i++) { _ = num * num * num * num * num * num * num * num * num * num; }
        }
        private void Test07_10乗MathPow(double num, int count)
        {
            for (int i = 0; i < count; i++) { _ = Math.Pow(num, 10); }
        }
        private void Test08_ループで10乗(double num, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var temp = num;
                for (int k = 0; k < 9; k++)
                {
                    temp *= num;
                }
            }
        }

        private void Test09_ループで9回掛け算(double num, int count)
        {
            for (int i = 0; i < count; i++)
            {
                for (int k = 0; k < 9; k++)
                {
                    _ = num * num;
                }
            }
        }

        private double Test10_配列2乗和(double[] ary)
        {
            double total = 0;
            for (int i = 0; i < ary.Length; i++)
                total += ary[i] * ary[i];
            return total;
        }

        private double Test11_配列2乗和_MathPow(double[] ary)
        {
            double total = 0;
            for (int i = 0; i < ary.Length; i++)
                total += Math.Pow(ary[i], 2.0);
            return total;
        }

        private void Test12_配列2乗値破棄(double[] ary)
        {
            for (int i = 0; i < ary.Length; i++)
                _ = ary[i] * ary[i];
        }

        private void Test13_配列2乗値破棄_MathPow(double[] ary)
        {
            for (int i = 0; i < ary.Length; i++)
                _ = Math.Pow(ary[i], 2);
        }

        private void Test14_Span2乗和_MathPow(Span<double> span, TextBlock textBlock)
        {
            var sw = new Stopwatch();
            sw.Start();
            double total = 0;
            for (int k = 0; k < MY_COUNT; k++)
            {
                total = 0;
                for (int i = 0; i < span.Length; i++)
                {
                    total += Math.Pow(span[i], 2.0);
                }
            }
            sw.Stop();
            this.Dispatcher.Invoke(() =>
            {
                textBlock.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒 合計値={total} {nameof(Test14_Span2乗和_MathPow)}";
            });

        }

        private double Test15_2乗和その2(double[] ary)
        {
            double total = 0;
            double temp;
            for (int i = 0; i < ary.Length; i++)
            {
                temp = ary[i];
                total += temp * temp;
            }
            return total;
        }
        private double Test16_2乗和その2_MathPow(double[] ary)
        {
            double total = 0;
            double temp;
            for (int i = 0; i < ary.Length; i++)
            {
                temp = ary[i];
                total += Math.Pow(temp, 2.0);
            }
            return total;
        }
        private void Test17_配列2乗値破棄その2(double[] ary)
        {
            double d = ary[0];
            for (int i = 0; i < ary.Length; i++)
                _ = d * d;
        }

        private void Test18_配列2乗値破棄その2_MathPow(double[] ary)
        {
            double d = ary[0];
            for (int i = 0; i < ary.Length; i++)
                _ = Math.Pow(d, 2);
        }








        private void MyExe(Action<int, int> action, int num, int count, TextBlock textBlock)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                action(num, count);
            }
            sw.Stop();

            this.Dispatcher.Invoke(() =>
            {
                textBlock.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒 {action.Method.Name}";
            });

        }
        private void MyExe(Action<double, int> action, double num, int count, TextBlock textBlock)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                action(num, count);
            }
            sw.Stop();
            this.Dispatcher.Invoke(() =>
            {
                textBlock.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒 {action.Method.Name}";
            });
        }
        private void MyExe(Func<double[], double> func, double[] ary, int count, TextBlock textBlock)
        {
            var sw = new Stopwatch();
            sw.Start();
            double total = 0;
            for (int i = 0; i < count; i++)
            {
                total = func(ary);
            }
            sw.Stop();
            this.Dispatcher.Invoke(() =>
            {
                textBlock.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒 合計値={total} {func.Method.Name}";
            });
        }
        private void MyExe(Action<double[]> action, double[] ary, int count, TextBlock textBlock)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                action(ary);
            }
            sw.Stop();
            this.Dispatcher.Invoke(() =>
            {
                textBlock.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒 {action.Method.Name}";
            });
        }
        

        private async Task MyExeAll()
        {
            var sw = new Stopwatch();
            sw.Start();
            this.IsEnabled = false;
            await Task.Run(() => MyExe(Test01_2乗_int, MY_INTEGER, MY_COUNT, Tb1));
            await Task.Run(() => MyExe(Test02_2乗_int_MathPow, MY_INTEGER, MY_COUNT, Tb2));
            await Task.Run(() => MyExe(Test03_2乗その2_int, MY_INTEGER, MY_COUNT, Tb3));
            await Task.Run(() => MyExe(Test04_2乗, MY_DOUBLE, MY_COUNT, Tb4));
            await Task.Run(() => MyExe(Test05_2乗MathPow, MY_DOUBLE, MY_COUNT, Tb5));
            await Task.Run(() => MyExe(Test06_10乗, MY_DOUBLE, MY_COUNT, Tb6));
            await Task.Run(() => MyExe(Test07_10乗MathPow, MY_DOUBLE, MY_COUNT, Tb7));
            await Task.Run(() => MyExe(Test08_ループで10乗, MY_DOUBLE, MY_COUNT, Tb8));
            await Task.Run(() => MyExe(Test09_ループで9回掛け算, MY_DOUBLE, MY_COUNT, Tb9));
            await Task.Run(() => MyExe(Test10_配列2乗和, MyArray, MY_COUNT, Tb10));
            await Task.Run(() => MyExe(Test11_配列2乗和_MathPow, MyArray, MY_COUNT, Tb11));
            await Task.Run(() => MyExe(Test12_配列2乗値破棄, MyArray, MY_COUNT, Tb12));
            await Task.Run(() => MyExe(Test13_配列2乗値破棄_MathPow, MyArray, MY_COUNT, Tb13));
            await Task.Run(() => Test14_Span2乗和_MathPow(MyArray, Tb14));
            await Task.Run(() => MyExe(Test15_2乗和その2, MyArray, MY_COUNT, Tb15));
            await Task.Run(() => MyExe(Test16_2乗和その2_MathPow, MyArray, MY_COUNT, Tb16));
            await Task.Run(() => MyExe(Test17_配列2乗値破棄その2, MyArray, MY_COUNT, Tb17));
            await Task.Run(() => MyExe(Test18_配列2乗値破棄その2_MathPow, MyArray, MY_COUNT, Tb18));
            this.IsEnabled = true;
            sw.Stop();
            TbAll.Text = $"一斉テスト処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒";
        }
        private void MyReset()
        {
            var tb = new List<TextBlock>() { Tb1, Tb2, Tb3, Tb4, Tb5, Tb6, Tb7, Tb8, Tb9, Tb10, Tb11, Tb12, Tb13, Tb14, Tb15, Tb16, Tb17, Tb18, TbAll };
            foreach (var item in tb)
            {
                item.Text = "";
            }
        }
    }
}
