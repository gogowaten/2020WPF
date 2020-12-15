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
using System.ComponentModel.DataAnnotations;
using System.Reflection;


namespace _20201212_ファイル名
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Data MyData = new();
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = MyData;
            MyData.FrontText.Add("1st");

            var t = new System.Windows.Threading.DispatcherTimer();
            t.Interval = new TimeSpan(0, 0, 0, 1);
            t.Start();
            t.Tick += (s, e) => { MyTextNow.Text = DateTime.Now.ToString(); };

            Dictionary<ImageType, string> myType = new();            
            foreach (var item in Enum.GetValues(typeof(ImageType)))
            {
                var imageType = (ImageType)item;
                FieldInfo field = imageType.GetType().GetField(imageType.ToString());                
                myType.Add(imageType, field.GetCustomAttribute<DisplayAttribute>().Name);
            }
            MyComboTest.ItemsSource = myType;



            MyComboTest3.ItemsSource = Enum.GetValues(typeof(ImageType));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var neko = MyData;
            var inu = MyComboBoxDateFormat.Text;
            var um = MyComboTest.SelectedItem;
            var v = MyComboTest.SelectedValue;
            //var item2 = MyComboTest2.SelectedItem;
            //var value2 = MyComboTest2.SelectedValue;
            var item3 = MyComboTest3.SelectedItem;
            var value3 = MyComboTest3.SelectedValue;

        }


        private void MyComboBoxDateFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var format = MyComboBoxDateFormat.SelectedValue;
            var date = DateTime.Now;
            MyTextResult.Text = date.ToString("G");
        }
    }




    public class Data
    {
        public ObservableCollection<string> FrontText { get; set; } = new();
        public bool IsFrontText { get; set; }
        public ObservableCollection<string> DateFormats { get; set; } = new();
        public ImageType ImageType { get; set; }
        public Data()
        {
            //FrontText = new();
            IsFrontText = true;
            //DateFormats = new();
        }

    }


    public class MyConvert : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (string)value;
            var date = DateTime.Now;
            try
            {
                return date.ToString(str);
            }
            catch (Exception)
            {
                return date.ToString();
            }


        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



//    列挙体をコンボボックスにBindingする
//https://tepp91.github.io/contents/wpf/enum-combobox.html

    public class MyImageTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DisplayAttribute displayAttribute = field.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute.Name;

            //return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //FieldInfo field = value.GetType().GetField(value.ToString());
            //DisplayAttribute displayAttribute = field.GetCustomAttribute<DisplayAttribute>();
            //return displayAttribute.Name;

            //return value.ToString();
            throw new NotImplementedException();
        }


        //public static string GetString(ImageType type)
        //{
        //    return type.ToString()+": " + getdi
        //}
        //public static string GetDescription(ImageType type)
        //{
        //    return type.GetType().GetMember(type.ToString())[0].GetCustomAttributes(typeof(desc)
        //}
    }


    public enum ImageType
    {
        [Display(Name = "ピング")] png,
        [Display(Name = "ビットマップ")] bmp,
        [Display(Name = "ジェイペグ")] jpg,
        [Display(Name = "ティフ")] tiff,

    }
}
