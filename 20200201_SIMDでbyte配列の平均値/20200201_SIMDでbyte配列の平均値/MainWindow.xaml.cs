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
using System.Diagnostics;
//配列の一部だけをコピーするには？［C#／VB］：.NET TIPS - ＠IT
//https://www.atmarkit.co.jp/ait/articles/1705/10/news015.html
//方法: 小さいループ本体を高速化する | Microsoft Docs
//https://docs.microsoft.com/ja-jp/dotnet/standard/parallel-programming/how-to-speed-up-small-loop-bodies


//イマイチ
//byte配列の平均値を求める
//SIMDを使う場合はVector.addを使うんだけどVector<byte>だと合計値が255いじょうにならないから
//Vector<int>を使う必要がある、ってことはbyte型配列をint型配列に変換する必要がある、この処理が重いので
//SIMD使わずに普通にループで足し算したほうが10倍以上速かった
namespace _20200201_SIMDでbyte配列の平均値
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int ELEMENT_COUNT = 1000;
        const int MIN_VALUE = 0;
        const int MAX_VALUE = 100000000;
        const int LOOP_COUNT = 100;
        int[] IntArray;
        //byte[] ByteArray;

        public MainWindow()
        {
            InitializeComponent();



            IntArray = MakeIntArray(ELEMENT_COUNT, MIN_VALUE, MAX_VALUE);
            //ByteArray = MakeByteArray(ELEMENT_COUNT);

            //var neko = Vector<Int64>.Count;//4
            //var inu = Vector<decimal>.Count;//エラー、サポートされていない
            //var neko = Vector<long>.Count;//4
            //var neko = new Span<int>(IntArray, 0, 20);
            //var neko = new Vector<int>(new Span<int>(IntArray).Slice(1,8));
            //var neko = long.MaxValue > (int.MaxValue * 10000000L);
            //var neko = new Span<int>(IntArray, 0, 10);
            //var inu = new Span<long>(IntArray, 0, 29);//エラー型が違う

            IntArray = new int[2];
            IntArray[0] = int.MaxValue;
            IntArray[1] = 10;
            var rangePartitioner = System.Collections.Concurrent.Partitioner.Create(0,IntArray.Length);
            BigInteger max = 0;
            Parallel.ForEach(rangePartitioner, (range, loopState) =>
             {
                 for (int i = range.Item1; i < range.Item2; i++)
                 {
                     max += IntArray[i];
                 }
             });
            //var neko = max /(double) IntArray.Length;
            var inu = IntArray.Average();

            MessageBox.Show(IntArray.Average().ToString());
        }

        private int[] MakeIntArray(int count, int min, int max)
        {
            var r = new Random();
            var ary = new int[count];
            for (int i = 0; i < count; i++)
            {
                ary[i] = r.Next(min, max);
            }
            return ary;
        }


        #region
        private void MyIntAverageTime(Func<int[], double> func, TextBlock textBlock)
        {
            var sw = new Stopwatch();
            double average = 0;
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                average = func(IntArray);
            }
            sw.Stop();
            textBlock.Text =
                $"{System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}" +
                $"\n平均値 = {average}\n処理時間：{sw.Elapsed.TotalSeconds.ToString("00.00")}秒";
        }
        private void MyIntAverageTime(Func<List<int[]>, double> func, TextBlock textBlock, List<int[]> aryList)
        {
            var sw = new Stopwatch();
            double average = 0;
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                average = func(aryList);
            }
            sw.Stop();
            textBlock.Text =
                $"{System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}" +
                $"\n平均値 = {average}\n処理時間：{sw.Elapsed.TotalSeconds.ToString("00.00")}秒";
        }

        #endregion

        #region シングルスレッド
        private long IntAdd(int[] ary)
        {
            long add = 0;
            for (int i = 0; i < ary.Length; i++)
            {
                add += ary[i];
            }
            return add;
        }
        private double IntAverage(int[] ary)
        {
            return IntAdd(ary) / (double)ary.Length;
        }

        private long IntAddSimd(int[] ary)
        {
            long[] longAry = new long[ary.Length];//9ms
            ary.CopyTo(longAry, 0);//36ms、この処理が一番時間かかる
            var v1 = new Vector<long>(longAry);
            int simdLength = Vector<long>.Count;//Vectorの長さ、一度に計算できる量
            int amari = ary.Length % simdLength;//余り、配列の端数
            int lastIndex = ary.Length - amari;//SIMDで計算する最後のIndex
            //SIMD(Vector.Add)で足し算
            for (int i = simdLength; i < lastIndex; i += simdLength)//27ms
            {
                v1 = System.Numerics.Vector.Add(v1, new Vector<long>(longAry, i));
            }
            //Vectorの中の合計
            long add = v1[0];
            for (int i = 1; i < simdLength; i++)
            {
                add += v1[i];
            }
            //配列の端数も合計
            for (int i = ary.Length - amari; i < ary.Length; i++)
            {
                add += ary[i];
            }
            //平均値
            return add;
        }
        private double IntAverageSimd(int[] ary)
        {
            return IntAddSimd(ary) / (double)ary.Length;
        }



        //不正確、オーバーフロー
        private double IntIntAverageSimdIncorrect(int[] ary)
        {
            var v1 = new Vector<int>(ary);
            int simdLength = Vector<int>.Count;//Vectorの長さ、一度に計算できる量
            int amari = ary.Length % simdLength;//余り、配列の端数
            int lastIndex = ary.Length - amari;//SIMDで計算する最後のIndex
            //SIMDで足し算計算
            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                v1 = System.Numerics.Vector.Add(v1, new Vector<int>(ary, i));
            }
            //Vectorの中の合計
            int add = v1[0];
            for (int i = 1; i < simdLength; i++)
            {
                add += v1[i];
            }
            //配列の端数も合計
            for (int i = ary.Length - amari; i < ary.Length; i++)
            {
                add += ary[i];
            }
            //平均値
            return add / (double)ary.Length;
        }

        private double IntAverageSimdCastLong(int[] ary)
        {
            Vector<long> v = (Vector<long>)new Vector<int>(ary);
            int simdLength = Vector<long>.Count;//Vectorの長さ、一度に計算できる量
            int amari = ary.Length % simdLength;//余り、配列の端数
            int lastIndex = ary.Length - amari;//SIMDで計算する最後のIndex
            //SIMDで足し算計算
            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                v = System.Numerics.Vector.Add(v, (Vector<long>)new Vector<int>(ary, i));                
            }
            //Vectorの中の合計
            long add = v[0];
            for (int i = 1; i < simdLength; i++)
            {
                add += v[i];
            }
            //配列の端数も合計
            for (int i = ary.Length - amari; i < ary.Length; i++)
            {
                add += ary[i];
            }
            //平均値
            return add / (double)ary.Length;


        }
        #endregion

        #region マルチスレッド



        //Parallelを使ったマルチスレッド
        private double IntAverageMutli(int[] ary)
        {
            int MyTreadsCount = Environment.ProcessorCount;
            long[] adds = new long[MyTreadsCount];
            int windowLength = ary.Length / MyTreadsCount;

            //Linqで配列を分割
            Parallel.For(0, MyTreadsCount, n =>
            {
                adds[n] = IntAdd(ary.Skip(n * windowLength).Take(windowLength).ToArray());
            });
            long add = 0;
            for (int i = 0; i < MyTreadsCount; i++)
            {
                add += adds[i];
            }

            int amari = ary.Length - (ary.Length % MyTreadsCount);
            for (int i = amari; i < ary.Length; i++)
            {
                add += ary[i];
            }
            return add / (double)ary.Length;
        }

        private double IntAverageMulti2(int[] ary)
        {
            int MyTreadsCount = Environment.ProcessorCount;
            long[] adds = new long[MyTreadsCount];
            int windowLength = ary.Length / MyTreadsCount;
            //Buffer.BlockCopyを使って配列を分割、Linqより気持ち速い
            int typeSize = System.Runtime.InteropServices.Marshal.SizeOf(ary.GetType().GetElementType());
            List<int[]> vs = new List<int[]>();
            for (int i = 0; i < MyTreadsCount; i++)
            {
                vs.Add(new int[windowLength]);
                Buffer.BlockCopy(ary, i * windowLength * typeSize, vs[i], 0, windowLength * typeSize);
            }

            Parallel.For(0, MyTreadsCount, n =>
            {
                adds[n] = IntAdd(vs[n]);
            });
            long add = 0;
            for (int i = 0; i < MyTreadsCount; i++)
            {
                add += adds[i];
            }

            int amari = ary.Length - (ary.Length % MyTreadsCount);
            for (int i = amari; i < ary.Length; i++)
            {
                add += ary[i];
            }
            return add / (double)ary.Length;
        }


        //Takeを使ってマルチスレッド
        private double IntAverageMulti3(int[] ary)
        {
            int MyTreadsCount = Environment.ProcessorCount;
            //int[] adds = new int[MyTreadsCount];
            int windowLength = ary.Length / MyTreadsCount;

            long[] neko = Task.WhenAll(Enumerable.Range(0, MyTreadsCount).Select(n => Task.Run(() =>
                {
                    return IntAdd(ary.Skip(n * windowLength).Take(windowLength).ToArray());
                }))).GetAwaiter().GetResult();

            long add = 0;
            for (int i = 0; i < MyTreadsCount; i++)
            {
                add += neko[i];
            }

            int amari = ary.Length - (ary.Length % MyTreadsCount);
            for (int i = amari; i < ary.Length; i++)
            {
                add += ary[i];
            }
            return add / (double)ary.Length;
        }

        //Parallelを使ったマルチスレッド+SIMD
        private double IntAverageMultiSimd(int[] ary)
        {
            int MyTreadsCount = Environment.ProcessorCount;
            long[] adds = new long[MyTreadsCount];
            int windowLength = ary.Length / MyTreadsCount;

            //Linqで配列を分割
            Parallel.For(0, MyTreadsCount, n =>
            {
                //adds[n] = IntAddSimd(ary.Skip(n * windowLength).Take(windowLength).ToArray());
                adds[n] = IntAddSimd(new ArraySegment<int>(ary, n * windowLength, windowLength).ToArray());
            });
            long add = 0;
            for (int i = 0; i < MyTreadsCount; i++)
            {
                add += adds[i];
            }

            int amari = ary.Length - (ary.Length % MyTreadsCount);
            for (int i = amari; i < ary.Length; i++)
            {
                add += ary[i];
            }
            return add / (double)ary.Length;
        }

        //Takeを使ってマルチスレッド+SIMD
        private double IntAverageMultiSimd2(int[] ary)
        {
            int MyTreadsCount = Environment.ProcessorCount;
            int windowLength = ary.Length / MyTreadsCount;

            long[] neko = Task.WhenAll(Enumerable.Range(0, MyTreadsCount).Select(n => Task.Run(() =>
            {
                //return IntAddSimd(ary.Skip(n * windowLength).Take(windowLength).ToArray());
                //下のほうが少し早い
                return IntAddSimd(new ArraySegment<int>(ary, n * windowLength, windowLength).ToArray());
                //上の方が誤差程度に速い？
                //return IntAddSimd(new Span<int>(ary, n * windowLength, windowLength).ToArray());
            }))).GetAwaiter().GetResult();

            long add = 0;
            for (int i = 0; i < MyTreadsCount; i++)
            {
                add += neko[i];
            }

            int amari = ary.Length - (ary.Length % MyTreadsCount);
            for (int i = amari; i < ary.Length; i++)
            {
                add += ary[i];
            }
            return add / (double)ary.Length;
        }

        //不正確
        private double IntIntAverageMultiSimdIncorrect(int[] ary)
        {
            int MyTreadsCount = Environment.ProcessorCount;
            int windowLength = ary.Length / MyTreadsCount;

            int[] neko = Task.WhenAll(Enumerable.Range(0, MyTreadsCount).Select(n => Task.Run(() =>
            {
                //return IntAddSimdIncorrect(new Span<int>(ary, n * windowLength, windowLength).ToArray());
                //こっちのほうが少し早い
                return IntAddSimdIncorrect(new ArraySegment<int>(ary, n * windowLength, windowLength).ToArray());
            }))).GetAwaiter().GetResult();

            int add = 0;
            for (int i = 0; i < MyTreadsCount; i++)
            {
                add += neko[i];
            }

            int amari = ary.Length - (ary.Length % MyTreadsCount);
            for (int i = amari; i < ary.Length; i++)
            {
                add += ary[i];
            }
            return add / (double)ary.Length;
        }
        //不正確
        private int IntAddSimdIncorrect(int[] ary)
        {
            var v = new Vector<int>(ary, 0);
            int simdLength = Vector<int>.Count;//Vectorの長さ、一度に計算できる量
            int amari = ary.Length % simdLength;//余り、配列の端数
            int lastIndex = ary.Length - amari;//SIMDで計算する最後のIndex
            //SIMD(Vector.Add)で足し算
            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                v = System.Numerics.Vector.Add(v, new Vector<int>(ary, i));
            }
            //Vectorの中の合計
            int add = v[0];
            for (int i = 1; i < simdLength; i++)
            {
                add += v[i];
            }
            //配列の端数も合計
            for (int i = ary.Length - amari; i < ary.Length; i++)
            {
                add += ary[i];
            }
            //平均値
            return add;
        }

        //複数配列、8個まで
        private double MultiArrayMultiSimd(List<int[]> aryList)
        {
            double[] results = new double[aryList.Count];
            Parallel.For(0, aryList.Count, n =>
            {
                results[n] = IntAddSimdIncorrect(aryList[n]);
            });
            return results.Average() / aryList[0].Length;
        }
        #endregion



        private void ButtonExe1_Click(object sender, RoutedEventArgs e)
        {
            MyIntAverageTime(IntAverage, tbTime1);
        }

        private void ButtonExe2_Click(object sender, RoutedEventArgs e)
        {
            //MyIntAverageTime(IntAverageSimd, tbTime2);
            MyIntAverageTime(IntAverageSimdCastLong, tbTime2);
        }

        private void ButtonExe3_Click(object sender, RoutedEventArgs e)
        {
            //MyByteAverageTime(ByteAverage, tbTime3);
            MyIntAverageTime(IntIntAverageSimdIncorrect, tbTime3);
        }

        private void ButtonExe4_Click(object sender, RoutedEventArgs e)
        {
            //MyByteAverageTime(ByteAverageSimd, tbTime4);
            int thread = 8;
            int count = ELEMENT_COUNT / thread;
            List<int[]> aryList = new List<int[]>();
            for (int i = 0; i < thread; i++)
            {
                aryList.Add(MakeIntArray(count, 0, 255));
            }
            MyIntAverageTime(MultiArrayMultiSimd, tbTime4, aryList);
        }

        private void ButtonExe5_Click(object sender, RoutedEventArgs e)
        {
            MyIntAverageTime(IntAverageMutli, tbTime5);
        }

        private void ButtonExe6_Click(object sender, RoutedEventArgs e)
        {
            MyIntAverageTime(IntAverageMulti3, tbTime6);
        }

        private void ButtonExe7_Click(object sender, RoutedEventArgs e)
        {
            MyIntAverageTime(IntAverageMultiSimd, tbTime7);
        }

        private void ButtonExe8_Click(object sender, RoutedEventArgs e)
        {
            MyIntAverageTime(IntAverageMultiSimd2, tbTime8);
        }

        private void ButtonExe9_Click(object sender, RoutedEventArgs e)
        {
            MyIntAverageTime(IntIntAverageMultiSimdIncorrect, tbTime9);
        }
    }


}
