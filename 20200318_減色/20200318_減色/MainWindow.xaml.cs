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

using System.Collections.ObjectModel;


namespace _20200318_減色
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] MyOriginPixels;
        BitmapSource MyOriginBitmap;
        List<List<Color>> MyColorList = new List<List<Color>>();

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
            ComboBoxColorSelectType.ItemsSource = Enum.GetValues(typeof(ColorSelectType));
            ComboBoxColorSelectType.SelectedIndex = 0;

            string file;
            file = @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん.jpg";
            (MyOriginPixels, MyOriginBitmap) = MakeBitmapSourceAndPixelData(file, PixelFormats.Gray8, 96, 96);

            MyImage.Source = MyOriginBitmap;

        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            SelectType select = (SelectType)ComboBoxSelectType.SelectedItem;
            SplitType split = (SplitType)ComboBoxSplitType.SelectedItem;
            var cube = new Cube5(MyOriginPixels, GetSelecter(select), GetSplitter(split));
            cube.ExeGensyoku(8);

            var colors = cube.GetColors((ColorSelectType)ComboBoxColorSelectType.SelectedItem);
            var data = MakeDataContext(colors);
            MyColorList.Add(colors);

            ListBox list = CreateListBox();
            MyStackPanel.Children.Add(list);
            list.DataContext = data;


        }
        private ObservableCollection<MyData> MakeDataContext(List<Color> colors)
        {
            var data = new ObservableCollection<MyData>();
            for (int i = 0; i < colors.Count; i++)
            {
                data.Add(new MyData(colors[i]));
            }
            return data;
        }

        //        2020WPF/MainWindow.xaml.cs at master · gogowaten/2020WPF
        //https://github.com/gogowaten/2020WPF/blob/master/20200317_ListBox/20200317_ListBox/MainWindow.xaml.cs

        private ListBox CreateListBox()
        {
            var listBox = new ListBox();
            listBox.SetBinding(ListBox.ItemsSourceProperty, new Binding());
            //listboxの要素追加方向を横にする
            var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
            stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            var itemsPanel = new ItemsPanelTemplate() { VisualTree = stackPanel };
            listBox.ItemsPanel = itemsPanel;

            //ListBoxのアイテムテンプレート作成、設定
            //ItemTemplate作成、Bindingも設定する
            //縦積みのstackPanelにBorderとTextBlock
            //StackPanel(縦積み)
            //┣Border
            //┗TextBlock
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(WidthProperty, 20.0);
            border.SetValue(HeightProperty, 10.0);
            border.SetBinding(BackgroundProperty, new Binding(nameof(MyData.Brush)));

            var textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(MyData.GrayScaleValue)));

            var panel = new FrameworkElementFactory(typeof(StackPanel));
            //panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);//横積み
            panel.AppendChild(border);
            panel.AppendChild(textBlock);

            var dt = new DataTemplate();
            dt.VisualTree = panel;
            listBox.ItemTemplate = dt;
            return listBox;
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
    #region Cube選択
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
    #endregion
    #region 分割
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
    #endregion

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

        public List<Color> GetColors(ColorSelectType type)
        {
            var colors = new List<Color>();
            if (type == ColorSelectType.Average)
            {
                foreach (var cube in Cubes)
                {
                    long total = 0;
                    foreach (var pixel in cube.Pixels)
                    {
                        total += pixel;
                    }
                    byte v = (byte)Math.Round((double)total / cube.Pixels.Length, MidpointRounding.AwayFromZero);
                    colors.Add(Color.FromRgb(v, v, v));
                }
            }
            else if (type == ColorSelectType.Core)
            {
                foreach (var cube in Cubes)
                {
                    if (cube.IsCalcMinMax == false)
                    {
                        new Select_LongSide().Calc(cube);
                    }
                    byte v = (byte)Math.Round((cube.Max + cube.Min) / 2.0, MidpointRounding.AwayFromZero);
                    colors.Add(Color.FromRgb(v, v, v));
                }
            }
            else if (type == ColorSelectType.Median)
            {
                foreach (var cube in Cubes)
                {
                    int mid = (cube.Pixels.Length + 1) / 2;
                    var vs = cube.Pixels.OrderBy(x => x).ToArray();
                    var v = vs[mid];
                    colors.Add(Color.FromRgb(v, v, v));
                }
            }
            return colors;
        }


        public override string ToString()
        {
            return $"{Pixels.Length}個 Min={Min} Max={Max}";
        }
    }

    #endregion


    public class MyData
    {
        public SolidColorBrush Brush { get; set; }
        public string ColorCode { get; set; }
        public byte GrayScaleValue { get; set; }
        public MyData(Color color)
        {
            Brush = new SolidColorBrush(color);
            ColorCode = color.ToString();
            GrayScaleValue = color.R;
        }
    }


    //Cube選択タイプ
    public enum SelectType
    {
        SideLong = 1,
        ManyPixels,
        //Taiseki,
        //Varient,
    }
    //分割タイプ
    public enum SplitType
    {
        SideMid = 1,
        PixelsMid,
        //Ootu
    }
    //Cubeからの色選択タイプ
    public enum ColorSelectType
    {
        Average = 1,//ピクセルの平均
        Core,//Cube中心
        Median,//RGB中央値

    }
}
