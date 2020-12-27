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

namespace _20201112_dotNET5Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Core3のときと同じように、参照の追加無しでIntrinsicsが使える
            var v = System.Runtime.Intrinsics.Vector256.Create(5.0);
            var vv = System.Runtime.Intrinsics.X86.Avx.Add(v, v);

            //
            var ab = Environment.CurrentDirectory;//実行ファイルのあるフォルダパス、\なし
            var aa = Environment.CommandLine;//実行ファイルのフルパス？プロジェクト名.dll
            var bb = Environment.GetCommandLineArgs();//配列、実行ファイルのフルパス？プロジェクト名.dll
            var cc = Environment.Is64BitProcess;//true
            var dd = Environment.ProcessorCount;//8
            var ee = Environment.Version;//5.0.0、.NETのバージョン
            var ff = Environment.SystemDirectory;//c win system32
            var gg = System.AppContext.BaseDirectory;//実行ファイルのあるフォルダパス、最後に\付き

            var hh= System.Diagnostics.FileVersionInfo.GetVersionInfo(Environment.CommandLine).FileVersion;//実行ファイルのバージョン、文字列
            var ii = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;//実行ファイルのバージョン、配列形式でメジャーマイナーとか分かれている

            MessageBox.Show($"{ab}\n{aa}\n{bb}\n{cc}\n{dd}\n{ee}\n{ff}\n{gg}\n{hh}\n{ii}\n");
        }
//        .NET 5 の確認「単一ファイルの配置と実行可能ファイル」 - rksoftware
//https://rksoftware.hatenablog.com/entry/2020/11/29/130535?_ga=2.75216402.1273136612.1606628202-572939506.1592007149

//        単一ファイル アプリケーション - .NET Core | Microsoft Docs
//https://docs.microsoft.com/ja-jp/dotnet/core/deploying/single-file#api-incompatibility

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //.NET Core 以前ならこれでok
            var na = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (na == null)
            {
                MessageBox.Show("(　´∀｀)＜ぬるぽ","■━⊂(　・∀・) 彡 ｶﾞｯ☆`Д´)ﾉ");
            }
            else
            {
                MessageBox.Show(na.ToString());

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //.NET 5からはこの方法
            string[] na = Environment.GetCommandLineArgs();
            string path = na[0];
            if (path == null)
            {
                MessageBox.Show("null");
            }
            else
            {
                path = System.IO.Path.GetDirectoryName(path);
                MessageBox.Show(path);

            }
        }
    }
}
