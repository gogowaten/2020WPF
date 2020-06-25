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

//依存関係プロパティ

//目的
//テキストボックスのTextプロパティに数値型の依存関係プロパティをBindingしておいて
//テキストボックスに入力中(UpdateSourceTrigger=PropertyChanged)にも更新したい
//書式の設定もしたい。入力中は書式を外して、ロストフォーカス時に書式を適用

//問題
//-0.2と入力する場合に順番にキーを打つと、「-0」の時点で強制的に「0」にされてしまう
//また、0.2の場合も「0.」まで打つと強制的に「0」にされてしまう
//このような問題はUpdateSourceTrigger=PropertyChangedじゃなければ起きない

//String型とdecimal型の依存関係プロパティを用意、MyTextとMyValue
//それぞれにPropertyChangedCallbackを用意
//MyTextのCallbackでは、decimal.TryParseで数値に変換できたときだけMyValueを変更する
//MyValueのCallbackでは、ToStringで文字列に変換してMyTextに当てる

//テキストボックスのTextプロパティとMyTextをBinding

//
//無限ループしそうだけど、そうはならない
namespace _20200625_decimalTextBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private string MyStringFormat = "C";
        public MainWindow()
        {
            InitializeComponent();

        }



        public decimal MyValue
        {
            get { return (decimal)GetValue(MyValueProperty); }
            set { SetValue(MyValueProperty, value); }
        }

        public static readonly DependencyProperty MyValueProperty =
            DependencyProperty.Register(nameof(MyValue), typeof(decimal), typeof(MainWindow),
                new PropertyMetadata(0m, OnMyValuePropertyChanged));

        private static void OnMyValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mw = d as MainWindow;
            var m = (decimal)e.NewValue;
            if (mw.MyTextBox.IsFocused)
            {
                mw.MyText = m.ToString();
            }
            else
            {
                mw.MyText = m.ToString(mw.MyStringFormat);
            }
        }


        public string MyText
        {
            get { return (string)GetValue(MyTextProperty); }
            set { SetValue(MyTextProperty, value); }
        }

        public static readonly DependencyProperty MyTextProperty =
            DependencyProperty.Register(nameof(MyText), typeof(string), typeof(MainWindow),
                new PropertyMetadata("", OnMyTextPropertyChanged));

        private static void OnMyTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mw = d as MainWindow;
            var s = (string)e.NewValue;
            if (s == "-0" || s == "-0.") { return; }
            if (decimal.TryParse(s, out decimal m))
            {
                mw.MyValue = m;
            }
        }




        public string MyStringFormat
        {
            get { return (string)GetValue(MyStringFormatProperty); }
            set { SetValue(MyStringFormatProperty, value); }
        }

        public static readonly DependencyProperty MyStringFormatProperty =
            DependencyProperty.Register(nameof(MyStringFormat), typeof(string), typeof(MainWindow),
                new PropertyMetadata("", OnMyStrinfFormatChanged,CoerceMyStrinfFormatValue));

        private static void OnMyStrinfFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mw = d as MainWindow;
            var sf = (string)e.NewValue;
            mw.MyText = mw.MyValue.ToString(sf);
        }
        private static object CoerceMyStrinfFormatValue(DependencyObject d, object baseValue)
        {
            //新しい書式を適用するとエラーになる場合は、元の書式に書き換える
            var mw = d as MainWindow;
            var s = (string)baseValue;//新しい書式
            try
            {
                //新しい書式適用
                mw.MyValue.ToString(s);
            }
            catch (Exception)
            {
                //書き換え
                s = mw.MyStringFormat;
            }
            return s;
        }


        private void MyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.Text = MyValue.ToString();
            tb.SelectAll();
        }

        private void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.Text = MyValue.ToString(MyStringFormat);
        }

        private void MyTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.IsFocused == false)
            {
                tb.Focus();
                e.Handled = true;
            }
        }

        //        書式を指定して数値を文字列に変換する - .NET Tips(VB.NET, C#...)
        //https://dobon.net/vb/dotnet/string/inttostring.html

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tb = sender as Button;
            MyStringFormat = tb.Content.ToString();// "0.000";
        }


    }

}
