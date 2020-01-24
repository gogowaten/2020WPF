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


namespace _20200124_VectorMinMax
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int[] MyIntArray;
        const int R_MIN = 0;
        const int R_MAX = 10000;//10000
        public MainWindow()
        {
            InitializeComponent();

            MyIntArray = new int[10000];
            ReSetRandomIntArray(R_MIN, R_MAX);

            

        }

        private void ReSetRandomIntArray(int min, int max)
        {
            var r = new Random();
            for (int i = 0; i < MyIntArray.Length; i++)
            {
                MyIntArray[i] = r.Next(min, max);
            }
        }
        private (int min, int max) MyVectorMinMax(int[] intArray)
        {
            int simdLength = Vector<int>.Count;
            int MyLast = intArray.Length - simdLength;
            Vector<int> vMin = new Vector<int>(int.MaxValue);
            Vector<int> vMax = new Vector<int>(int.MinValue);
            Vector<int> temp;
            //Vector<int>.CountごとにVector<int>を作成して、前回のと比較
            //最小と最大のVectorを取得
            for (int i = 0; i < MyLast; i += simdLength)
            {
                temp = new Vector<int>(intArray, i);
                vMin = System.Numerics.Vector.Min(vMin, temp);
                vMax = System.Numerics.Vector.Max(vMax, temp);
            }

            //ベクトルの中で比較
            int iMin = int.MaxValue;
            int iMax = int.MinValue;
            for (int i = 0; i < simdLength; i++)
            {
                if (iMin > vMin[i]) iMin = vMin[i];
                if (iMax < vMax[i]) iMax = vMax[i];
            }

            //Vector<int>.CounTで割り切れなかった余りの要素と比較
            for (int i = MyLast + simdLength; i < intArray.Length; i++)
            {
                if (iMin > intArray[i]) iMin = intArray[i];
                if (iMax < intArray[i]) iMax = intArray[i];
            }
            return (iMin, iMax);
        }

        private (int min, int max) MyForIf(int[] intArray)
        {
            int iMin = int.MaxValue;
            int iMax = int.MinValue;

            for (int i = 0; i < intArray.Length; i++)
            {
                if (iMin > intArray[i]) iMin = intArray[i];
                if (iMax < intArray[i]) iMax = intArray[i];
            }
            return (iMin, iMax);
        }

        private (int min, int max) MyMath(int[] intArray)
        {
            int iMin = int.MaxValue;
            int iMax = int.MinValue;

            for (int i = 0; i < intArray.Length; i++)
            {
                iMin = Math.Min(iMin, intArray[i]);
                iMax = Math.Max(iMax, intArray[i]);
            }
            return (iMin, iMax);
        }

        private (int min, int max) MyMathMT1(int[] intArray)
        {
            //IEnumerable<int> inu = intArray.Select(n => n);
            int threadCount = Environment.ProcessorCount;
            int windowSize = intArray.Length / threadCount;
            (int tMin, int tMax)[] neko = Task.WhenAll(
                Enumerable.Range(0, threadCount)
                .Select(n => Task.Run(() =>
                  {
                      int tMin = 0;
                      int tMax = 0;
                      for (int i = n * windowSize; i < (n + 1) * windowSize; i++)
                      {
                          tMin = Math.Min(tMin, intArray[i]);
                          tMax = Math.Max(tMax, intArray[i]);
                      }
                      return (tMin, tMax);
                  }))).GetAwaiter().GetResult();

            int min = int.MaxValue;
            int mam = int.MinValue;
            for (int i = 0; i < threadCount; i++)
            {
                if (min > neko[i].tMin) min = neko[i].tMin;
                if (mam < neko[i].tMax) mam = neko[i].tMax;
            }

            return (min, mam);
        }

        //1スレッドあたりの範囲の配列をArray.Copyで作ってメソッドに渡す
        private (int min, int max) MyMathMT2(int[] intArray)
        {

            int threadCount = Environment.ProcessorCount;
            int windowSize = intArray.Length / threadCount;
            (int tMin, int tMax)[] neko = Task.WhenAll(
                Enumerable.Range(0, threadCount)
                .Select(n => Task.Run(() =>
                {
                    int[] temp = new int[windowSize];
                    Array.Copy(intArray, n * windowSize, temp, 0, windowSize);
                    return MyMath(temp);
                }
                ))).GetAwaiter().GetResult();

            int min = int.MaxValue;
            int max = int.MinValue;
            for (int i = 0; i < threadCount; i++)
            {
                if (min > neko[i].tMin) min = neko[i].tMin;
                if (max < neko[i].tMax) max = neko[i].tMax;
            }
            return (min, max);
        }

        //1スレッドあたりの範囲の配列をLINQで作ってメソッドに渡す
        private (int min, int max) MyMathMT3(int[] intArray)
        {

            int threadCount = Environment.ProcessorCount;
            int windowSize = intArray.Length / threadCount;
            (int tMin, int tMax)[] neko = Task.WhenAll(
                Enumerable.Range(0, threadCount)
                .Select(n => Task.Run(() =>
                {
                    return MyMath(intArray.Skip(n * windowSize).Take(windowSize).ToArray());
                }
                ))).GetAwaiter().GetResult();

            int min = int.MaxValue;
            int max = int.MinValue;
            for (int i = 0; i < threadCount; i++)
            {
                if (min > neko[i].tMin) min = neko[i].tMin;
                if (max < neko[i].tMax) max = neko[i].tMax;
            }
            return (min, max);
        }


        private (int min, int max) MyVectorMinMaxMT1(int[] intArray)
        {
            int threadCount = Environment.ProcessorCount;
            int windowSize = intArray.Length / threadCount;

            int simdLength = Vector<int>.Count;
            int MyLast = intArray.Length - simdLength;

            (int min, int max)[] neko = Task.WhenAll(Enumerable.Range(0, threadCount)
                .Select(n => Task.Run(() =>
                {
                    return MyVectorMinMax(intArray.Skip(n * windowSize).Take(windowSize).ToArray());
                }))).GetAwaiter().GetResult();

            int iMin = int.MaxValue;
            int iMax = int.MinValue;
            for (int i = 0; i < threadCount; i++)
            {
                if (iMin > neko[i].min) iMin = neko[i].min;
                if (iMax < neko[i].max) iMax = neko[i].max;
            }
            return (iMin, iMax);
        }

        private (int min, int max) MyVectorMinMaxMT2(int[] intArray)
        {
            int threadCount = Environment.ProcessorCount;
            int windowSize = intArray.Length / threadCount;
            int simdLength = Vector<int>.Count;
            Vector<int> temp;
            //var last = new System.Collections.Concurrent.ConcurrentBag<int>();
            int amari = intArray.Length % threadCount;
            int amari2 = amari % simdLength;

            (int iMin, int iMax)[] neko = Task.WhenAll(Enumerable.Range(0, threadCount)
                .Select(n => Task.Run(() =>
                 {
                     Vector<int> vMin = new Vector<int>(int.MaxValue);
                     Vector<int> vMax = new Vector<int>(int.MinValue);
                     int min = int.MaxValue;
                     int max = int.MinValue;

                     int begin = n * windowSize;
                     int end = ((n + 1) * windowSize) - simdLength;

                     for (int i = begin; i < end; i += simdLength)
                     {
                         temp = new Vector<int>(intArray, i);
                         vMin = System.Numerics.Vector.Min(vMin, temp);
                         vMax = System.Numerics.Vector.Max(vMax, temp);
                     }

                     //ベクトルの中で比較
                     for (int j = 0; j < simdLength; j++)
                     {
                         if (min > vMin[j]) min = vMin[j];
                         if (max < vMax[j]) max = vMax[j];
                     }

                     ////Vector<int>.CounTで割り切れなかった余りの要素と比較
                     //for (int j = end+simdLength; j < intArray.Length; j++)
                     //{
                     //    if (iMin > intArray[j]) iMin = intArray[j];
                     //    if (iMax < intArray[j]) iMax = intArray[j];
                     //}

                     return (min, max);
                 }))).GetAwaiter().GetResult();

            int min = neko[0].iMin;
            int max = neko[0].iMax;
            for (int i = 1; i < threadCount; i++)
            {
                if (min > neko[i].iMin) min = neko[i].iMin;
                if (max < neko[i].iMax) max = neko[i].iMax;
            }
            return (min, max);
        }

        //private (int min,int max)MyParallelForIf(int[] intArray)
        //{
        //    int iMin = int.MaxValue;
        //    int iMax = int.MinValue;
        //    Dictionary<int, int> dMin = new Dictionary<int, int>();
        //    var cMin = new System.Collections.Concurrent.ConcurrentDictionary<int, int>();
        //    for (int i = 0; i < intArray.Length; i++)
        //    {
        //        cMin.GetOrAdd(intArray[i], 1);
        //        if (iMin > intArray[i]) iMin = intArray[i];
        //        if (iMax < intArray[i]) iMax = intArray[i];
        //    }
        //    return (iMin, iMax);
        //}





        private void MyTime(Func<int[], (int, int)> func, string funcName)
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                (min, max) = func(MyIntArray);
            }
            sw.Stop();
            MessageBox.Show($"{funcName}\n{sw.ElapsedMilliseconds} ミリ秒\n最小値={min}\n最大値={max}");
        }


        private void Button_ReSetRandomIntArray(object sender, RoutedEventArgs e)
        {
            ReSetRandomIntArray(R_MIN, R_MAX);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyTime(MyVectorMinMax, nameof(MyVectorMinMax));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MyTime(MyMath, nameof(MyMath));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MyTime(MyForIf, nameof(MyForIf));
        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MyTime(MyMathMT1, nameof(MyMathMT1));
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            MyTime(MyMathMT2, nameof(MyMathMT2));
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            MyTime(MyMathMT3, nameof(MyMathMT3));
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            MyTime(MyVectorMinMaxMT1, nameof(MyVectorMinMaxMT1));
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            MyTime(MyVectorMinMaxMT2, nameof(MyVectorMinMaxMT2));
        }
    }
}
