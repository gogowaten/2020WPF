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

//結果
//MathのSqrtを使って普通に計算で良さそう、Vector256floatで計算しても2倍速も行かないくらい


namespace _20200306_ユークリッド距離
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyX;
        private byte[] MyY;
        private byte[] MyZ;
        private byte[] MyXX;
        private byte[] MyYY;
        private byte[] MyZZ;
        private double[] MyResult;
        private float[] MyResultFloat;

        private const int LOOP_COUNT = 1000;
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
            Button1.Click += (s, e) => MyExe(Test1_MathSqrt, Tb1, MyX, MyY, MyZ, MyXX, MyYY, MyZZ, MyResult);
            Button2.Click += (s, e) => MyExe(Test2_Vector256Double, Tb2, MyX, MyY, MyZ, MyXX, MyYY, MyZZ, MyResult);
            Button3.Click += (s, e) => MyExe(Test3_Vector256Float, Tb3, MyX, MyY, MyZ, MyXX, MyYY, MyZZ, MyResultFloat);
            //Button4.Click += (s, e) => MyExe(Test4_Vector2_Distance, Tb4, MyX, MyY, MyXX, MyYY, MyResultFloat);
            Button5.Click += (s, e) => MyExe(Test5_Vector3_Distance, Tb5, MyX, MyY, MyZ, MyXX, MyYY, MyZZ, MyResultFloat);
            //Button6.Click += (s, e) => MyExe(Test6_Intrinsics_SSE2_MultiplyAddAdjacent_int, Tb6, MyArray);
            //Button7.Click += (s, e) => MyExe(Test7_Intrinsics_SSE41_DotProduct_float, Tb7, MyArray);
            //Button8.Click += (s, e) => MyExe(Test8_Intrinsics_SSE41_DotProduct_float, Tb8, MyArray);
            ////Button9.Click += (s, e) => MyExe(Test9_Intrinsics_SSE41_DotProduct_float, Tb9, MyArray);
            ////Button10.Click += (s, e) => MyExe(Test11_Normal_MT, Tb10, MyArray);
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

        private void Test1_MathSqrt(byte[] x, byte[] y, byte[] z, byte[] xx, byte[] yy, byte[] zz, double[] result)
        {
            Parallel.ForEach(Partitioner.Create(0, x.Length), range =>
            {
                int xd, yd, zd;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    xd = x[i] - xx[i];
                    yd = y[i] - yy[i];
                    zd = z[i] - zz[i];
                    result[i] = Math.Sqrt((xd * xd) + (yd * yd) + (zd * zd));
                }
            });
        }


        private unsafe void Test2_Vector256Double(byte[] x, byte[] y, byte[] z, byte[] xx, byte[] yy, byte[] zz, double[] result)
        {
            Parallel.ForEach(Partitioner.Create(0, x.Length), range =>
            {
                int simdLength = Vector256<double>.Count;
                int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                Vector256<double> vx, vy, vz, vm;
                fixed (byte* px = x, py = y, pz = z, pxx = xx, pyy = yy, pzz = zz)
                {
                    fixed (double* dp = result)
                    {
                        for (int i = range.Item1; i < range.Item2; i += simdLength)
                        {
                            //引き算
                            vx = Avx.Subtract(
                                Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(px + i)),
                                Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(pxx + i)));
                            vy = Avx.Subtract(
                                Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(py + i)),
                                Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(pyy + i)));
                            vz = Avx.Subtract(
                                Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(pz + i)),
                                Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(pzz + i)));

                            //2乗和の平方根
                            vm = Avx.Add(Avx.Multiply(vx, vx), Avx.Multiply(vy, vy));
                            vm = Avx.Sqrt(Avx.Add(vm, Avx.Multiply(vz, vz)));

                            //結果を配列に書き込み
                            Avx.Store(dp + i, vm);
                        }
                    }
                }
            });
        }

        private unsafe void Test3_Vector256Float(byte[] x, byte[] y, byte[] z, byte[] xx, byte[] yy, byte[] zz, float[] result)
        {
            Parallel.ForEach(Partitioner.Create(0, x.Length), range =>
            {
                int simdLength = Vector256<float>.Count;
                int lastIndex = range.Item2 - (range.Item2 - range.Item1) % simdLength;
                Vector256<float> vx, vy, vz, vm;
                fixed (byte* px = x, py = y, pz = z, pxx = xx, pyy = yy, pzz = zz)
                {
                    fixed (float* dp = result)
                    {
                        for (int i = range.Item1; i < range.Item2; i += simdLength)
                        {
                            vx = Avx.Subtract(
                                Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(px + i)),
                                Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(pxx + i)));
                            vy = Avx.Subtract(
                                Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(py + i)),
                                Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(pyy + i)));
                            vz = Avx.Subtract(
                                Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(pz + i)),
                                Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(pzz + i)));

                            vm = Avx.Add(Avx.Multiply(vx, vx), Avx.Multiply(vy, vy));
                            vm = Avx.Sqrt(Avx.Add(vm, Avx.Multiply(vz, vz)));
                            Avx.Store(dp + i, vm);
                        }
                    }
                }
            });
        }

        //遅すぎ
        //private unsafe void Test4_Vector2_Distance(byte[] x, byte[] y, byte[] xx, byte[] yy, float[] result)
        //{
        //    Parallel.ForEach(Partitioner.Create(0, x.Length), range =>
        //    {
        //        var v1 = new Vector2();
        //        var v2 = new Vector2();
        //        for (int i = range.Item1; i < range.Item2; i++)
        //        {
        //            v1.X = x[i]; v1.Y = y[i];
        //            v2.X = xx[i]; v2.Y = yy[i];
        //            result[i] = Vector2.Distance(v1, v2);
        //        }
        //    });
        //}

        private unsafe void Test5_Vector3_Distance(byte[] x, byte[] y, byte[] z, byte[] xx, byte[] yy, byte[] zz, float[] result)
        {
            Parallel.ForEach(Partitioner.Create(0, x.Length), range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    result[i] = Vector3.Distance(new Vector3(x[i], y[i], z[i]), new Vector3(xx[i], yy[i], zz[i]));
                }
            });
        }


        #region 未使用
        private unsafe float Test(float* f1, float* f2, int n)
        {            
            var u = Vector128<float>.Zero;
            for (int i = 0; i < n; i += 4)
            {
                var w = Sse.LoadVector128(f1);
                var x = Sse.LoadVector128(f2);
                x = Sse.Multiply(w, x);
                u = Sse.Add(u, x);
            }
            float* p = stackalloc float[4];
            Sse.Store(p, u);
            return p[0] + p[1] + p[2] + p[3];
        }

        private unsafe void Exe()
        {
            float[] f1 = new float[ELEMENT_COUNT];
            float[] f2 = new float[ELEMENT_COUNT];
            fixed (float* p1 = f1, p2 = f2)
            {
                var neko = Test(p1, p2, ELEMENT_COUNT);
            }
        }
        #endregion 未使用

        private void MyInitialize()
        {
            MyX = new byte[ELEMENT_COUNT];
            MyY = new byte[ELEMENT_COUNT];
            MyZ = new byte[ELEMENT_COUNT];
            MyXX = new byte[ELEMENT_COUNT];
            MyYY = new byte[ELEMENT_COUNT];
            MyZZ = new byte[ELEMENT_COUNT];
            MyResult = new double[ELEMENT_COUNT];
            MyResultFloat = new float[ELEMENT_COUNT];

            //指定値で埋める
            //var span = new Span<byte>(MyX);
            //span.Fill(255);

            //最後の要素
            //MyArray[ELEMENT_COUNT - 1] = 100;

            //ランダム値
            //var r = new Random();
            //r.NextBytes(MyArray);

            //0～255までを連番で繰り返し
            for (int i = 0; i < ELEMENT_COUNT; i++)
            {
                MyX[i] = (byte)i;
                MyY[i] = (byte)i;
                MyZ[i] = (byte)i;
            }


        }

        #region 時間計測
        private void MyExe(Action<byte[], byte[], byte[], byte[], byte[], byte[], double[]> func,
            TextBlock tb, byte[] x, byte[] y, byte[] z, byte[] xx, byte[] yy, byte[] zz, double[] result)
        {
            Span<double> span = new Span<double>(result);
            span.Fill(0);
            var sw = new Stopwatch();

            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                func(x, y, z, xx, yy, zz, result);
            }
            sw.Stop();

            double total = GetTotal(result);
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒 {total.ToString("F16")}  {func.Method.Name}");
        }

        private void MyExe(Action<byte[], byte[], byte[], byte[], byte[], byte[], float[]> func,
            TextBlock tb, byte[] x, byte[] y, byte[] z, byte[] xx, byte[] yy, byte[] zz, float[] result)
        {
            Span<float> span = new Span<float>(result);
            span.Fill(0);
            var sw = new Stopwatch();

            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                func(x, y, z, xx, yy, zz, result);
            }
            sw.Stop();

            double total = GetTotal(result);
            this.Dispatcher.Invoke(() => tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("000.000")}秒 {total.ToString("F16")}  {func.Method.Name}");
        }

        private double GetTotal(double[] vs)
        {
            double total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total;
        }
        private double GetTotal(float[] vs)
        {
            double total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total;
        }

        //一斉テスト用
        private async void MyExeAll()
        {
            var sw = new Stopwatch();
            sw.Start();
            this.IsEnabled = false;
            await Task.Run(() => MyExe(Test1_MathSqrt, Tb1, MyX, MyY, MyZ, MyXX, MyYY, MyZZ, MyResult));
            await Task.Run(() => MyExe(Test2_Vector256Double, Tb2, MyX, MyY, MyZ, MyXX, MyYY, MyZZ, MyResult));
            await Task.Run(() => MyExe(Test3_Vector256Float, Tb3, MyX, MyY, MyZ, MyXX, MyYY, MyZZ, MyResultFloat));
            //await Task.Run(() => MyExe(Test4_Intrinsics_FMA_MultiplyAdd_double, Tb4, MyArray));
            await Task.Run(() => MyExe(Test5_Vector3_Distance, Tb5, MyX, MyY, MyZ, MyXX, MyYY, MyZZ, MyResultFloat));
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
