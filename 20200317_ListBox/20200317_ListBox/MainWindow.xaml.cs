using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.ObjectModel;
//ItemTemplateで表示を変更したListBoxを動的作成したい
//WPFのListBox、ItemTemplateで表示変更したListBoxを動的作成したいのでXAMLじゃなくてC#コードで書いてみた - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2020/03/17/133941

namespace _20200317_ListBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //XAMLで用意してあるListBoxにデータ追加
            MyListBox1.DataContext = MakeMyDataList(MakeColors(5));
        }

        
        //ListBoxを動的作成追加
        private void AddListBox()
        {
            var listBox = new ListBox();

            //ListBoxのItemsSourceのBindingはソースの指定もない空のBinding
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
            textBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(MyData.ColorCode)));

            var panel = new FrameworkElementFactory(typeof(StackPanel));
            //panel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);//横積み
            panel.AppendChild(border);
            panel.AppendChild(textBlock);

            var dt = new DataTemplate();
            dt.VisualTree = panel;
            listBox.ItemTemplate = dt;

            //追加(表示)
            MyStackPanel.Children.Add(listBox);


            //表示するデータ作成、設定
            listBox.DataContext = MakeMyDataList(MakeColors(5));
        }



        private List<Color> MakeColors(int count)
        {
            var vs = new byte[count * 3];
            var r = new Random();
            r.NextBytes(vs);
            var colors = new List<Color>();
            for (int i = 0; i < vs.Length; i += 3)
            {
                colors.Add(Color.FromRgb(vs[i], vs[i + 1], vs[i + 2]));
            }
            return colors;
        }
        private ObservableCollection<MyData> MakeMyDataList(List<Color> colors)
        {
            var list = new ObservableCollection<MyData>();
            for (int i = 0; i < colors.Count; i++)
            {
                list.Add(new MyData(colors[i]));
            }
            return list;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            AddListBox();
        }
    }


    //表示するデータ用
    public class MyData
    {
        //Bindingで使う項目はpublicでgetが必要
        public SolidColorBrush Brush { get; set; }
        public string ColorCode { get; set; }

        public MyData(Color color)
        {
            Brush = new SolidColorBrush(color);
            ColorCode = color.ToString();
        }
    }

}
