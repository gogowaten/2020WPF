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

namespace _20200318_減色
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] MyOriginPixels;
        BitmapSource MyOriginBitmap;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();

            MyInitialize();


        }
        private void MyInitialize()
        {
            Button1.Click += Button1_Click;

            ComboBoxSelectType.ItemsSource = Enum.GetValues(typeof(SelectType));
            ComboBoxSelectType.SelectedIndex = 0;
            ComboBoxSplitType.ItemsSource = Enum.GetValues(typeof(SplitType));
            ComboBoxSplitType.SelectedIndex = 0;

            string file;
            file = @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん.jpg";
            (MyOriginPixels, MyOriginBitmap) = MakeBitmapSourceAndPixelData(file, PixelFormats.Gray8, 96, 96);


        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            SelectType select = (SelectType)ComboBoxSelectType.SelectedItem;
            SplitType split = (SplitType)ComboBoxSplitType.SelectedItem;
            var cube = new Cube5(MyOriginPixels, GetSelecter(select), GetSplitter(split));
            cube.ExeGensyoku(2);

        }

        private ISelecter GetSelecter(SelectType type)
        {
            ISelecter select;
            if (type == SelectType.SideLong)
                select = new Select_LongSide();
            else if (type == SelectType.ManyPixels)
                select = new Select_ManyPicexls();
            else select = new Select_LongSide();
            return select;
        }
        private ISplitter GetSplitter(SplitType type)
        {
            ISplitter split;
            if (type == SplitType.SideMid)
                split = new Split_SideMid();
            else if (type == SplitType.PixelsMid)
                split = new Split_Mid();
            else split = new Split_SideMid();
            return split;
        }




        #region 画像読み込み
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
    }

    #endregion


    #region
    public interface ISelecter
    {
        public void Calc(Cube5 cube);
        public Cube5 Select(Cube5 cube);
    }
    public class Select_LongSide : ISelecter
    {
        public void Calc(Cube5 cube)
        {
            byte min = byte.MaxValue;
            byte max = byte.MinValue;
            foreach (var item in cube.Pixels)
            {
                if (min > item) min = item;
                if (max < item) max = item;
            }
            cube.Min = min; cube.Max = max;
            cube.IsCalcMinMax = true;
        }

        public Cube5 Select(Cube5 cube)
        {

            Cube5 result = cube.Cubes[0];
            int length = result.Max - result.Min;
            foreach (var item in cube.Cubes)
            {
                if (length < item.Max - item.Min) result = item;
            }
            return result;
        }
    }
    public class Select_ManyPicexls : ISelecter
    {
        public void Calc(Cube5 cube)
        {
            cube.Count = cube.Pixels.Length;
            cube.IsCalcCount = true;
        }

        public Cube5 Select(Cube5 cube)
        {
            Cube5 result = cube.Cubes[0];
            foreach (var item in cube.Cubes)
            {
                if (result.Pixels.Length < item.Pixels.Length)
                    result = item;
            }
            return result;
        }
    }
    public interface ISplitter
    {
        public void Calc(Cube5 cube);
        public (Cube5 cubeA, Cube5 cubeB) Split(Cube5 cube);
    }
    //辺の中央で分割
    public class Split_SideMid : ISplitter
    {
        public void Calc(Cube5 cube)
        {
            //if (cube.IsCalcMinMax == false)
            //{
            //    new CalcA().Calc(cube);
            //}
        }

        public (Cube5 cubeA, Cube5 cubeB) Split(Cube5 cube)
        {
            if (cube.IsCalcMinMax == false) new Select_LongSide().Calc(cube);
            var pixA = new List<byte>();
            var pixB = new List<byte>();
            int mid = (int)((cube.Max + cube.Min) / 2.0);
            foreach (var item in cube.Pixels)
            {
                if (item < mid)
                {
                    pixA.Add(item);
                }
                else
                {
                    pixB.Add(item);
                }
            }
            var cuA = new Cube5(pixA.ToArray(), cube.Selecter, cube.Splitter);
            var cuB = new Cube5(pixB.ToArray(), cube.Selecter, cube.Splitter);
            return (cuA, cuB);
        }
    }
    //中央値
    public class Split_Mid : ISplitter
    {
        public void Calc(Cube5 cube)
        {
            //if (cube.IsCalcCount == false) new CalcB().Calc(cube);
        }

        public (Cube5 cubeA, Cube5 cubeB) Split(Cube5 cube)
        {
            if (cube.IsCalcCount == false) new Select_ManyPicexls().Calc(cube);
            var pixA = new List<byte>();
            var pixB = new List<byte>();
            byte[] neko = cube.Pixels.OrderBy(x => x).ToArray();

            int mid = neko[neko.Length / 2];
            foreach (var item in cube.Pixels)
            {
                if (item < mid)
                    pixA.Add(item);
                else
                    pixB.Add(item);
            }
            var cuA = new Cube5(pixA.ToArray(), cube.Selecter, cube.Splitter);
            var cuB = new Cube5(pixB.ToArray(), cube.Selecter, cube.Splitter);
            return (cuA, cuB);
        }
    }

    public class Cube5
    {
        //public SelectType SelectType;
        //public SplitType SplitType;
        public ISelecter Selecter;
        public ISplitter Splitter;
        public List<Cube5> Cubes = new List<Cube5>();
        public byte[] Pixels;
        public byte Min;
        public byte Max;
        public bool IsCalcMinMax = false;
        public int Count;
        public bool IsCalcCount = false;
        public Cube5(byte[] vs, ISelecter calc, ISplitter splitCalc)
        {
            Pixels = vs;
            //SelectType = selectType;
            //SplitType = splitType;
            Cubes.Add(this);
            Selecter = calc;
            Splitter = splitCalc;
        }
        //public void Calc()
        //{
        //    Selecter.Calc(this);
        //}
        //public void Select()
        //{
        //    var ccc = Selecter.Select(this);
        //    var neko = ccc;
        //}
        public void ExeSplitCalc()
        {
            Splitter.Calc(this);
        }
        public void ExeSplit()
        {
            var ccc = Selecter.Select(this);
            var neko = Splitter.Split(ccc);
        }
        public void ExeGensyoku(int count)
        {
            while (Cubes.Count < count)
            {
                Cube5 ccc = Selecter.Select(this);
                var (cubeA, cubeB) = Splitter.Split(ccc);
                Cubes.Remove(ccc);
                Cubes.Add(cubeA);
                Cubes.Add(cubeB);
            }
        }
        public override string ToString()
        {
            return $"{Pixels.Length}個 Min={Min} Max={Max}";
        }
    }

    #endregion



    //選択タイプ
    public enum SelectType
    {
        SideLong = 1,
        ManyPixels,
        Taiseki,
        Varient,
    }
    //分割タイプ
    public enum SplitType
    {
        SideMid = 1,
        PixelsMid,
        Ootu
    }

}
