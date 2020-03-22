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


namespace _20200322_減色一括クラス
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[] MyOriginPixels;
        BitmapSource MyOriginBitmap;
        List<List<Color>> MyColorDataList = new List<List<Color>>();

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();

            MyInitialize();

        }

        private void MyInitialize()
        {
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;

            ButtonGetClipboardImage.Click += ButtonGetClipboardImage_Click;
            ButtonListClear.Click += ButtonListClear_Click;

            //コンボボックスの初期化
            ComboBoxSelectType.ItemsSource = Enum.GetValues(typeof(SelectType));
            ComboBoxSelectType.SelectedIndex = 0;
            ComboBoxSplitType.ItemsSource = Enum.GetValues(typeof(SplitType));
            ComboBoxSplitType.SelectedIndex = 0;
            ComboBoxColorSelectType.ItemsSource = Enum.GetValues(typeof(ColorSelectType));
            ComboBoxColorSelectType.SelectedIndex = 0;

            //string file;
            //file = @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん.jpg";
            //(MyOriginPixels, MyOriginBitmap) = MakeBitmapSourceAndPixelData(file, PixelFormats.Gray8, 96, 96);
            //MyImage.Source = MyOriginBitmap;

        }

        //ファイルドロップされたとき
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            (byte[] pixels, BitmapSource source) = MakeBitmapSourceAndPixelData(filePath[0], PixelFormats.Gray8, 96, 96);
            if (source == null)
            {
                MessageBox.Show("ドロップされたファイルは画像として開くことができなかった");
            }
            else
            {
                MyOriginBitmap = source;
                MyOriginPixels = pixels;
                MyImage.Source = source;
            }
        }

        //すべてのリストボックス消去
        private void ButtonListClear_Click(object sender, RoutedEventArgs e)
        {
            MyColorDataList.Clear();
            MyStackPanel.Children.Clear();
        }

        //クリップボードの画像取得、グレースケール化
        private void ButtonGetClipboardImage_Click(object sender, RoutedEventArgs e)
        {
            if (!Clipboard.ContainsImage()) return;

            MyOriginBitmap = Clipboard.GetImage();
            MyOriginBitmap = new FormatConvertedBitmap(MyOriginBitmap, PixelFormats.Gray8, null, 0);
            MyImage.Source = MyOriginBitmap;
            int w = MyOriginBitmap.PixelWidth;
            int h = MyOriginBitmap.PixelHeight;
            int stride = w * 1;
            byte[] pixels = new byte[h * stride];
            MyOriginBitmap.CopyPixels(pixels, stride, 0);
            MyOriginPixels = pixels;
        }

        private void ButtonMakePalette_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int colorCount = int.Parse(button.Content.ToString());
            MakePaletteColor(colorCount);
        }

        //減色パレット作成
        private void MakePaletteColor(int colorCount)
        {
            if (MyOriginPixels == null) return;

            SelectType selecter = (SelectType)ComboBoxSelectType.SelectedItem;
            SplitType splitter = (SplitType)ComboBoxSplitType.SelectedItem;
            var cube = new Cube(MyOriginPixels);
            cube.Split(colorCount, selecter, splitter);//分割数指定でCube分割

            //Cubeから色取得して、色データ作成
            List<Color> colors = cube.GetColors((ColorSelectType)ComboBoxColorSelectType.SelectedItem);
            ObservableCollection<MyData> data = MakeDataContext(colors);
            MyColorDataList.Add(colors);

            //色データ表示用のlistboxを作成して表示
            ListBox list = CreateListBox();
            list.DataContext = data;
            //MyStackPanel.Children.Add(list);
            MyStackPanel.Children.Add(MakePanelPalette(list, colors.Count));

        }

        private StackPanel MakePanelPalette(ListBox listBox, int colorCount)
        {
            var panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            var tb = new TextBlock() { Text = $"{colorCount}色 " };
            panel.Children.Add(tb);
            panel.Children.Add(listBox);
            return panel;
        }

        //Colorのリストから色データ作成
        private ObservableCollection<MyData> MakeDataContext(List<Color> colors)
        {
            var data = new ObservableCollection<MyData>();
            for (int i = 0; i < colors.Count; i++)
            {
                data.Add(new MyData(colors[i]));
            }
            return data;
        }

        //色データ表示用のlistbox作成
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
            textBlock.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
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

        #endregion
    }



    //グレースケール用だからCube(立方体)じゃなくて線だけど
    public class Cube
    {
        public List<Cube> Cubes = new List<Cube>();//分割したCubeを入れる用
        public byte[] Pixels;
        public byte Min;
        public byte Max;
        public bool IsCalcMinMax = false;
        public bool IsPixelsSorted = false;

        public Cube(byte[] pixels)
        {
            Pixels = pixels;
        }

        /// <summary>
        /// Cubeを分割してCubeのリスト作成
        /// </summary>
        /// <param name="count">分割数</param>
        /// <param name="select">分割するCubeを選択する方法</param>
        /// <param name="split">Cubeを分割する方法</param>
        public void Split(int count, SelectType select, SplitType split)
        {
            Cubes.Clear();
            Cubes.Add(this);
            var confirmCubes = new List<Cube>();//これ以上分割できないCube隔離用
            while (Cubes.Count + confirmCubes.Count < count)
            {
                Cube cube = SelectCube(Cubes, select);
                var (cubeA, cubeB) = SplitCube(cube, split);
                if (cubeA.Pixels.Length == 0 || cubeB.Pixels.Length == 0)
                {
                    //分割できなかったCubeを隔離用リストに移動
                    confirmCubes.Add(cube);
                    Cubes.Remove(cube);
                    //分割できるCubeが尽きたらループ抜け
                    if (Cubes.Count == 0) break;
                }
                else
                {
                    //分割できたCubeをリストから削除して、分割したCubeを追加
                    Cubes.Remove(cube);
                    Cubes.Add(cubeA);
                    Cubes.Add(cubeB);
                }
            }
            //隔離しておいたCubeを戻す
            foreach (var item in confirmCubes)
            {
                Cubes.Add(item);
            }
        }

        #region 分割するCube選択
        private Cube SelectCube(List<Cube> cubes, SelectType select)
        {
            Cube result = cubes[0];
            if (select == SelectType.LongeSide)
            {
                int length = 0;
                foreach (var item in cubes)
                {
                    if (item.IsCalcMinMax == false) CalcMinMax(item);
                    if (length < item.Max - item.Min)
                    {
                        result = item;
                        length = item.Max - item.Min;
                    }
                }
            }
            else if (select == SelectType.MostPixels)
            {
                foreach (var item in cubes)
                {
                    {
                        if (result.Pixels.Length < item.Pixels.Length)
                            result = item;
                    }
                }
            }
            return result;
        }
        #endregion 分割するCube選択

        #region 選択されたCubeを2分割
        private (Cube cubeA, Cube cubeB) SplitCube(Cube cube, SplitType split)
        {
            var pixA = new List<byte>();
            var pixB = new List<byte>();
            Cube cuA = null;
            Cube cuB = null;
            if (split == SplitType.SideCenter)
            {
                if (cube.IsCalcMinMax == false) CalcMinMax(cube);
                int mid = (int)((cube.Max + cube.Min) / 2.0);
                foreach (var item in cube.Pixels)
                {
                    if (item > mid)
                    {
                        pixA.Add(item);
                    }
                    else
                    {
                        pixB.Add(item);
                    }
                }
                cuA = new Cube(pixA.ToArray());
                cuB = new Cube(pixB.ToArray());
            }
            else if (split == SplitType.Median)
            {
                if (cube.IsPixelsSorted == false)
                {
                    Array.Sort(cube.Pixels);
                    IsPixelsSorted = true;
                }
                int mid = cube.Pixels[cube.Pixels.Length / 2];

                foreach (var item in cube.Pixels)
                {
                    if (item > mid)
                        pixA.Add(item);
                    else
                        pixB.Add(item);
                }
                cuA = new Cube(pixA.ToArray());
                cuB = new Cube(pixB.ToArray());
            }
            return (cuA, cuB);
        }
        #endregion 分割

        private void CalcMinMax(Cube cube)
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

        #region 色取得、Cubesから色の抽出
        public List<Color> GetColors(ColorSelectType type)
        {
            var colors = new List<Color>();
            if (type == ColorSelectType.Average)
            {
                colors = GetColorsAverage();
            }
            else if (type == ColorSelectType.Core)
            {
                colors = GetColorsCore();
            }
            else if (type == ColorSelectType.Median)
            {
                colors = GetColorsMedian();
            }
            return colors;
        }
        private List<Color> GetColorsAverage()
        {
            var colors = new List<Color>();
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
            return colors;
        }

        private List<Color> GetColorsCore()
        {
            var colors = new List<Color>();
            foreach (var cube in Cubes)
            {
                if (cube.IsCalcMinMax == false) CalcMinMax(cube);
                byte v = (byte)Math.Round((cube.Max + cube.Min) / 2.0, MidpointRounding.AwayFromZero);
                colors.Add(Color.FromRgb(v, v, v));
            }
            return colors;
        }

        private List<Color> GetColorsMedian()
        {
            var colors = new List<Color>();
            foreach (var cube in Cubes)
            {
                int mid = ((cube.Pixels.Length + 1) / 2) - 1;//+1して2で割っているのは四捨五入、-1してるのは配列のインデックスは0からカウントだから
                if (cube.IsPixelsSorted == false)
                {
                    Array.Sort(cube.Pixels);
                    cube.IsPixelsSorted = true;
                }
                var v = cube.Pixels[mid];
                colors.Add(Color.FromRgb(v, v, v));
            }
            return colors;
        }
        #endregion
    }

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
        LongeSide = 1,//辺最長
        MostPixels,//ピクセル数最多
        //VolumeMax,//体積最大
        //VarientMax,//分散最大
    }
    //分割タイプ
    public enum SplitType
    {
        SideCenter = 1,//辺の中央
        Median,//中央値
        //Ootu,//大津の2値化
    }
    //Cubeからの色選択タイプ
    public enum ColorSelectType
    {
        Average = 1,//ピクセルの平均
        Core,//Cube中心
        Median,//RGB中央値

    }
}
