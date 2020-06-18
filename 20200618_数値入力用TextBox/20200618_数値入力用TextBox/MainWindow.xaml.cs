using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

//        Visual Studio / WPF > link > TextBoxに数値しか入力できなくする > PreviewTextInput使用 | e.Handled = true; - Qiita
//https://qiita.com/7of9/items/04793406f94d229a6c4d
//          C# WPF 数値のみ入力できるTextBoxを作る - 備忘録
//https://kagasu.hatenablog.com/entry/2017/02/14/155824
//            TextBoxに数値のみを入力する[C# WPF]
//https://vdlz.xyz/Csharp/WPF/Control/EditableControl/TextBox/TextBoxNumberOnly.html
//            [Tips][TextBox] キャレットの位置を取得する | HIROs.NET Blog
//https://blog.hiros-dot.net/?p=1594
//          テキストボックスのIME制御 - WPF覚え書き
//https://sites.google.com/site/wpfjueeshuki/tekisutobokkusunoime-zhi-yu
//          正規表現を使って文字列を置換するには？［C#／VB］：.NET TIPS - ＠IT
//https://www.atmarkit.co.jp/ait/articles/1702/08/news023.html



namespace _20200618_数値入力用TextBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

//            IEEE 754における負のゼロ - Wikipedia
//https://ja.wikipedia.org/wiki/IEEE_754%E3%81%AB%E3%81%8A%E3%81%91%E3%82%8B%E8%B2%A0%E3%81%AE%E3%82%BC%E3%83%AD

            //0と-0は同じ？
            var str = "-0.0000";
            var d = double.TryParse(str, out double dd);
            var neko = -0.0 == 0.0;
            double negativeZero = -0.0;
            var inu = negativeZero.Equals(0.0);
            var uma = negativeZero.Equals(-0.0);
            var tako = 1.0 / 0.0;
            var ika = 1.0 / -0.0;
            var ore = 1.0 / double.Parse("-0.0");
            var ddd = 1.0 / dd;
        }


        private void MyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textbox = (TextBox)sender;
            string str = textbox.Text;//文字列
            var inputStr = e.Text;//入力された文字

            //正規表現で入力文字の判定、数字とピリオド、ハイフンならtrue
            bool neko = new System.Text.RegularExpressions.Regex("[0-9.-]").IsMatch(inputStr);

            //入力文字が数値とピリオド、ハイフン以外だったら無効
            if (neko == false)
            {
                e.Handled = true;//無効
                return;//終了
            }

            //キャレット(カーソル)位置が先頭(0)じゃないときの、ハイフン入力は無効
            if (textbox.CaretIndex != 0 && inputStr == "-") { e.Handled = true; return; }

            //2つ目のハイフン入力は無効(全選択時なら許可)
            if (textbox.SelectedText != str)
            {
                if (str.Contains("-") && inputStr == "-") { e.Handled = true; return; }
            }

            //2つ目のピリオド入力は無効
            if (str.Contains(".") && inputStr == ".") { e.Handled = true; return; }
        }

        private void MyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //ピリオドの削除
            //先頭か末尾にあった場合は削除
            var tb = (TextBox)sender;
            string text = tb.Text;
            if (text.StartsWith('.') || text.EndsWith('.'))
            {
                text = text.Replace(".", "");
            }

            // -. も変なのでピリオドだけ削除
            text = text.Replace("-.", "-");

            //数値がないのにハイフンやピリオドがあった場合は削除
            if (text == "-" || text == ".")
                text = "";

            tb.Text = text;
        }

        private void MyTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //貼り付け無効
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}
