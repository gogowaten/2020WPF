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
using System.Diagnostics;
using System.Numerics;


namespace _20200205_int配列の値の合計
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int[] MyIntAry;
        private long[] MyLongAry;
        const int LOOP_COUNT = 100;
        const int ELEMENT_COUNT = 1_000_000;
        

        public MainWindow()
        {
            InitializeComponent();

            MyTextBlock.Text = $"配列の値の合計、要素数{ELEMENT_COUNT.ToString("N0")}の合計を{LOOP_COUNT}回求める処理時間";
            MyTextBlockVectorCount.Text = $"Vector<long>.Count = {Vector<long>.Count}";
            MyIntAry = Enumerable.Range(1, ELEMENT_COUNT).ToArray();
            MyLongAry = new long[MyIntAry.Length];
            MyIntAry.CopyTo(MyLongAry, 0);

            

            Button1.Click += (s, e) => MyExe(TestFor, Tb1);
            Button2.Click += (s, e) => MyExe(TestForeach, Tb2);
            Button3.Click += (s, e) => MyExe(TestLinqSum, Tb3);
            Button4.Click += (s, e) => MyExe(TestVectorAddCopyToAll, Tb4);
            Button5.Click += (s, e) => MyExe(TestVectorAddEach, Tb5);
            Button6.Click += (s, e) => MyExe(TestVectorAddEachCount4, Tb6);
            Button7.Click += (s, e) => MyExe(TestVectorAddEachCopyToLinq, Tb7);
            Button8.Click += (s, e) => MyExe(TestVectorAddCopyToEachArraySegment, Tb8);
            Button9.Click += (s, e) => MyExe(TestVectorAddCopyToEachArraySegmentSlice, Tb9);
            Button10.Click += (s, e) => MyExe(TestVectorAddEachCopyToArraySpan, Tb10);
            Button11.Click += (s, e) => MyExe(TestVectorAddEachCopyToArraySpan2, Tb11);
            Button12.Click += (s, e) => MyExe(TestVectorAddEachCopyToArrayBlockCopy, Tb12);

            Button13.Click += (s, e) => MyExe(TestLongLinqSum, Tb13);
            Button14.Click += (s, e) => MyExe(TestLongVectorAdd, Tb14);
            Button15.Click += (s, e) => MyExe(TestLongVectorAddSpan, Tb15);
            Button16.Click += (s, e) => MyExe(TestLongVectorAddReadOnlySpan, Tb16);
            Button17.Click += (s, e) => MyExe(TestLongVectorAdd2, Tb17);
            //Button18.Click += (s, e) => MyExe(Test38, Tb18);
            //Button19.Click += (s, e) => MyExe(Test39, Tb19);




        }

        //for
        private long TestFor(int[] Ary)
        {
            long total = 0;
            for (int i = 0; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        //foreach
        private long TestForeach(int[] Ary)
        {
            long total = 0;
            foreach (var item in Ary)
            {
                total += item;
            }
            return total;
        }

        //LINQのSum
        //        LINQの処理中に使うメモリを節約するには？［C#、VB］：.NET TIPS - ＠IT
        //https://www.atmarkit.co.jp/ait/articles/1409/24/news105.html
        private long TestLinqSum(int[] Ary)
        {
            //return Ary.Sum();//エラー、オーバーフロー
            return Ary.Sum(i => (long)i);
            //return Ary.Sum((int i) => { return (long)i; });//↑と同じ
        }

        //System.Numerics.VectorのAdd
        //int型配列からlong型配列には一括で変換
        private long TestVectorAddCopyToAll(int[] Ary)
        {
            var longAry = new long[Ary.Length];
            Ary.CopyTo(longAry, 0);
            int simdLength = Vector<long>.Count;
            var v = new Vector<long>(longAry);
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                v = System.Numerics.Vector.Add(v, new Vector<long>(longAry, i));
            }
            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        //int型配列からlong型配列には計算する分だけをその都度変換
        //一括変換よりメモリ使用量が小さくて済む
        private long TestVectorAddEach(int[] Ary)
        {
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            var longAry = new long[simdLength];
            for (int i = 0; i < simdLength; i++)
            {
                longAry[i] = Ary[i];
            }
            var v = new Vector<long>(longAry);

            for (int j = simdLength; j < lastIndex; j += simdLength)
            {
                for (int i = 0; i < simdLength; i++)
                {
                    longAry[i] = Ary[j + i];
                }
                v = System.Numerics.Vector.Add(v, new Vector<long>(longAry));
            }

            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        //決め打ち
        //int型配列からlong型配列には計算する分だけをその都度変換
        //一括変換よりメモリ使用量が小さくて済む
        //変換する要素数は4個で決め打ち
        private long TestVectorAddEachCount4(int[] Ary)
        {
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % 4);
            var longAry = new long[4];
            longAry[0] = Ary[0];
            longAry[1] = Ary[1];
            longAry[2] = Ary[2];
            longAry[3] = Ary[3];
            var v = new Vector<long>(longAry);

            for (int i = 4; i < lastIndex; i += simdLength)
            {
                longAry[0] = Ary[i];
                longAry[1] = Ary[i + 1];
                longAry[2] = Ary[i + 2];
                longAry[3] = Ary[i + 3];

                v = System.Numerics.Vector.Add(v, new Vector<long>(longAry));
            }

            long total = 0;
            for (int i = 0; i < 4; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        //LINQ、最遅
        //int型配列からlong型配列には計算する分だけをその都度変換
        //一括変換よりメモリ使用量が小さくて済む
        //LINQのSkipとTakeを使って変換範囲を取り出す
        private long TestVectorAddEachCopyToLinq(int[] Ary)
        {
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);

            long[] longAry = new long[simdLength];
            Ary.Take(simdLength).ToArray().CopyTo(longAry, 0);
            var v = new Vector<long>(longAry);
            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                Ary.Skip(i).Take(simdLength).ToArray().CopyTo(longAry, 0);
                v = System.Numerics.Vector.Add(v, new Vector<long>(longAry, 0));
            }

            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        //ArraySegment
        //int型配列からlong型配列には計算する分だけをその都度変換
        //一括変換よりメモリ使用量が小さくて済む
        //ArraySegmentを使って変換範囲を指定する
        private long TestVectorAddCopyToEachArraySegment(int[] Ary)
        {
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            long[] longAry = new long[simdLength];
            new ArraySegment<int>(Ary, 0, simdLength).ToArray().CopyTo(longAry, 0);
            var v = new Vector<long>(longAry);

            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                new ArraySegment<int>(Ary, i, simdLength).ToArray().CopyTo(longAry, 0);
                v = System.Numerics.Vector.Add(v, new Vector<long>(longAry, 0));
            }

            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        //ArraySegment
        //int型配列からlong型配列には計算する分だけをその都度変換
        //一括変換よりメモリ使用量が小さくて済む
        //ArraySegmentのSliceを使って変換範囲を指定する
        private long TestVectorAddCopyToEachArraySegmentSlice(int[] Ary)
        {
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            long[] longAry = new long[simdLength];
            var segment = new ArraySegment<int>(Ary);
            segment.Slice(0, simdLength).ToArray().CopyTo(longAry, 0);
            var v = new Vector<long>(longAry);

            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                segment.Slice(i, simdLength).ToArray().CopyTo(longAry, 0);
                v = System.Numerics.Vector.Add(v, new Vector<long>(longAry, 0));
            }

            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        //Span        
        //int型配列からlong型配列には計算する分だけをその都度変換
        //一括変換よりメモリ使用量が小さくて済む
        //SpanのSliceを使って変換範囲を指定する
        private long TestVectorAddEachCopyToArraySpan(int[] Ary)
        {
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            long[] longAry = new long[simdLength];
            new Span<int>(Ary).Slice(0, simdLength).ToArray().CopyTo(longAry, 0);
            var v = new Vector<long>(longAry);

            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                new Span<int>(Ary).Slice(i, simdLength).ToArray().CopyTo(longAry, 0);
                v = System.Numerics.Vector.Add(v, new Vector<long>(longAry, 0));
            }

            long total = 0;
            for (int i = 0; i < simdLength; i++) total += v[i];
            for (int i = lastIndex; i < Ary.Length; i++) total += Ary[i];
            return total;
        }

        //Span        
        //int型配列からlong型配列には計算する分だけをその都度変換
        //一括変換よりメモリ使用量が小さくて済む
        //SpanのSliceを使って変換範囲を指定する
        private long TestVectorAddEachCopyToArraySpan2(int[] Ary)
        {
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            long[] longAry = new long[simdLength];
            var sp = new Span<int>(Ary);
            sp.Slice(0, simdLength).ToArray().CopyTo(longAry, 0);
            var v = new Vector<long>(longAry);

            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                sp.Slice(i, simdLength).ToArray().CopyTo(longAry, 0);
                v = System.Numerics.Vector.Add(v, new Vector<long>(longAry, 0));
            }

            long total = 0;
            for (int i = 0; i < simdLength; i++) total += v[i];
            for (int i = lastIndex; i < Ary.Length; i++) total += Ary[i];
            return total;
        }

        //Buffer.BlockCopy
        //int型配列からlong型配列には計算する分だけをその都度変換
        //一括変換よりメモリ使用量が小さくて済む
        //Buffer.BlockCopyを使って変換範囲を指定する
        private long TestVectorAddEachCopyToArrayBlockCopy(int[] Ary)
        {
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            long[] longAry = new long[simdLength];
            int[] iAry = new int[simdLength];
            int typeSize = System.Runtime.InteropServices.Marshal.SizeOf(Ary.GetType().GetElementType());
            Buffer.BlockCopy(Ary, 0, iAry, 0, simdLength * typeSize);
            iAry.CopyTo(longAry, 0);
            var v = new Vector<long>(longAry);
            int blockSize = simdLength * typeSize;
            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                Buffer.BlockCopy(Ary, i * typeSize, iAry, 0, blockSize);
                iAry.CopyTo(longAry, 0);
                v = System.Numerics.Vector.Add(v, new Vector<long>(longAry, 0));
            }

            long total = 0;
            for (int i = 0; i < simdLength; i++) total += v[i];
            for (int i = lastIndex; i < Ary.Length; i++) total += Ary[i];
            return total;
        }


        #region long型配列
        //
        //intからlongへのキャストや配列作成する必要がないので速い
        //条件：すべての値をlong型で合計したときにオーバーフローする可能性がない
        private long TestLongLinqSum(long[] Ary)
        {
            return Ary.Sum();
        }


        private long TestLongVectorAdd(long[] Ary)
        {
            var v = new Vector<long>(Ary);
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                v = System.Numerics.Vector.Add(v, new Vector<long>(Ary, i));
            }
            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        private long TestLongVectorAddSpan(long[] Ary)
        {
            var sp = new Span<long>(MyLongAry);
            var v = new Vector<long>(sp);
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                v = System.Numerics.Vector.Add(v, new Vector<long>(sp.Slice(i, simdLength)));
            }
            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        private long TestLongVectorAddReadOnlySpan(long[] Ary)
        {
            var sp = new ReadOnlySpan<long>(MyLongAry);
            var v = new Vector<long>(sp);
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
                v = System.Numerics.Vector.Add(v, new Vector<long>(sp.Slice(i, simdLength)));
            }
            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        private long TestLongVectorAdd2(long[] Ary)
        {
            var v = new Vector<long>(Ary);
            int simdLength = Vector<long>.Count;
            int lastIndex = Ary.Length - (Ary.Length % simdLength);
            for (int i = simdLength; i < lastIndex; i += simdLength)
            {
//                C# 8.0 の新機能 - C# によるプログラミング入門 | ++C++; // 未確認飛行 C
//https://ufcpp.net/study/csharp/cheatsheet/ap_ver8/
                v = System.Numerics.Vector.Add(v, new Vector<long>(Ary[i..(i + simdLength)]));
            }
            long total = 0;
            for (int i = 0; i < simdLength; i++)
            {
                total += v[i];
            }
            for (int i = lastIndex; i < Ary.Length; i++)
            {
                total += Ary[i];
            }
            return total;
        }

        #endregion


        private void MyExe(Func<int[], long> func, TextBlock tb)
        {
            long total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(MyIntAry);
            }
            sw.Stop();
            tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  合計値：{total}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
        }
        private void MyExe(Func<long[], long> func, TextBlock tb)
        {
            long total = 0;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < LOOP_COUNT; i++)
            {
                total = func(MyLongAry);
            }
            sw.Stop();
            tb.Text = $"処理時間：{sw.Elapsed.TotalSeconds.ToString("00.000")}秒  合計値：{total}  {System.Reflection.RuntimeReflectionExtensions.GetMethodInfo(func).Name}";
        }

    }
}
