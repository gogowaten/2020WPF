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

            //var v1 = new Vector<int>(10);
            //var v2 = new Vector<int>(Enumerable.Range(20, 8).ToArray());
            //var add = System.Numerics.Vector.Add(v1, v2);//足し算
            //var abs = System.Numerics.Vector.Abs(v1);//絶対値
            //var andnot = System.Numerics.Vector.AndNot(v1, v2);
            //var bitwiseand = System.Numerics.Vector.BitwiseAnd(v1, v2);
            //var bitwiseor = System.Numerics.Vector.BitwiseOr(v1, v2);
            //var divide = System.Numerics.Vector.Divide(v1, v2);//割り算
            //var dot = System.Numerics.Vector.Dot(v1, v2);//掛け算して足し算
            //var greaterthan = System.Numerics.Vector.GreaterThan(v1, v2);
            //var greaterthanall = System.Numerics.Vector.GreaterThanAll(v1, v2);
            //var lessthan = System.Numerics.Vector.LessThan(v1, v2);
            //var multiply = System.Numerics.Vector.Multiply(v1, v2);//掛け算
            //var negati = System.Numerics.Vector.Negate(v1);//符号反転
            //var squareroot = System.Numerics.Vector.SquareRoot(v1);//平方根
            //var subtract = System.Numerics.Vector.Subtract(v1, v2);//引き算
            //var xor = System.Numerics.Vector.Xor(v1, v2);
            //var neko = add;



            //var ia = new int[] { 10, 11, 12, 13, 14, 15, 16, 17 };
            //var ib = Enumerable.Repeat(int.MaxValue, iCount).ToArray();
            //var va = new Vector<int>(ia);
            //var vb = new Vector<int>(ib);
            //var vv = System.Numerics.Vector.Add(va, vb);
            //var ii = new List<int>();
            //for (int i = 0; i < iCount; i++)
            //{
            //    ii.Add(vv[i]);
            //}


            var iCount = Vector<int>.Count;//8、Vectorで一度に計算できる個数
            var lCount = Vector<long>.Count;//4、Vectorで一度に計算できる個数
            var ia = new int[] { 10, 11, 12, 13, 14, 15, 16, 17 };
            var lTemp = new long[ia.Length];
            ia.CopyTo(lTemp, 0);
            var lva = new Vector<long>(lTemp);
            var ib = Enumerable.Repeat(int.MaxValue, lCount).ToArray();
            ib.CopyTo(lTemp, 0);
            var lvb = new Vector<long>(lTemp);
            
            var vv = System.Numerics.Vector.Add(lva, lvb);
            var ii = new List<long>();
            for (int i = 0; i < lCount; i++)
            {
                ii.Add(vv[i]);
            }























            var inu = 1;

            //int[] ri = new int[10000000];
            //var r = new Random();

            //for (int i = 0; i < ri.Length; i++)
            //{
            //    ri[i] = r.Next(int.MinValue, int.MaxValue);
            //}
            //int max = int.MinValue;
            //var sw = new Stopwatch();
            //sw.Start();
            //for (int i = 0; i < 10; i++)
            //{

            //    max = MyVectorMax(ri);
            //}
            //sw.Stop();
            //MessageBox.Show($"{sw.ElapsedMilliseconds}ミリ秒 MAX={max}");

            //sw.Restart();
            //max = MyMax(ri);
            //sw.Stop();
            //MessageBox.Show($"{sw.ElapsedMilliseconds}ミリ秒 MAX={max}");




            //var simdLength = Vector<byte>.Count;//32
            //var rAArray = ZRandomByte(simdLength);
            //var rBArray = ZRandomByte(simdLength);

            //var bba = new Vector<byte>(rAArray);
            //var bbb = new Vector<byte>(rBArray);
            //Vector<byte> ii = System.Numerics.Vector.Min(bba, bbb);
            //var vMin = 255;
            //for (int i = 0; i < simdLength; i++)
            //{
            //    if (vMin > ii[i]) vMin = ii[i];
            //}

            //byte aMin = rAArray.Min();
            //byte bMin = rBArray.Min();
            //byte min = Math.Min(aMin, bMin);




            string imagePath = @"D:\ブログ用\チェック用2\WP_20200111_09_25_36_Pro_2020_01_11_午後わてん.jpg";
            imagePath = @"D:\ブログ用\テスト用画像\1ピクセルだけ半透明_.png";
            imagePath = @"D:\ブログ用\テスト用画像\不透明と半透明.png";
            imagePath = @"D:\ブログ用\テスト用画像\テスト結果用\NEC_1354_2018_02_25_午後わてん.jpg_.png";
            imagePath = @"E:\オレ\携帯\2019スマホ\WP_20200125_12_28_46_Pro.jpg";
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

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            var neko = new Cube4(OriginPixels);
            sw.Stop();
            MessageBox.Show(sw.ElapsedMilliseconds.ToString());

        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            var rgbArray = MakeRgbArray(OriginPixels);
            sw.Stop();
            MessageBox.Show(sw.ElapsedMilliseconds.ToString());

            sw.Restart();
            var neko = new Cube5(rgbArray.red, rgbArray.green, rgbArray.blue);
            sw.Stop();
            MessageBox.Show(sw.ElapsedMilliseconds.ToString());

            sw.Restart();
            var inu = neko.Bunkatu(18);
            sw.Stop();
            MessageBox.Show(sw.ElapsedMilliseconds.ToString());

        }
        private (byte[] red, byte[] green, byte[] blue) MakeRgbArray(byte[] pixels)
        {
            var ColorCount = pixels.Length / 4;
            var redArray = new byte[ColorCount];
            var greenArray = new byte[ColorCount];
            var blueArray = new byte[ColorCount];
            for (int i = 0; i < ColorCount; i++)
            {
                redArray[i] = pixels[i * 4 + 2];
                greenArray[i] = pixels[i * 4 + 1];
                blueArray[i] = pixels[i * 4];
            }

            return (redArray, greenArray, blueArray);
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


    //最速
    public class Cube3
    {
        public byte MaxR { get; private set; }
        public byte MaxG { get; private set; }
        public byte MaxB { get; private set; }
        public byte MinR { get; private set; }
        public byte MinG { get; private set; }
        public byte MinB { get; private set; }
        private byte[] Pixels;
        private List<Cube3> Cubes;

        /// <summary>
        /// PixelFormats.Bgra32だけが対象
        /// </summary>
        /// <param name="pixels"></param>
        public Cube3(byte[] pixels)
        {
            Cubes = new List<Cube3>();
            Pixels = pixels;
            var simdLength = Vector<byte>.Count;
            var vMaxR = new Vector<byte>(0);
            var vMaxG = new Vector<byte>(0);
            var vMaxB = new Vector<byte>(0);
            var vMinR = new Vector<byte>(255);
            var vMinG = new Vector<byte>(255);
            var vMinB = new Vector<byte>(byte.MaxValue);

            var r = new byte[simdLength];
            var g = new byte[simdLength];
            var b = new byte[simdLength];

            Vector<byte> vr; Vector<byte> vg; Vector<byte> vb;
            int myLast = pixels.Length - simdLength;
            int pixelSimdLength = simdLength * 4;

            for (int i = 0; i < myLast; i += pixelSimdLength)//bgra * simdLenghtづつ進む
            {
                //simdLenghtに収まる配列を作成
                for (int j = 0; j < simdLength; j++)
                {
                    int jj = j * 4;
                    r[j] = pixels[i + jj + 2];
                    g[j] = pixels[i + jj + 1];
                    b[j] = pixels[i + jj];
                }
                //配列からVector作成
                vr = new Vector<byte>(r);
                vg = new Vector<byte>(g);
                vb = new Vector<byte>(b);
                //比較
                vMaxR = System.Numerics.Vector.Max(vMaxR, vr);
                vMaxG = System.Numerics.Vector.Max(vMaxG, vg);
                vMaxB = System.Numerics.Vector.Max(vMaxB, vb);
                vMinR = System.Numerics.Vector.Min(vMinR, vr);
                vMinG = System.Numerics.Vector.Min(vMinG, vg);
                vMinB = System.Numerics.Vector.Min(vMinB, vb);
            }


            MaxR = 0; MaxG = 0; MaxB = 0;
            MinR = 255; MinG = 255; MinB = 255;
            for (int i = 0; i < simdLength; i++)
            {
                if (MaxR < vMaxR[i]) MaxR = vMaxR[i];
                if (MaxG < vMaxG[i]) MaxG = vMaxG[i];
                if (MaxB < vMaxB[i]) MaxB = vMaxB[i];
                if (MinR > vMinR[i]) MinR = vMinR[i];
                if (MinG > vMinG[i]) MinG = vMinG[i];
                if (MinB > vMinB[i]) MinB = vMinB[i];
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

        private int GetSideLength(Cube3 cube)
        {
            int rLength = cube.MaxR - cube.MinR;
            int gLength = cube.MaxG - cube.MinG;
            int bLength = cube.MaxB - cube.MinB;

            if (rLength >= gLength)
            {
                if (rLength >= bLength) { return rLength; }
                else { return bLength; }
            }
            else if (gLength >= bLength) { return gLength; }
            else { return bLength; }
        }
        //最大辺を持つCubeを選択、最大辺の色も取得
        private Cube3 SelectCube()
        {
            int[] sides = new int[Cubes.Count];
            for (int i = 0; i < Cubes.Count; i++)
            {
                sides[i] = GetSideLength(Cubes[i]);
            }
            return Cubes[sides.Max()];
        }

        //Cubeリストから選択、分割、選択Cubeをリストから削除、分割したCubeをリストに追加
        //最大辺の中間で分割
        private void Bunkatu(Cube3 cube, int count)
        {
            int rLength = MaxR - MinR;
            int gLength = MaxG - MinG;
            int bLength = MaxB - MinB;
            byte[] cPixels = cube.Pixels;
            int colorCount = 1;
            //中間は切り捨てで取得、判定は以下(より大きい)
            int mid;
            var lowList = new List<byte>();
            var HiList = new List<byte>();

            if (rLength >= gLength)
            {
                if (rLength >= bLength)
                {
                    //aka
                    mid = MaxR - MinR;//中間は切り捨てで取得
                    for (int i = 0; i < cPixels.Length; i += 4)
                    {
                        if (cPixels[i + 2] > mid)
                        {
                            HiList.Add(cPixels[i]);
                            HiList.Add(cPixels[i + 1]);
                            HiList.Add(cPixels[i + 2]);
                            HiList.Add(cPixels[i + 3]);
                        }
                        else
                        {
                            lowList.Add(cPixels[i]);
                            lowList.Add(cPixels[i + 1]);
                            lowList.Add(cPixels[i + 2]);
                            lowList.Add(cPixels[i + 3]);
                        }
                    }
                }
                //分割できなかった場合
                if (HiList.Count == 0 && lowList.Count == 0) { }
                //分割できたら元のCubeをリストから削除、分割Cubeをリストに追加
                else
                {
                    Cubes.Remove(cube);
                    Cubes.Add(new Cube3(HiList.ToArray()));
                    Cubes.Add(new Cube3(lowList.ToArray()));
                    count++;
                    if (colorCount < count)
                    {
                        //次の分割
                    }
                }
            }
            else if (gLength >= bLength)
            {

            }
            else
            {

            }
        }

    }



    public class Cube4
    {
        public byte MaxR { get; private set; }
        public byte MaxG { get; private set; }
        public byte MaxB { get; private set; }
        public byte MinR { get; private set; }
        public byte MinG { get; private set; }
        public byte MinB { get; private set; }


        /// <summary>
        /// PixelFormats.Bgra32だけが対象
        /// Vector使用
        /// rgbごとの配列を作成(このぶんメモリを余計に使うはず)しておいて、Vector作成にはインデックスを指定
        /// cuve3より遅くなった…
        /// </summary>
        /// <param name="pixels"></param>
        public Cube4(byte[] pixels)
        {
            int cLength = pixels.Length / 4;
            byte[] rA = new byte[cLength];
            byte[] gA = new byte[cLength];
            byte[] bA = new byte[cLength];
            //rgbごとの配列を作成
            for (int i = 0; i < cLength; i++)
            {
                rA[i] = pixels[i * 4 + 2];
                gA[i] = pixels[i * 4 + 1];
                bA[i] = pixels[i * 4];
            }
            //int p = 0;
            //for (int i = 0; i < pixels.Length; i += 4)
            //{
            //    rA[p] = pixels[i + 2];
            //    gA[p] = pixels[i + 1];
            //    bA[p] = pixels[i];
            //    p++;
            //}

            var simdLength = Vector<byte>.Count;
            var vMaxR = new Vector<byte>(byte.MinValue);
            var vMaxG = new Vector<byte>(byte.MinValue);
            var vMaxB = new Vector<byte>(byte.MinValue);
            var vMinR = new Vector<byte>(byte.MaxValue);
            var vMinG = new Vector<byte>(byte.MaxValue);
            var vMinB = new Vector<byte>(byte.MaxValue);


            int myLast = cLength - simdLength;

            for (int i = 0; i < myLast; i += simdLength)
            {
                vMaxR = System.Numerics.Vector.Max(vMaxR, new Vector<byte>(rA, i));
                vMaxG = System.Numerics.Vector.Max(vMaxG, new Vector<byte>(gA, i));
                vMaxB = System.Numerics.Vector.Max(vMaxB, new Vector<byte>(bA, i));
                vMinR = System.Numerics.Vector.Min(vMinR, new Vector<byte>(rA, i));
                vMinG = System.Numerics.Vector.Min(vMinG, new Vector<byte>(gA, i));
                vMinB = System.Numerics.Vector.Min(vMinB, new Vector<byte>(bA, i));
            }

            //ここだけ並列処理にしたけど逆に遅くなった
            //Parallel.Invoke(() => { MaxR = GetMax(rA); MaxG= GetMax(gA); MaxB = GetMax(bA); });


            MaxR = 0; MaxG = 0; MaxB = 0;
            MinR = 255; MinG = 255; MinB = 255;
            for (int i = 0; i < simdLength; i++)
            {
                if (MaxR < vMaxR[i]) MaxR = vMaxR[i];
                if (MaxG < vMaxG[i]) MaxG = vMaxG[i];
                if (MaxB < vMaxB[i]) MaxB = vMaxB[i];
                if (MinR > vMinR[i]) MinR = vMinR[i];
                if (MinG > vMinG[i]) MinG = vMinG[i];
                if (MinB > vMinB[i]) MinB = vMinB[i];
            }

            myLast = pixels.Length - simdLength;
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
        //private byte GetMax(byte[] vs)
        //{
        //    int simdLenght = Vector<byte>.Count;
        //    int myLast = vs.Length - simdLenght;
        //    var vMax = new Vector<byte>(byte.MinValue);
        //    for (int i = 0; i < myLast; i += simdLenght)
        //    {
        //        vMax = System.Numerics.Vector.Max(vMax, new Vector<byte>(vs, i));
        //    }
        //    byte max = byte.MinValue;
        //    for (int i = 0; i < simdLenght; i++)
        //    { if (max < vMax[i]) max = vMax[i]; }
        //    for (int i = myLast; i < vs.Length; i++)
        //    {
        //        if (max < vs[i]) max = vs[i];
        //    }
        //    return max;
        //}
    }



    public class Cube5
    {
        private enum MaxSideColor
        {
            Red = 2, Green = 1, Blue = 0, None = -1
        }


        #region sonota
        public byte MaxR { get; private set; }
        public byte MaxG { get; private set; }
        public byte MaxB { get; private set; }
        public byte MinR { get; private set; }
        public byte MinG { get; private set; }
        public byte MinB { get; private set; }
        public byte[] RedArray { get; private set; }
        public byte[] GreArray { get; private set; }
        public byte[] BluArray { get; private set; }
        public int ColorCount { get; private set; }
        public List<Cube5> Cubes { get; private set; }
        #endregion
        private MaxSideColor SpritKeyColor;//最大辺長の色(RGBのどれか)
        private int MaxSideLength;//最大辺長

        public Cube5(byte[] redArray, byte[] greenArray, byte[] blueArray)
        {
            ColorCount = redArray.Length;
            Initialize(redArray, greenArray, blueArray);
        }

        public Cube5(byte[] pixels)
        {
            ColorCount = pixels.Length / 4;
            var redArray = new byte[ColorCount];
            var greenArray = new byte[ColorCount];
            var blueArray = new byte[ColorCount];
            for (int i = 0; i < ColorCount; i++)
            {
                redArray[i] = pixels[i * 4 + 2];
                greenArray[i] = pixels[i * 4 + 1];
                blueArray[i] = pixels[i * 4];
            }

            Initialize(redArray, greenArray, blueArray);
        }
        private void Initialize(byte[] redArray, byte[] greenArray, byte[] blueArray)
        {
            RedArray = redArray;
            GreArray = greenArray;
            BluArray = blueArray;
            SetMinMax();
            Cubes = new List<Cube5>();
            Cubes.Add(this);
            SetSelectKey();
        }
        private void SetMinMax()
        {
            var simdLength = Vector<byte>.Count;
            if (simdLength > ColorCount)
            {
                SetMinMax2();
                return;
            }

            var vMaxR = new Vector<byte>(byte.MinValue);
            var vMaxG = new Vector<byte>(byte.MinValue);
            var vMaxB = new Vector<byte>(byte.MinValue);
            var vMinR = new Vector<byte>(byte.MaxValue);
            var vMinG = new Vector<byte>(byte.MaxValue);
            var vMinB = new Vector<byte>(byte.MaxValue);

            int myLast = ColorCount - simdLength;
            for (int i = 0; i < myLast; i += simdLength)
            {
                vMaxR = System.Numerics.Vector.Max(vMaxR, new Vector<byte>(RedArray, i));
                vMaxG = System.Numerics.Vector.Max(vMaxG, new Vector<byte>(GreArray, i));
                vMaxB = System.Numerics.Vector.Max(vMaxB, new Vector<byte>(BluArray, i));
                vMinR = System.Numerics.Vector.Min(vMinR, new Vector<byte>(RedArray, i));
                vMinG = System.Numerics.Vector.Min(vMinG, new Vector<byte>(GreArray, i));
                vMinB = System.Numerics.Vector.Min(vMinB, new Vector<byte>(BluArray, i));
            }

            MaxR = vMaxR[0];
            MaxG = vMaxG[0];
            MaxB = vMaxB[0];
            MinR = vMinR[0];
            MinG = vMinG[0];
            MinB = vMinB[0];
            for (int i = 1; i < simdLength; i++)
            {
                if (MaxR < vMaxR[i]) MaxR = vMaxR[i];
                if (MaxG < vMaxG[i]) MaxG = vMaxG[i];
                if (MaxB < vMaxB[i]) MaxB = vMaxB[i];
                if (MinR > vMinR[i]) MinR = vMinR[i];
                if (MinG > vMinG[i]) MinG = vMinG[i];
                if (MinB > vMinB[i]) MinB = vMinB[i];
            }

            for (int i = myLast; i < ColorCount; i++)
            {
                MaxR = Math.Max(MaxR, RedArray[i]);
                MaxG = Math.Max(MaxG, GreArray[i]);
                MaxB = Math.Max(MaxB, BluArray[i]);
                MinR = Math.Min(MinR, RedArray[i]);
                MinG = Math.Min(MinG, GreArray[i]);
                MinB = Math.Min(MinB, BluArray[i]);
            }
        }
        private void SetMinMax2()
        {
            for (int i = 0; i < ColorCount; i++)
            {
                MaxR = Math.Max(MaxR, RedArray[i]);
                MaxG = Math.Max(MaxG, GreArray[i]);
                MaxB = Math.Max(MaxB, BluArray[i]);
                MinR = Math.Min(MinR, RedArray[i]);
                MinG = Math.Min(MinG, GreArray[i]);
                MinB = Math.Min(MinB, BluArray[i]);
            }
        }

        public List<Cube5> Bunkatu(int spritCount)
        {
            Cube5 selectedCube = Select();
            List<byte> vs1R = new List<byte>();
            List<byte> vs1G = new List<byte>();
            List<byte> vs1B = new List<byte>();
            List<byte> vs2R = new List<byte>();
            List<byte> vs2G = new List<byte>();
            List<byte> vs2B = new List<byte>();
            int mid;//中間値は切り捨てint

            byte[] keyArray;
            if (selectedCube.SpritKeyColor == MaxSideColor.Red)
            {
                keyArray = selectedCube.RedArray;
                mid = (selectedCube.MaxR + selectedCube.MinR) / 2;
            }
            else if (selectedCube.SpritKeyColor == MaxSideColor.Green)
            {
                keyArray = selectedCube.GreArray;
                mid = (selectedCube.MaxG + selectedCube.MinG) / 2;
            }
            else
            {
                keyArray = selectedCube.BluArray;
                mid = (selectedCube.MaxB + selectedCube.MinB) / 2;
            }
            //中間値で分割
            for (int i = 0; i < selectedCube.ColorCount; i++)
            {
                if (mid > keyArray[i])
                {
                    vs1R.Add(selectedCube.RedArray[i]);
                    vs1G.Add(selectedCube.GreArray[i]);
                    vs1B.Add(selectedCube.BluArray[i]);

                }
                else
                {
                    vs2R.Add(selectedCube.RedArray[i]);
                    vs2G.Add(selectedCube.GreArray[i]);
                    vs2B.Add(selectedCube.BluArray[i]);
                }
            }
            //2つに分割できなかったら、今のCubeを諦めて別のCubeで試行
            if (vs1B.Count == 0 || vs2B.Count == 0)
            {

                return Cubes;
            }
            //2つに分割できたら
            else
            {
                //分割した元Cubeはリストから削除、元Cubeから分割した2つのCubeをリストに追加
                Cubes.Remove(selectedCube);
                var c1 = new Cube5(vs1R.ToArray(), vs1G.ToArray(), vs1B.ToArray());
                var c2 = new Cube5(vs2R.ToArray(), vs2G.ToArray(), vs2B.ToArray());
                Cubes.Add(c1);
                Cubes.Add(c2);
                //指定分割数になるまで繰り返す
                if (Cubes.Count < spritCount) Bunkatu(spritCount);
            }
            return Cubes;
        }

        //最大辺
        private Cube5 Select()
        {
            Cube5 selectedCube = Cubes[0];
            if (Cubes.Count == 1) return selectedCube;

            for (int i = 1; i < Cubes.Count; i++)
            {
                if (selectedCube.MaxSideLength < Cubes[i].MaxSideLength)
                {
                    selectedCube = Cubes[i];
                }
            }
            return selectedCube;
        }

        private void SetSelectKey()
        {
            int r = MaxR - MinR;
            int g = MaxG - MinG;
            int b = MaxB - MinB;
            if (r > g)
            {
                if (r > b)
                {
                    MaxSideLength = r;
                    SpritKeyColor = MaxSideColor.Red;
                }
                else
                {
                    MaxSideLength = b;
                    SpritKeyColor = MaxSideColor.Blue;
                }
            }
            else if (g > b)
            {
                MaxSideLength = g;
                SpritKeyColor = MaxSideColor.Green;
            }

            else
            {
                MaxSideLength = b;
                SpritKeyColor = MaxSideColor.Blue;
            }
        }
    }
}
