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
using System.Globalization;

//WPFのListBoxでいろいろ、Binding、見た目の変更、横リスト - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/15893148

namespace _20200315_パレットデザイン
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Color> MyColors = new ObservableCollection<Color>();
        public MainWindow()
        {
            InitializeComponent();

            MyColors.Add(Colors.Red);
            MyColors.Add(Colors.MediumAquamarine);
            MyListBox.DataContext = MyColors;
            var b = new Binding();
            b.Source = MyColors;
            //b.Path=new PropertyPath()
            //MyListBox.SetBinding(ListBox.ItemsSourceProperty,)
            //Loaded += (s, e) => { SetStackPanel(); };
        }
        private void SetStackPanel()
        {
            MyStackPanel.Children.Clear();
            for (int i = 0; i < MyColors.Count; i++)
            {
                var b = new Border();
                MyStackPanel.Children.Add(b);
                b.Width = 20; b.Height = 10;
                var bi = new Binding();
                bi.Source = MyColors[i];
                //bi.Path = new PropertyPath("");//必要ない
                bi.Converter = new MyConverter();
                b.SetBinding(Border.BackgroundProperty, bi);
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            SetStackPanel();

            MyColors.Clear();
            MyColors.Add(Colors.MediumOrchid);
            MyColors.Add(Colors.Orange);
            int count = 10;
            var vs = new byte[count * 3];
            var r = new Random();
            r.NextBytes(vs);
            for (int i = 0; i < count * 3; i += 3)
            {
                MyColors.Add(Color.FromRgb(vs[i], vs[i + 1], vs[i + 2]));
            }

          
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border b = (Border)sender;
            MessageBox.Show($"{b.Background.ToString()}");
            b.Background = new SolidColorBrush(Colors.Red);

        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (MyColors[0] == null) return;
            MyColors[0] = Colors.Black;
        }
    }

    public class MyPalettePanel : StackPanel
    {
        public ObservableCollection<Color> MyColors = new ObservableCollection<Color>();
        public List<Border> Pans = new List<Border>();
        public MyPalettePanel(ObservableCollection<Color> colors)
        {
            this.Orientation = Orientation.Horizontal;
            this.DataContext = colors;
            MyColors = colors;
            
            for (int i = 0; i < colors.Count; i++)
            {
                var b = new Border() { Width = 20, Height = 20 };
                Pans.Add(b);
                this.Children.Add(b);
                var bind = new Binding();
                bind.Converter = new MyConverter();
                bind.Source = colors[i];
                b.SetBinding(Border.BackgroundProperty, bind);

            }
        }

        public void Add(Color color)
        {

        }
    }
    public class MyListBox : ListBox
    {
        public List<FrameworkElementFactory> Pans = new List<FrameworkElementFactory>();
        public MyListBox()
        {
            this.ItemTemplate = CreateDataTemplate();
        }
        private DataTemplate CreateDataTemplate()
        {
            var border = new FrameworkElementFactory(typeof(Border));
            Pans.Add(border);
            border.SetValue(WidthProperty, 20.0);
            border.SetValue(HeightProperty, 20.0);
            var bind = new Binding();
            bind.Converter = new MyConverter();
            border.SetBinding(Border.BackgroundProperty, bind);

            var stackPanel = new FrameworkElementFactory(typeof(StackPanel));
            stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            stackPanel.AppendChild(border);

            var template = new DataTemplate();
            template.VisualTree = stackPanel;
            return template;
        }

        private ListBox BuildListBox()
        {
            var listBox = new ListBox();
            listBox.ItemTemplate = CreateDataTemplate();
            return listBox;
        }


    }



    //    20180226forMyBlog/MainWindow.xaml.cs at master · gogowaten/20180226forMyBlog
    //https://github.com/gogowaten/20180226forMyBlog/blob/master/20180410_%E6%B8%9B%E8%89%B2%E3%83%91%E3%83%AC%E3%83%83%E3%83%88%E3%81%A8%E3%82%AB%E3%83%A9%E3%83%BC%E3%83%94%E3%83%83%E3%82%AB%E3%83%BC/MainWindow.xaml.cs

    /// <summary>
    /// パレット表示用のBorderの配列の管理
    /// </summary>
    public class MyWrapPanel : WrapPanel
    {
        public List<Color> Palette;// { set; get; }
        public List<Border> Pans;
        private int MaxPanCount;

        public MyWrapPanel(int max)
        {
            MaxPanCount = max;
            Pans = new List<Border>(max);
            Border border;
            for (int i = 0; i < max; i++)
            {
                border = new Border()
                {
                    Width = 18,
                    Height = 18,
                    BorderBrush = new SolidColorBrush(Colors.AliceBlue),
                    BorderThickness = new Thickness(1f),
                };
                Pans.Add(border);
                this.Children.Add(border);
            }
            Palette = new List<Color>();
            VerticalAlignment = VerticalAlignment.Center;

        }


        public void SetColorList(List<Color> listColor)
        {
            int cc = (MaxPanCount < listColor.Count) ? MaxPanCount : listColor.Count;
            ClearColor();
            for (int i = 0; i < cc; ++i)
            {
                Pans[i].Background = new SolidColorBrush(listColor[i]);
            }
            for (int i = 0; i < listColor.Count; ++i)
            {
                Palette.Add(listColor[i]);
            }

        }

        public void ClearColor()
        {
            Palette.Clear();
            foreach (var item in Pans)
            {
                item.Background = null;
            }
        }

    }
    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = (Color)value;
            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
