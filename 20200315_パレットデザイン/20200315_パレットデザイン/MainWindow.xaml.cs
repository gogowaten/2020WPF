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
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            MyColors.Clear();
            MyColors.Add(Colors.MediumOrchid);
            MyColors.Add(Colors.Orange);

        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            var source = e.Source;
            var osource = e.OriginalSource;
            Border b = (Border)sender;
            MessageBox.Show($"{b.Background.ToString()}");
            b.Background = new SolidColorBrush(Colors.Red);

        }
    }


    public class Palette1
    {

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
