using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20200621_BindingMyValueMyText
{
    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();

        }

        //外からは参照されたくないので
        //protectedな依存関係プロパティ
        protected string MyText
        {
            get { return (string)GetValue(MyTextProperty); }
            set { SetValue(MyTextProperty, value); }
        }
        protected static readonly DependencyProperty MyTextProperty =
                    DependencyProperty.Register(nameof(MyText), typeof(string), typeof(UserControl1),
                        new PropertyMetadata("myTextBox", MyTextProperyChanged));
        private static void MyTextProperyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uc = d as UserControl1;
            string s = (string)e.NewValue;
            //var f = new System.Text.RegularExpressions.Regex("-0[0-9]").IsMatch(s);
            //var ff = new System.Text.RegularExpressions.Regex("-0.[0-9]").IsMatch(s);
            if (s == "-0" || s == "-0.") { return; }
            if (decimal.TryParse(s, out decimal m))
            {
                uc.MyValue = m;
            }
        }

        public decimal MyValue
        {
            get { return (decimal)GetValue(MyValueProperty); }
            set { SetValue(MyValueProperty, value); }
        }

        public static readonly DependencyProperty MyValueProperty =
            DependencyProperty.Register(nameof(MyValue), typeof(decimal), typeof(UserControl1),
                new PropertyMetadata(10m, MyValuePropertyChanged));
        private static void MyValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var m = (decimal)e.NewValue;
            var uc = d as UserControl1;
            uc.MyText = m.ToString();
        }


    }


    //public class MyConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var m = (decimal)value;
    //        return m.ToString();
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var s = (string)value;
    //        if (decimal.TryParse(s, out decimal m))
    //        {
    //            return m;
    //        }
    //        else
    //        {
    //            return 0m;
    //        }
    //    }
    //}
}
