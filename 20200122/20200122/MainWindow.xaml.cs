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

namespace _20200122
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] OriginPixels;
        private BitmapSource OriginBitmapSource;

        public MainWindow()
        {
            InitializeComponent();

            int[] ri = new int[10000000];
            var r = new Random();

            for (int i = 0; i < ri.Length; i++)
            {
                ri[i] = r.Next(int.MinValue, int.MaxValue);
            }
            int max = int.MinValue;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 10; i++)
            {

                max = MyVectorMax(ri);
            }
            sw.Stop();
            MessageBox.Show($"{sw.ElapsedMilliseconds}ミリ秒 MAX={max}");

            sw.Restart();
            max = MyMax(ri);
            sw.Stop();
            MessageBox.Show($"{sw.ElapsedMilliseconds}ミリ秒 MAX={max}");


            Vector<ushort> vb = new Vector<ushort>(10);
            Vector<ushort> vg = new Vector<ushort>(20);
            var neko = System.Numerics.Vector.Min(vb, vg);

            var simdLength = Vector<byte>.Count;//32
            var rAArray = ZRandomByte(simdLength);
            var rBArray = ZRandomByte(simdLength);

            var bba = new Vector<byte>(rAArray);
            var bbb = new Vector<byte>(rBArray);
            Vector<byte> ii = System.Numerics.Vector.Min(bba, bbb);
            var vMin = 255;
            for (int i = 0; i < simdLength; i++)
            {
                if (vMin > ii[i]) vMin = ii[i];
            }

            byte aMin = rAArray.Min();
            byte bMin = rBArray.Min();
            byte min = Math.Min(aMin, bMin);




            string imagePath = @"D:\ブログ用\チェック用2\WP_20200111_09_25_36_Pro_2020_01_11_午後わてん.jpg";
            imagePath = @"D:\ブログ用\テスト用画像\1ピクセルだけ半透明_.png";
            //imagePath = @"D:\ブログ用\テスト用画像\不透明と半透明.png";
            (OriginPixels, OriginBitmapSource) = MakeBitmapSourceAndPixelData(imagePath, PixelFormats.Bgra32, 96, 96);
            MyImage.Source = OriginBitmapSource;


        }
        private byte[] ZRandomByte(int count)
        {
            var v = new byte[count];
            var r = new Random();
            r.NextBytes(v);
            return v;
        }

        private int MyVectorMax(int[] intArray)
        {
            int simdLength = Vector<int>.Count;
            int MyLast = intArray.Length - simdLength;
            Vector<int> vMax = new Vector<int>(int.MinValue);
            for (int i = 0; i < MyLast; i += simdLength)
            {
                vMax = System.Numerics.Vector.Max(vMax, new Vector<int>(intArray, i));
            }
            int iMax = int.MinValue;
            for (int i = 0; i < simdLength; i++)
            {
                if (iMax < vMax[i]) iMax = vMax[i];
            }
            return iMax;
        }

        private int MyMax(int[] intArray)
        {
            int iMax = int.MinValue;
            for (int i = 0; i < intArray.Length; i++)
            {
                if (iMax < intArray[i]) iMax = intArray[i];
            }
            return iMax;
        }

        internal static void HWAcceleratedGetStats(ushort[] input, out ushort min, out ushort max, out double average)
        {
            var simdLength = Vector<ushort>.Count;
            var vmin = new Vector<ushort>(ushort.MaxValue);
            var vmax = new Vector<ushort>(ushort.MinValue);
            var i = 0;
            ulong total = 0;
            var lastSafeVectorIndex = input.Length - simdLength;
            for (i = 0; i < lastSafeVectorIndex; i += simdLength)
            {
                total += input[i];
                total += input[i + 1];
                total += input[i + 2];
                total += input[i + 3];
                total += input[i + 4];
                total += input[i + 5];
                total += input[i + 6];
                total += input[i + 7];
                var vector = new Vector<ushort>(input, i);
                vmin = System.Numerics.Vector.Min(vector, vmin);
                vmax = System.Numerics.Vector.Max(vector, vmax);
            }
            min = ushort.MaxValue;
            max = ushort.MinValue;
            for (var j = 0; j < simdLength; ++j)
            {
                min = Math.Min(min, vmin[j]);
                max = Math.Max(max, vmax[j]);
            }
            for (; i < input.Length; ++i)
            {
                min = Math.Min(min, input[i]);
                max = Math.Max(max, input[i]);
                total += input[i];
            }
            average = total / (double)input.Length;
        }


        /// <summary>
        /// 画像ファイルからbitmapと、そのbyte配列を返す、ピクセルフォーマットは指定したものに変換
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsを指定、null指定ならBgra32で作成する</param>
        /// <param name="dpiX">96が基本、指定なしなら元画像と同じにする</param>
        /// <param name="dpiY">96が基本、指定なしなら元画像と同じにする</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource source) MakeBitmapSourceAndPixelData(
            string filePath,
            PixelFormat pixelFormat,
            double dpiX = 0, double dpiY = 0)
        {
            byte[] pixels = null;//PixelData
            BitmapSource source = null;
            if (pixelFormat == null) { pixelFormat = PixelFormats.Bgra32; }
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var frame = BitmapFrame.Create(fs);
                    var tempBitmap = new FormatConvertedBitmap(frame, pixelFormat, null, 0);
                    int w = tempBitmap.PixelWidth;
                    int h = tempBitmap.PixelHeight;
                    int stride = (w * pixelFormat.BitsPerPixel + 7) / 8;
                    pixels = new byte[h * stride];
                    tempBitmap.CopyPixels(pixels, stride, 0);
                    //dpi指定がなければ元の画像と同じdpiにする
                    if (dpiX == 0) { dpiX = frame.DpiX; }
                    if (dpiY == 0) { dpiY = frame.DpiY; }
                    //dpiを指定してBitmapSource作成
                    source = BitmapSource.Create(
                        w, h, dpiX, dpiY,
                        tempBitmap.Format,
                        tempBitmap.Palette, pixels, stride);
                };
            }
            catch (Exception)
            {
            }
            return (pixels, source);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            var neko = new Cube(OriginPixels);
            sw.Stop();
            MessageBox.Show(sw.ElapsedMilliseconds.ToString());

        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            var neko = new Cube2(OriginPixels);
            sw.Stop();
            MessageBox.Show(sw.ElapsedMilliseconds.ToString());

        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            var neko = new Cube3(OriginPixels);
            sw.Stop();
            MessageBox.Show(sw.ElapsedMilliseconds.ToString());

        }
    }

    public class Cube
    {
        public byte MaxR { get; private set; }
        public byte MaxG { get; private set; }
        public byte MaxB { get; private set; }
        public byte MinR { get; private set; }
        public byte MinG { get; private set; }
        public byte MinB { get; private set; }


        /// <summary>
        /// PixelFormats.Bgra32だけが対象
        /// </summary>
        /// <param name="pixels"></param>
        public Cube(byte[] pixels)
        {
            MaxR = 0; MaxG = 0; MaxB = 0;
            MinR = 255; MinG = 255; MinB = 255;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                if (MaxR < pixels[i + 2]) MaxR = pixels[i + 2];
                if (MaxG < pixels[i + 1]) MaxG = pixels[i + 1];
                if (MaxB < pixels[i]) MaxB = pixels[i];
                if (MinR > pixels[i + 2]) MinR = pixels[i + 2];
                if (MinG > pixels[i + 1]) MinG = pixels[i + 1];
                if (MinB > pixels[i]) MinB = pixels[i];


            }
        }
    }



    public class Cube2
    {
        public byte MaxR { get; private set; }
        public byte MaxG { get; private set; }
        public byte MaxB { get; private set; }
        public byte MinR { get; private set; }
        public byte MinG { get; private set; }
        public byte MinB { get; private set; }


        /// <summary>
        /// PixelFormats.Bgra32だけが対象
        /// </summary>
        /// <param name="pixels"></param>
        public Cube2(byte[] pixels)
        {
            MaxR = 0; MaxG = 0; MaxB = 0;
            MinR = 255; MinG = 255; MinB = 255;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                MaxR = Math.Max(MaxR, pixels[i + 2]);
                MaxG = Math.Max(MaxG, pixels[i + 1]);
                MaxB = Math.Max(MaxB, pixels[i]);
                MinR = Math.Min(MinR, pixels[i + 2]);
                MinG = Math.Min(MinG, pixels[i + 1]);
                MinB = Math.Min(MinB, pixels[i]);

            }
        }
    }


    public class Cube3
    {
        public byte MaxR { get; private set; }
        public byte MaxG { get; private set; }
        public byte MaxB { get; private set; }
        public byte MinR { get; private set; }
        public byte MinG { get; private set; }
        public byte MinB { get; private set; }


        /// <summary>
        /// PixelFormats.Bgra32だけが対象
        /// </summary>
        /// <param name="pixels"></param>
        public Cube3(byte[] pixels)
        {
            var simdLength = Vector<byte>.Count;
            var vMaxR = new Vector<byte>(255);
            var vMaxG = new Vector<byte>(255);
            var vMaxB = new Vector<byte>(255);
            var vMinR = new Vector<byte>(0);
            var vMinG = new Vector<byte>(0);
            var vMinB = new Vector<byte>(0);

            var r = new byte[simdLength];
            var g = new byte[simdLength];
            var b = new byte[simdLength];

            Vector<byte> vr; Vector<byte> vg; Vector<byte> vb;

            int myLast = pixels.Length - simdLength;

            for (int i = 0; i < myLast; i += simdLength * 4)
            {
                for (int j = 0; j < simdLength; j++)
                {
                    int jj = j * 4;
                    r[j] = pixels[i + jj + 2];
                    g[j] = pixels[i + jj + 1];
                    b[j] = pixels[i + jj];
                }
                vr = new Vector<byte>(r);
                vg = new Vector<byte>(g);
                vb = new Vector<byte>(b);
                vMaxR = System.Numerics.Vector.Max(vMaxR, vr);
                vMaxG = System.Numerics.Vector.Max(vMaxG, vg);
                vMaxB = System.Numerics.Vector.Max(vMaxB, vb);
                vMinR = System.Numerics.Vector.Min(vMinR, vr);
                vMinG = System.Numerics.Vector.Min(vMinG, vr);
                vMinB = System.Numerics.Vector.Min(vMinB, vr);
            }


            MaxR = 0; MaxG = 0; MaxB = 0;
            MinR = 255; MinG = 255; MinB = 255;
            for (int i = 0; i < simdLength; i++)
            {
                if (MaxR < vMaxR[i]) MaxR = vMaxR[i];
                if (MaxG < vMaxG[i]) MaxG = vMaxG[i];
                if (MaxB < vMaxB[i]) MaxB = vMaxB[i];
                if (MinR < vMinR[i]) MinR = vMinR[i];
                if (MinG < vMinG[i]) MinG = vMinG[i];
                if (MinB < vMinB[i]) MinB = vMinB[i];
            }

            for (int i = myLast; i < pixels.Length; i += 4)
            {
                MaxR = Math.Max(MaxR, pixels[i + 2]);
                MaxG = Math.Max(MaxG, pixels[i + 1]);
                MaxB = Math.Max(MaxB, pixels[i]);
                MinR = Math.Min(MinR, pixels[i + 2]);
                MinG = Math.Min(MinG, pixels[i + 1]);
                MinB = Math.Min(MinB, pixels[i]);

            }
        }
    }

}
