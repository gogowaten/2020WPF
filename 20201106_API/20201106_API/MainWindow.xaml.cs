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
using System.Runtime.InteropServices;
using System.Threading;

//Win32 APIやDLL関数を呼び出すには？：.NET TIPS - ＠IT
//https://www.atmarkit.co.jp/ait/articles/0305/09/news004.html
//C#-.net 備忘録
//http://www.orangemaker.sakura.ne.jp/labo/memo/CSharp-DotNet/win32Api.html
//Beep function(utilapiset.h) -Win32 apps | Microsoft Docs
//https://docs.microsoft.com/en-us/windows/win32/api/utilapiset/nf-utilapiset-beep
//C#からdllImportでWin32 APIのEnumWindows関数を使う方法 - もこたんブログ@mocuLab(´･ω･`)
//https://mocotan.hatenablog.com/entry/2017/10/13/123957

namespace _20201106_API
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll")]
        extern static bool Beep(uint dwFreq, uint dwDuration);

        [DllImport("kernel32.dll")]
        extern static bool GetNumberOfConsoleMouseButtons(out uint lpNumberOfMouseButtons);

        public MainWindow()
        {
            InitializeComponent();

        }

        private void ButtonBeep_Click(object sender, RoutedEventArgs e)
        {
            //            音階周波数
            //https://tomari.org/main/java/oto.html

            //            弾いてみよう
            //http://ayakosan.com/c-lesson31.htm

            Beep(1397, 1000);//ファ
            Beep(1046, 1000);//ド
            Beep(1175, 1000);//レ
            Beep(880, 1000);//ラ
            Beep(932, 1000);//シ♭
            Beep(698, 1000);//ファ
            Beep(932, 1000);//シ♭
            Beep(1046, 1000);//ド
        }

        private void ButtonMouseNumber_Click(object sender, RoutedEventArgs e)
        {

            var neko = GetNumberOfConsoleMouseButtons(out uint mouse);
            var inu = mouse;
        }


        //        ほぼC#だけのぶろぐ
        //http://csharp.maeyan.net/Blog/Details/36

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private void ButtonForegroudWindow_Click(object sender, RoutedEventArgs e)
        {
            IntPtr hwin = GetForegroundWindow();
            MessageBox.Show(hwin.ToString());
        }


        //        【C#】アクティブウィンドウのウィンドウ名を取得 - プログラミングとかブログ
        //https://shirakamisauto.hatenablog.com/entry/2016/03/26/110000

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWin, StringBuilder lpString, int nMaxCount);
        //[DllImport("user32.dll")]
        //private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwprocessId);

        private void ButtonActiveWindowName_Click(object sender, RoutedEventArgs e)
        {
            //int processid;
            //GetWindowThreadProcessId(GetForegroundWindow(), out processid);
            var winName = new StringBuilder(65535);
            GetWindowText(GetForegroundWindow(), winName, 65535);
            MessageBox.Show(winName.ToString());

        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private void ButtonKeyCtrlTest_Click(object sender, RoutedEventArgs e)
        {
            //int keyValue = (int)Key.RightCtrl;
            ////while (true)
            //{

            //    var neko = GetAsyncKeyState((int)Key.RightCtrl);
            //    Thread.Sleep(10);
            //    //MessageBox.Show("1");
            //    //var k = (GetAsyncKeyState(keyValue) & 0x8000) >> 16;
            //    //if (k == 1)
            //    //{
            //    //    MessageBox.Show("");
            //    //}
            //    var inu = GetAsyncKeyState(keyValue);

            //    if((GetAsyncKeyState(keyValue) & 0x8000) != 0)
            //    {
            //        MessageBox.Show((""));
            //    }
            //}

            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {            
            if (GetAsyncKeyState(0x2c) != 0)//0x2cはプリントスクリーンキー
            {
                //MessageBox.Show("");
                Beep(1000, 100);
            }
        }
    }
}
