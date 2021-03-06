﻿using System;
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
using System.Collections.ObjectModel;
using System.Globalization;

namespace _20200316_パレットデザイン
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MyColorData MyDataContext;
        const int COLOR_COUNT = 12;
        ListBox MyListBox3;
        MyColorData MyColorData3;

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();

        }
        private void MyInitialize()
        {
            MyListBox1.ItemTemplate = CreateDataTemplate();



            List<Color> colors = MakeColors(COLOR_COUNT);
            MyDataContext = new MyColorData(colors);
            MyListBox1.DataContext = MyDataContext.Data;

            var data = new List<int> { 1, 2, 3 };
            MyListBox2.DataContext = data;

            MyListBox3 = new ListBox();
            //ListBoxのItemsSourceのバインディングBinding
            var bind = new Binding();
            MyListBox3.SetBinding(ListBox.ItemsSourceProperty, bind);
            
            //listboxの要素追加方向を横にする
            var stackP = new FrameworkElementFactory(typeof(StackPanel));
            stackP.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            var itemsPanel = new ItemsPanelTemplate() { VisualTree = stackP };
            MyListBox3.ItemsPanel = itemsPanel;
            //ListBoxのアイテムテンプレート作成、設定
            MyListBox3.ItemTemplate = CreateDataTemplate();
            //データ作成、設定
            MyColorData3 = new MyColorData(MakeColors(9));
            MyListBox3.DataContext = MyColorData3.Data;
            //表示
            MyStackPanel.Children.Add(MyListBox3);

            MyListBox3.MouseLeftButtonUp += MyListBox3_MouseLeftButtonUp;
        }

        private void MyListBox3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var lb = sender as ListBox;
            var index = lb.SelectedIndex;
            MyColorData3.Data[index] = new MyColor(Colors.Red);
        }

    

     

        private List<Color> MakeColors(int count)
        {
            var vs = new byte[count * 3];
            var r = new Random();
            r.NextBytes(vs);
            var cs = new List<Color>();
            for (int i = 0; i < vs.Length; i += 3)
            {
                cs.Add(Color.FromRgb(vs[i], vs[i + 1], vs[i + 2]));
            }
            return cs;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            MyDataContext.Data[0] = new MyColor(Colors.Red);//ok

        }

        private DataTemplate CreateDataTemplate()
        {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(WidthProperty, 20.0);
            border.SetValue(HeightProperty, 20.0);
            border.SetBinding(BackgroundProperty, new Binding(nameof(MyColor.Brush)));
            

            var panel = new FrameworkElementFactory(typeof(StackPanel));
            panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            panel.AppendChild(border);


            var dt = new DataTemplate();
            dt.VisualTree = panel;
            return dt;
        }



    }


    public class MyColor
    {
        public Color Color { get; set; }
        public string ColorString { get; set; }
        public SolidColorBrush Brush { get; set; }
        public MyColor(Color color)
        {
            Color = color;
            ColorString = color.ToString();
            Brush = new SolidColorBrush(color);
        }
    }
    public class MyColorData
    {
        public ObservableCollection<MyColor> Data { get; set; }
        public MyColorData(List<Color> colors)
        {
            Data = new ObservableCollection<MyColor>();
            for (int i = 0; i < colors.Count; i++)
            {
                Data.Add(new MyColor(colors[i]));
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
