using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

//依存関係プロパティ

//目的
//テキストボックスのTextプロパティに数値型の依存関係プロパティをBindingしておいて
//テキストボックスに入力中(UpdateSourceTrigger=PropertyChanged)にも更新したい
//要はテキストボックスと数値型依存関係プロパティが連動していればいい
//追加で、書式の設定もしたい。入力中は書式を外して、ロストフォーカス時に書式を適用

//問題
//-0.2と入力する場合。順番にキーを打つと、「-0」の時点で強制的に「0」にされてしまう
//また、0.2の場合も「0.」まで打つと強制的に「0」にされてしまう
//このような問題はUpdateSourceTrigger=PropertyChangedじゃなければ起きない

//結果
//最初に考えていたのは
//テキストボックスのTextプロパティ ←Binding→ 数値型依存関係プロパティ
//こうだったけど
//テキストボックスのTextプロパティ ←Binding→ 文字列型依存関係プロパティ ←連携→ 数値型依存関係プロパティ
//こう、あいだに文字列型依存関係プロパティを挟んだ
//これで目的達成できた

//String型とdecimal型の依存関係プロパティを用意、MyTextとMyValue
//それぞれに値変更時のコールバック、PropertyChangedCallbackを用意
//MyTextのCallback
//      decimal.TryParseで数値に変換できたら、それをMyValueに入れる
//MyValueのCallback
//      ToStringで文字列に変換してMyTextに入れる
//これは無限ループしそうだけど、なぜかならない

//                                          
//
namespace _20200625_decimalTextBox
{
    public partial class MainWindow : Window
    {   
        public MainWindow()
        {
            InitializeComponent();

        }

        #region 依存関係プロパティ
        //数値型依存関係プロパティ、今回はdecimal型にした
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

        //文字列型依存関係プロパティ
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
            if (s == "-0" || s == "-0.") return;
            if (decimal.TryParse(s, out decimal m))
            {
                mw.MyValue = m;
            }
        }

        //書式指定用の文字列型依存関係プロパティ
        public string MyStringFormat
        {
            get { return (string)GetValue(MyStringFormatProperty); }
            set { SetValue(MyStringFormatProperty, value); }
        }

        public static readonly DependencyProperty MyStringFormatProperty =
            DependencyProperty.Register(nameof(MyStringFormat), typeof(string), typeof(MainWindow),
                new PropertyMetadata("", OnMyStrinfFormatChanged, CoerceMyStrinfFormatValue));

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
                //エラーなら元の書式に書き換え
                s = mw.MyStringFormat;
            }
            return s;
        }
        #endregion 依存関係プロパティ


        #region textboxのイベント時の動作
        //フォーカス時に文字列全部を選択
        private void MyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.Text = MyValue.ToString();
            tb.SelectAll();
        }

        //ロストフォーカス時に書式適用
        private void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.Text = MyValue.ToString(MyStringFormat);
        }

        //左クリック時にも文字列全部を選択
        private void MyTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.IsFocused == false)
            {
                tb.Focus();
                e.Handled = true;
            }
        }
        #endregion textboxのイベント時の動作


        //        書式を指定して数値を文字列に変換する - .NET Tips(VB.NET, C#...)
        //https://dobon.net/vb/dotnet/string/inttostring.html
        //書式変更確認用ボタンクリック時の動作
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tb = sender as Button;
            MyStringFormat = tb.Content.ToString();
        }


    }

}
