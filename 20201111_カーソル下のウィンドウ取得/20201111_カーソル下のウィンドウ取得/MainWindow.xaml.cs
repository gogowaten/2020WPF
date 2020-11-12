using System;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Threading;

//マウスカーソルの下にあるウィンドウのRect取得してみた、GetWindowRectとGetClientRect - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2020/11/12/123533

//1秒毎にマウスカーソルの下にあるウィンドウのRectを取得
namespace _20201111_カーソル下のウィンドウ取得
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region WindowsAPI^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //Rect取得用
        private struct RECT
        {
            //型はlongじゃなくてintが正解！！！！！！！！！！！！！！
            //longだとおかしな値になる
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        //座標取得用
        private struct POINT
        {
            public int x;
            public int y;
        }


        //ウィンドウのクライアント領域のRect取得
        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        //ウィンドウのRect取得
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        //手前にあるウィンドウのハンドル取得
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        //指定座標にあるウィンドウのハンドル取得
        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT pOINT);

        //ウィンドウ名取得
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWin, StringBuilder lpString, int nMaxCount);

        //マウスカーソルの位置取得
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        //ウィンドウ系のAPI
        //Windows（Windowsおよびメッセージ）-Win32アプリ | Microsoft Docs
        // https://docs.microsoft.com/en-us/windows/win32/winmsg/windows

        //        スクリーン上でのウィンドウクライアント領域の取得 - 捨てられたブログ
        //https://blog.recyclebin.jp/archives/863

        //        【C#】アクティブウィンドウのウィンドウ名を取得 - プログラミングとかブログ
        //https://shirakamisauto.hatenablog.com/entry/2016/03/26/110000

        #endregion ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^



        DispatcherTimer MyTimer;
        public MainWindow()
        {
            InitializeComponent();

            //1秒毎のタイマー
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
            //右クリックメニューやボタン、テキストボックスとかのコントロールも取得される
            IntPtr handleWin = WindowFromPoint(cursorP);

            //ウィンドウのRect取得、これは透明や半透明の枠も含むので見た目より大きな値になる
            //座標は画面全体での位置になる
            if (GetWindowRect(handleWin, out RECT wRect))
            {
                MyTextBlock1.Text = $"WindowRect 左上座標({wRect.left}, {wRect.top}) 右下({wRect.right}, {wRect.bottom})";
            }

            //ウィンドウのクライアント領域(枠の内側領域)のRect取得
            //leftとtopは常に0、なので実質、rightは横幅、bottomは縦幅ってことになる
            if (GetClientRect(handleWin, out RECT cRect))
            {
                MyTextBlock2.Text = $"ClientRect ({cRect.left}, {cRect.top}) (横幅：{cRect.right}, 縦幅：{cRect.bottom})";
            };

            //ウィンドウの名前表示
            var winName = new StringBuilder(65535);
            GetWindowText(handleWin, winName, 65535);
            MyTextBlock3.Text = $"ウィンドウ名：{winName}";

        }



    }

}
