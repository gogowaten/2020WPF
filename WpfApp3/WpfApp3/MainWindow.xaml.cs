﻿using System;
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

using System.Runtime.InteropServices;//Imagingで使っている
using System.Windows.Interop;//CreateBitmapSourceFromHBitmapで使っている
using System.Windows.Threading;//DispatcherTimerで使っている


namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region WindowsAPI^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        //キーの入力取得
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

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
                return $"横:{right - left:0000}, 縦:{bottom - top:0000}  ({left}, {top}, {right}, {bottom})";
            }
        }
        //座標取得用
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWin, StringBuilder lpString, int nMaxCount);

        //手前にあるウィンドウのハンドル取得
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        //ウィンドウのRect取得
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        //ウィンドウのクライアント領域のRect取得
        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        //クライアント領域の座標を画面全体での座標に変換
        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, out POINT lpPoint);

        //DWM（Desktop Window Manager）
        //見た目通りのRectを取得できる、引数のdwAttributeにDWMWA_EXTENDED_FRAME_BOUNDSを渡す
        //引数のcbAttributeにはRECTのサイズ、Marshal.SizeOf(typeof(RECT))これを渡す
        //戻り値が0なら成功、0以外ならエラー値
        [DllImport("dwmapi.dll")]
        private static extern long DwmGetWindowAttribute(IntPtr hWnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT rect, int cbAttribute);

        //ウィンドウ属性
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
            DWMWA_EXTENDED_FRAME_BOUNDS,//見た目通りのウィンドウのRect
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        };

        //パレントウィンドウ取得
        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, GETWINDOW_CMD uCmd);//本当のuCmdはuint型
        enum GETWINDOW_CMD
        {
            GW_CHILD = 5,
            //指定されたウィンドウが親ウィンドウである場合、取得されたハンドルは、Zオーダーの最上位にある子ウィンドウを識別します。それ以外の場合、取得されたハンドルはNULLです。この関数は、指定されたウィンドウの子ウィンドウのみを調べます。子孫ウィンドウは調べません。
            GW_ENABLEDPOPUP = 6,
            //取得されたハンドルは、指定されたウィンドウが所有する有効なポップアップウィンドウを識別します（検索では、GW_HWNDNEXTを使用して最初に見つかったそのようなウィンドウが使用されます）。それ以外の場合、有効なポップアップウィンドウがない場合、取得されるハンドルは指定されたウィンドウのハンドルです。
            GW_HWNDFIRST = 0,
            //取得されたハンドルは、Zオーダーで最も高い同じタイプのウィンドウを識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_HWNDLAST = 1,
            //取得されたハンドルは、Zオーダーで最も低い同じタイプのウィンドウを識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_HWNDNEXT = 2,
            //取得されたハンドルは、指定されたウィンドウの下のウィンドウをZオーダーで識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_HWNDPREV = 3,
            //取得されたハンドルは、指定されたウィンドウの上のウィンドウをZオーダーで識別します。
            //指定されたウィンドウが最上位のウィンドウである場合、ハンドルは最上位のウィンドウを識別します。指定されたウィンドウがトップレベルウィンドウである場合、ハンドルはトップレベルウィンドウを識別します。指定されたウィンドウが子ウィンドウの場合、ハンドルは兄弟ウィンドウを識別します。

            GW_OWNER = 4,
            //取得されたハンドルは、指定されたウィンドウの所有者ウィンドウを識別します（存在する場合）。詳細については、「所有するWindows」を参照してください。
        }
        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hWnd, GETANCESTOR_FLAGS gaFlags);//本当のgaFlagsはuint型の1 2 3
        //GetAncestorのフラグ用
        enum GETANCESTOR_FLAGS
        {
            GA_PARENT = 1,
            //親ウィンドウを取得します。GetParent関数の場合のように、これには所有者は含まれません。
            GA_ROOT = 2,
            //親ウィンドウのチェーンをたどってルートウィンドウを取得します。
            GA_ROOTOWNER = 3,
            //GetParent によって返された親ウィンドウと所有者ウィンドウのチェーンをたどって、所有されているルートウィンドウを取得します。
        }


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
        //private const int SRCCOPY = 0x00cc0020;
        //private const int SRCINVERT = 0x00660046;
        //private const int CAPTUREBLT = 0x40000000;
        enum MyEnum : uint
        {
            SRCCOPY = 0x00cc0020,
            SRCINVERT = 0x00660046,
            CAPTUREBLT = 0x4000000,
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr ho);


        //
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);
        //


        //ウィンドウ系のAPI
        //Windows（Windowsおよびメッセージ）-Win32アプリ | Microsoft Docs
        // https://docs.microsoft.com/en-us/windows/win32/winmsg/windows





        #region マウスカーソル系API
        //マウスカーソル関係

        [DllImport("user32.dll")]
        private static extern IntPtr GetCursor();
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        private static extern IntPtr DrawIcon(IntPtr hDC, int x, int y, IntPtr hIcon);
        [DllImport("user32.dll")]
        private static extern IntPtr DrawIconEx(IntPtr hDC,
                                                int x,
                                                int y,
                                                IntPtr hIcon,
                                                int cxWidth,
                                                int cyWidth,
                                                int istepIfAniCur,
                                                IntPtr hbrFlickerFreeDraw,
                                                int diFlags);
        private const int DI_DEFAULTSIZE = 0x0008;//cxWidth cyWidthが0に指定されている場合に規定サイズで描画する
        private const int DI_NORMAL = 0x0003;//通常はこれを指定する、IMAGEとMASKの組み合わせ
        private const int DI_IMAGE = 0x0002;//画像を使用して描画
        private const int DI_MASK = 0x0001;//マスクを使用して描画
        private const int DI_COMPAT = 0x0004;//このフラグは無視の意味
        private const int DI_NOMIRROR = 0x0010;//ミラーリングされていないアイコンとし描画される
        [DllImport("user32.dll")]
        private static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO pIconInfo);
        struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CURSORINFO pci);
        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }
        [DllImport("user32.dll")]
        private static extern IntPtr CopyIcon(IntPtr hIcon);
        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);//CopyIcon使ったあとに使う
        #endregion マウスカーソル系


        #endregion コピペ呪文ここまで^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^



        //タイマー用
        private DispatcherTimer MyTimer;

        public MainWindow()
        {
            InitializeComponent();

            //タイマー初期化
            MyTimer = new DispatcherTimer();
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Start();

            //アプリ終了時、タイマーストップ
            this.Closing += (s, e) => { MyTimer.Stop(); };

        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            //キー入力取得用
            int vKey2 = KeyInterop.VirtualKeyFromKey(Key.RightShift);
            //キーの状態を取得
            //short key1state = GetAsyncKeyState(vKey1);
            short key2state = GetAsyncKeyState(vKey2);

            //keyが押されていたら
            if ((key2state & 1) == 1)
            //右Ctrlキー＋右Shiftキーが押されていたら
            //if ((key1state & 0x8000) >> 15 == 1 & ((key2state & 1) == 1))
            {

                //Size sz = new Size( SystemParameters.VirtualScreenHeight,SystemParameters.VirtualScreenWidth);// Screen.PrimaryScreen.Bounds.Size;
                int w = (int)SystemParameters.VirtualScreenWidth;
                int h = (int)SystemParameters.VirtualScreenHeight;
                IntPtr hDesk = GetForegroundWindow();
                //IntPtr hDesk = GetDesktopWindow();
                IntPtr hSrce = GetWindowDC(hDesk);
                IntPtr hDest = CreateCompatibleDC(hSrce);
                IntPtr hBmp = CreateCompatibleBitmap(hSrce, w, h);
                IntPtr hOldBmp = SelectObject(hDest, hBmp);
                bool b = BitBlt(hDest, 0, 0, w, h, hSrce, 0, 0, (uint)MyEnum.SRCINVERT | (uint)MyEnum.CAPTUREBLT);
                BitmapSource bmp = Imaging.CreateBitmapSourceFromHBitmap(hBmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());// Bitmap.FromHbitmap(hBmp);
                SelectObject(hDest, hOldBmp);
                DeleteObject(hBmp);
                DeleteDC(hDest);
                ReleaseDC(hDesk, hSrce);
                MyImage.Source = bmp;
            }
        }
    }
}
