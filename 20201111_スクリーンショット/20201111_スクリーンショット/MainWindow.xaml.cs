using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;

//ウィンドウ系のAPI
//Windows（Windowsおよびメッセージ）-Win32アプリ | Microsoft Docs
// https://docs.microsoft.com/en-us/windows/win32/winmsg/windows

namespace _20201111_スクリーンショット
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer MyTimer;
        public MainWindow()
        {
            InitializeComponent();

            MyTimer = new DispatcherTimer();
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Start();

        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            //一番手前のウィンドウのハンドル取得
            //IntPtr hw = GetForegroundWindow();

            //マウスカーソルの位置取得
            GetCursorPos(out POINT cursorP);

            //マウスカーソルの下にあるウィンドウのハンドル取得
            //右クリックメニューなどもウィンドウとして取得される
            IntPtr handleWin = WindowFromPoint(cursorP);

            //ウィンドウのクライアント領域(枠の内側領域)のRect取得
            if (GetClientRect(handleWin, out RECT cRect))
            {
                MyTextBlock1.Text = $"ClientRect ({cRect.left}, {cRect.top}) ({cRect.right}, {cRect.bottom})";
            };
            //ウィンドウのRect取得、これは透明や半透明の枠も含むので見た目と違ってくる
            if (GetWindowRect(handleWin, out RECT wRect))
            {
                MyTextBlock2.Text = $"WindowRect ({wRect.left}, {wRect.top}) ({wRect.right}, {wRect.bottom})";
            }

            //ウィンドウの名前表示
            var winName = new StringBuilder(65535);
            GetWindowText(handleWin, winName, 65535);
            MyTextBlock3.Text = winName.ToString();

        }



        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT pOINT);








        //        スクリーン上でのウィンドウクライアント領域の取得 - 捨てられたブログ
        //https://blog.recyclebin.jp/archives/863






        //        【C#】アクティブウィンドウのウィンドウ名を取得 - プログラミングとかブログ
        //https://shirakamisauto.hatenablog.com/entry/2016/03/26/110000
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWin, StringBuilder lpString, int nMaxCount);

        private struct RECT
        {
            //型はlongじゃなくてintが正解！！！！！！！！！！！！！！
            //longだとおかしな値になる
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        private struct POINT
        {
            public int x;
            public int y;
        }
    }



}
