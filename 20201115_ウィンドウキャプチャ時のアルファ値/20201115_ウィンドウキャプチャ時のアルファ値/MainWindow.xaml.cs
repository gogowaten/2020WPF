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


namespace _20201115_ウィンドウキャプチャ時のアルファ値
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
            public override string ToString()
            {
                return $"横:{right - left:0000}, 縦:{bottom - top:0000}  {left}, {top}, {right}, {bottom}";
            }
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

        //パレントウィンドウ取得
        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);
        //GetAncestorのフラグ用
        const uint GA_PARENT = 1;
        const uint GA_ROOT = 2;
        const uint GA_ROOTOWNER = 3;//ルートとオーナー


        //マウスカーソルの位置取得
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        //クライアント領域の座標を画面全体での座標に変換
        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, out POINT lpPoint);

        //DC取得
        //nullを渡すと画面全体のDCを取得、ウィンドウハンドルを渡すとそのウィンドウのクライアント領域DC
        //失敗した場合の戻り値はnull
        //使い終わったらReleaseDC
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        //渡したDCに互換性のあるDC作成
        //失敗した場合の戻り値はnull
        //使い終わったらDeleteDC
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        //指定されたDCに関連付けられているデバイスと互換性のあるビットマップを作成
        //使い終わったらDeleteObject
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

        //DCにオブジェクトを指定する、オブジェクトの種類はbitmap、brush、font、pen、Regionなど
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint rop);
        private const int SRCCOPY = 0x00cc0020;
        private const int SRCINVERT = 0x00660046;

        //DWM（Desktop Window Manager）
        [DllImport("dwmapi.dll")]
        private static extern long DwmGetWindowAttribute(IntPtr hWnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT rect, int cbAttribute);
        //
        //取得したい属性
        //列挙値の開始は0だとずれていたので1からにした
        enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,//ウィンドウのRect
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        };


        [DllImport("user32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr ho);




        //ウィンドウ系のAPI
        //Windows（Windowsおよびメッセージ）-Win32アプリ | Microsoft Docs
        // https://docs.microsoft.com/en-us/windows/win32/winmsg/windows

        //        スクリーン上でのウィンドウクライアント領域の取得 - 捨てられたブログ
        //https://blog.recyclebin.jp/archives/863

        //        【C#】アクティブウィンドウのウィンドウ名を取得 - プログラミングとかブログ
        //https://shirakamisauto.hatenablog.com/entry/2016/03/26/110000

        //        C#(.NET)で他のウィンドウのクライアント領域のスクリーンショットを撮る - castaneaiのブログ
        //https://castaneai.hatenablog.com/entry/2012/03/14/230323

        //ウィンドウハンドルからDC(デバイスコンテキスト)を取得
        //DCからコピー先のDC作成
        //
        #endregion ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^


        System.Windows.Threading.DispatcherTimer MyTimer;
        IntPtr hWindowForeground;
        IntPtr hWindowUnderCursor;
        IntPtr hWindowForegroundAncestor;

        RECT rectWindowForeground;
        RECT rectWindowForegroundAncestor;
        RECT rectWindowUnderCursor;

        RECT rectExWindowForeground;
        RECT rectExWindowForegroundAncestor;
        RECT rectExWindowUnderCursor;

        public MainWindow()
        {
            InitializeComponent();
            this.Left = 100;
            this.Top = 650;

            MyTimer = new System.Windows.Threading.DispatcherTimer();
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Start();

            this.Closing += (s, e) => { MyTimer.Stop(); };

        }
        private void MyTimer_Tick(object sender, EventArgs e)
        {
            hWindowForeground = GetForegroundWindow();
            GetCursorPos(out POINT cursorP);
            hWindowUnderCursor = WindowFromPoint(cursorP);
            hWindowForegroundAncestor = GetAncestor(hWindowForeground, GA_ROOTOWNER);

            GetWindowRect(hWindowForeground, out rectWindowForeground);
            DwmGetWindowAttribute(hWindowForeground, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out rectExWindowForeground, Marshal.SizeOf(typeof(RECT)));
            GetWindowRect(hWindowForegroundAncestor, out rectWindowForegroundAncestor);
            DwmGetWindowAttribute(hWindowForegroundAncestor, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out rectExWindowForegroundAncestor, Marshal.SizeOf(typeof(RECT)));
            GetWindowRect(hWindowUnderCursor, out rectWindowUnderCursor);
            DwmGetWindowAttribute(hWindowUnderCursor, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out rectExWindowUnderCursor, Marshal.SizeOf(typeof(RECT)));

            MyTextBlock1.Text = $"{rectWindowForeground} 一番手前ウィンドウ";
            MyTextBlock2.Text = $"{rectExWindowForeground} 一番手前ウィンドウの外見上";
            MyTextBlock3.Text = $"{rectWindowForegroundAncestor} 一番手前ウィンドウの祖先";
            MyTextBlock4.Text = $"{rectExWindowForegroundAncestor} 一番手前ウィンドウの祖先の外見上";
            MyTextBlock5.Text = $"{rectWindowUnderCursor} カーソル下ウィンドウ";
            MyTextBlock6.Text = $"{rectExWindowUnderCursor} カーソル下ウィンドウの外見上";

        }

        private void MyButtonStart_Click(object sender, RoutedEventArgs e)
        {
            MyTimer.Start();
        }

        private void MyButtonStop_Click(object sender, RoutedEventArgs e)
        {
            MyTimer.Stop();
        }
    }
}
