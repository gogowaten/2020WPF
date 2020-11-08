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
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Media.Media3D;


//DCはデバイスコンテキスト

//Win32 APIやDLL関数を呼び出すには？：.NET TIPS - ＠IT
//https://www.atmarkit.co.jp/ait/articles/0305/09/news004.html
//C#-.net 備忘録
//http://www.orangemaker.sakura.ne.jp/labo/memo/CSharp-DotNet/win32Api.html
//Beep function(utilapiset.h) -Win32 apps | Microsoft Docs
//https://docs.microsoft.com/en-us/windows/win32/api/utilapiset/nf-utilapiset-beep
//C#からdllImportでWin32 APIのEnumWindows関数を使う方法 - もこたんブログ@mocuLab(´･ω･`)
//https://mocotan.hatenablog.com/entry/2017/10/13/123957

//ここで関数名で検索で引数とかわかる？
//検索 | Microsoft Docs
//https://docs.microsoft.com/ja-jp/search/?terms=GetClientRect%20win32api
//Win32 API関数リスト
//http://chokuto.ifdef.jp/urawaza/api/


namespace _20201106_API
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer MyTimer;

        [DllImport("kernel32.dll")]
        extern static bool Beep(uint dwFreq, uint dwDuration);

        [DllImport("kernel32.dll")]
        extern static bool GetNumberOfConsoleMouseButtons(out uint lpNumberOfMouseButtons);

        public MainWindow()
        {
            InitializeComponent();

            MyTimer = new System.Windows.Threading.DispatcherTimer();
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);


            short s = -32768;//1000_0000_0000_0000
            short ss = (short)(s & 1);//0
            int sss = s & 1;//0
            short ssss = (short)(s & 0x8000);//-32768
            int i = s & 0x8000;//32768
            short sssss = (short)((s & 0x8000) >> 15);//1
            int ii = (s & 0x8000) >> 15;//1

            short s1 = -32767;//1000_0000_0000_0001
            short s2 = (short)(s1 & 1);//1
            int s3 = s1 & 1;//1

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

        #region アクティブウィンドウ名を取得

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
        #endregion


        #region キーの状態を調べる

        //指定キーが押されたらBEEP音を鳴らす

        //一定間隔でキーの状態を調べるためにタイマーを使用した

        //        【WPF】タイマーによる一定時間間隔の処理 - Netty’s Notebook
        //https://netty.hatenablog.com/entry/2020/03/25/020638
        //【C++初心者】GetAsyncKeyState() の返り値をなぜ&1するのか - Qiita
        //https://qiita.com/kamol/items/fc445f5dd8f431b12b82

        //        win32API今押したキー入力だけを取得する方法を教えて下さい -... - Yahoo!知恵袋
        //https://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q14131868434

        //        GetAsyncKeyState関数（winuser.h）-Win32アプリ| Microsoft Docs
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getasynckeystate
        //GetAsyncKeyStateの戻り値の型はshort
        //最上位ビットが1：現在、キーが押された状態
        //最下位ビットが1：前回のGetAsyncKeyStateの呼び出し後にキーが押された形跡がある状態

        //引数で渡す指定したいキーの値一覧表はここ
        //        Virtual-Key Codes(Winuser.h) - Win32 apps | Microsoft Docs
        //https://docs.microsoft.com/ja-jp/windows/win32/inputdev/virtual-key-codes
        //        KT Software - 仮想キーコード一覧
        //http://kts.sakaiweb.com/virtualkeycodes.html

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private void ButtonKeyCtrlTest_Click(object sender, RoutedEventArgs e)
        {

            //タイマー設定
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);//0.01秒間隔
            timer.Start();
        }

        //キーの状態を調べて押されていたらBEEP音
        //キーが押されていないか、押された形跡もなければ0なので
        //0以外だったらBEEP音にした
        private void Timer_Tick(object sender, EventArgs e)
        {
            //0x2cはプリントスクリーンキー

            //イマイチ反応が良くない
            //if (GetAsyncKeyState(0x2c) != 0) { Beep(1000, 10); }

            //以下の2つのどちらがいいのか、押しっぱなしのときの挙動とか
            //現在押されていた場合
            if (((GetAsyncKeyState(0x2c) & 0x8000) >> 15) == 1) { Beep(1500, 10); }
            //前回から押された形跡があった場合
            //if ((GetAsyncKeyState(0x2c) & 1) == 1) { Beep(1500, 10); }

            //var value = GetAsyncKeyState(0x2c);
            //if (((value & 0x8000) >> 15) == 1)
            //{
            //    if ((value & 1) == 1)
            //    {
            //        Beep(1500, 10);
            //    }
            //}

        }
        #endregion


        #region ウィンドウのクライアント領域(ウィンドウ枠の内側の幅と高さ)取得
        //        GetClientRect function(winuser.h) - Win32 apps | Microsoft Docs
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getclientrect
        //スクリーン上でのウィンドウクライアント領域の取得 - 捨てられたブログ
        //https://blog.recyclebin.jp/archives/863

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);


        private void ButtonClientRect_Click(object sender, RoutedEventArgs e)
        {
            var foreWin = GetForegroundWindow();
            GetClientRect(foreWin, out RECT rect);

        }
        #endregion

        #region 画面上でのウィンドウの上下左右位置取得
        //        GetWindowRect function(winuser.h) - Win32 apps | Microsoft Docs
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowrect

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private void ButtonWindowRect_Click(object sender, RoutedEventArgs e)
        {
            var hwnd = GetForegroundWindow();
            GetWindowRect(hwnd, out RECT rECT);
        }

        #endregion

        #region 画面上でのクライアント領域の左上座標取得
        //        ClientToScreen function(winuser.h) - Win32 apps | Microsoft Docs
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-clienttoscreen

        private struct POINT
        {
            public int X;
            public int Y;
        }
        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hwnd, out POINT lpPoint);
        private void ButtonClientToScreen_Click(object sender, RoutedEventArgs e)
        {
            ClientToScreen(GetForegroundWindow(), out POINT point);

        }
        #endregion

        #region
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        private void ButtonDC_Click(object sender, RoutedEventArgs e)
        {
            var desktopDC = GetDC(IntPtr.Zero);//画面全体のDCハンドル
            var hDC = GetDC(GetForegroundWindow());//アクティブウィンドウのクライアント領域のDCハンドル
        }
        #endregion


        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint rop);
        private const int SRCCOPY = 0x00cc0020;
        private const int SRCINVERT = 0x00660046;
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hDC);
        //画面の左上200x200をアプリに表示
        private void ButtonDraw_Click(object sender, RoutedEventArgs e)
        {
            var desktopDC = GetDC(IntPtr.Zero);
            var app = GetForegroundWindow();
            var appDC = GetDC(app);
            //ClientToScreen(appDC, out POINT pOINT);
            //GetClientRect(appDC, out RECT rECT);
            BitBlt(appDC, 0, 0, 200, 200, desktopDC, 0, 0, SRCCOPY);
            //BitBlt(appDC, pOINT.X, pOINT.Y, rECT.Right - rECT.Left, rECT.Bottom - rECT.Top, desktopDC, 0, 0, SRCCOPY);
            ReleaseDC(app, appDC);
            ReleaseDC(IntPtr.Zero, desktopDC);
        }


        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteObject(IntPtr hObject);

        //        c# - Capturing a window with WPF - Stack Overflow
        //https://stackoverflow.com/questions/1736287/capturing-a-window-with-wpf
        //ここのをコピペ改変
        //画面全体の左上の200ｘ100のbitmapsourceを作成
        private static BitmapSource Capture(Rect area)
        {
            IntPtr screenDC = GetDC(IntPtr.Zero);//プライマリディスプレイのDC
            IntPtr memDC = CreateCompatibleDC(screenDC);
            IntPtr hBitmap = CreateCompatibleBitmap(screenDC, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);
            SelectObject(memDC, hBitmap);
            BitBlt(memDC, 0, 0, (int)area.Width, (int)area.Height, screenDC, (int)area.X, (int)area.Y, SRCCOPY);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);
            return source;
        }

        private void ButtonBitmapSource_Click(object sender, RoutedEventArgs e)
        {
            var bmp = Capture(new Rect(0, 0, 200, 100));
            MyImage.Source = bmp;
        }


        private static BitmapSource Capture2(Rect area)
        {
            IntPtr screenDC = GetDC(GetForegroundWindow());
            IntPtr memDC = CreateCompatibleDC(screenDC);
            //IntPtr hBitmap = CreateCompatibleBitmap(screenDC, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);
            GetClientRect(GetForegroundWindow(), out RECT rECT);
            int w = rECT.Right - rECT.Left;
            int h = rECT.Bottom - rECT.Top;
            IntPtr hBitmap = CreateCompatibleBitmap(screenDC, w, h);

            SelectObject(memDC, hBitmap);
            //BitBlt(memDC, 0, 0, (int)area.Width, (int)area.Height, screenDC, (int)area.X, (int)area.Y, SRCCOPY);
            BitBlt(memDC, 0, 0, w, h, screenDC, 0, 0, SRCCOPY);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            FormatConvertedBitmap format = new FormatConvertedBitmap(source, PixelFormats.Bgr24, null, 0);

            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);
            return source;//alphaが0になってしまう？半透明に不具合
            //return format;//ピクセルフォーマットをBgr24に変更したもの、概ね正常だけどchromeは真っ黒になる、エクセルもタイトルバーが真っ黒
        }

        private void ButtonBitmapSource2_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = Capture2(new Rect(0, 0, 400, 100));
        }


        private void ActiveWindowCapture()
        {
            //タイマー設定
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += Timer_Tick1;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);//0.01秒間隔
            timer.Start();
        }

        private void Timer_Tick1(object sender, EventArgs e)
        {
            //プリントスクリーンキーで判定
            //現在押されていた場合
            if (((GetAsyncKeyState(0x2c) & 0x8000) >> 15) == 1)
            {
                MyImage.Source = Capture2(new Rect(0, 0, 300, 100));
            }
            //前回から押された形跡があった場合
            //if ((GetAsyncKeyState(0x2c) & 1) == 1) { Beep(1500, 10); }
        }
        private void ButtonBitmapSource3_Click(object sender, RoutedEventArgs e)
        {
            ActiveWindowCapture();
        }

        //できた、アルファの問題解消
        //画面全体をキャプチャして、そこから目的の領域を切り抜く方法
        //メニューの項目を表示した状態もキャプチャできた
        private static BitmapSource Capture3()
        {
            //SystemParameters.VirtualScreenWidthはマルチモニタ環境で使うと意味がある
            //IntPtr hBitmap = CreateCompatibleBitmap(screenDC, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);
            GetClientRect(GetForegroundWindow(), out RECT clientRect);//アクティブウィンドウのクライアント領域Rect取得
            int clientWidth = clientRect.Right - clientRect.Left;//実際はtopとleftは必ず0なので、rightとbottomだけでいい
            int clientHeight = clientRect.Bottom - clientRect.Top;
            ClientToScreen(GetForegroundWindow(), out POINT ClientLocate);//クライアント領域の左上座標

            IntPtr screenDC = GetDC(IntPtr.Zero);//画面全体のDC
            IntPtr memDC = CreateCompatibleDC(screenDC);//目的のウィンドウ用DC
            IntPtr hBitmap = CreateCompatibleBitmap(screenDC, clientWidth, clientHeight);
            SelectObject(memDC, hBitmap);

            BitBlt(memDC, 0, 0, clientWidth, clientHeight, screenDC, ClientLocate.X, ClientLocate.Y, SRCCOPY);

            //カーソルの描画
            GetCursorPos(out POINT cursorScreenPoint);//画面上でのカーソル位置
            int cursorClientX = cursorScreenPoint.X - ClientLocate.X;
            int cursorClientY = cursorScreenPoint.Y - ClientLocate.Y;

            IntPtr cursor = GetCursor();
            //カーソルの形状が見た目通りにならないことがある
            //DrawIcon(memDC, mpX, cursorClientY, cursor);
            //DrawIconEx(memDC, mpX, cursorClientY, cursor, 0, 0, 0, IntPtr.Zero, DI_DEFAULTSIZE);
            //DrawIconEx(memDC, mpX, cursorClientY, cursor, 0, 0, 0, IntPtr.Zero, DI_NORMAL);//NORMAL以外は表示されなかったり枠がついたりする
            //VB現在のマウスポインタの種類を取得したいのですが、いくら調べても方法が... - Yahoo!知恵袋
            //https://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q1180420296

            GetIconInfo(cursor, out ICONINFO iCONINFO);
            //BitmapSource ibmp = Imaging.CreateBitmapSourceFromHIcon(iCONINFO.hbmColor, new Int32Rect(), BitmapSizeOptions.FromEmptyOptions());//カーソルハンドルが無効エラー
            var icolor = Imaging.CreateBitmapSourceFromHBitmap(iCONINFO.hbmColor, IntPtr.Zero, new Int32Rect(), BitmapSizeOptions.FromEmptyOptions());
            var imask = Imaging.CreateBitmapSourceFromHBitmap(iCONINFO.hbmMask, IntPtr.Zero, new Int32Rect(), BitmapSizeOptions.FromEmptyOptions());

            CURSORINFO curInfo = new CURSORINFO();
            curInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            GetCursorInfo(out curInfo);
            //DrawIcon(memDC, mpX, cursorClientY, curInfo.hCursor);//かなり良くなったけど、I型アイコンのとき真っ白になる
            //DrawIconEx(memDC, mpX, cursorClientY, curInfo.hCursor, 0, 0, 0, IntPtr.Zero, DI_NORMAL);//これでも変わらず

            //            c# - C#-マウスカーソルイメージのキャプチャ
            //https://python5.com/q/ukmbkppc
            //これがわかれば解決できそう
            //カーソルインフォ → コピーアイコン → アイコンインフォのビットマップマスク画像でI型アイコンの元？の画像取得できた
            IntPtr hicon = CopyIcon(curInfo.hCursor);
            GetIconInfo(hicon, out ICONINFO icInfo);
            BitmapSource imask2 = Imaging.CreateBitmapSourceFromHBitmap(icInfo.hbmMask, IntPtr.Zero, new Int32Rect(), BitmapSizeOptions.FromEmptyOptions());
            //var icolor2 = Imaging.CreateBitmapSourceFromHBitmap(icInfo.hbmColor, IntPtr.Zero, new Int32Rect(), BitmapSizeOptions.FromEmptyOptions());
            IntPtr maskHdc = CreateCompatibleDC(screenDC);
            //SelectObject(memDC, hBitmap);
            //IntPtr iconDC = GetDC(icInfo.hbmMask);
            IntPtr iconDC = GetDC(hicon);
            //BitBlt(memDC, 0, 0, clientWidth, clientHeight, maskHdc, 0, 32, SRCCOPY);
            //BitBlt(memDC, 0, 0, clientWidth, clientHeight, maskHdc, 0, 0, SRCINVERT);
            //BitBlt(memDC, 0, 0, 32, 32, maskHdc, 0, 32, SRCCOPY);
            //BitBlt(memDC, 0, 0, 32, 32, maskHdc, 0, 0, SRCINVERT);
            //BitBlt(memDC, 0, 0, clientWidth, clientHeight, iconDC, 0, 32, SRCCOPY);
            BitBlt(memDC, clientWidth, clientHeight, 32, 32, iconDC, 0, 32, SRCCOPY);

            //DrawIconEx(memDC, cursorClientX, cursorClientY, hicon, 0, 0, 0, IntPtr.Zero, DI_NORMAL);//カーソル位置に白四角
            //DrawIconEx(memDC, 0, 0, hicon, 0, 0, 0, hicon, DI_NORMAL);//左上に白四角
            //DrawIconEx(memDC, cursorClientX, cursorClientY, hicon, 32, 32, 0, IntPtr.Zero, DI_NORMAL);//カーソル位置に白四角
            //DrawIconEx(memDC, 0, 0, icInfo.hbmMask, 32, 32, 0, hicon, DI_NORMAL);

            //DrawIconEx(memDC, cursorClientX, cursorClientY, hicon, 0, 0, 0, IntPtr.Zero, DI_COMPAT);//描画なし
            //DrawIconEx(memDC, cursorClientX, cursorClientY, hicon, 0, 0, 0, IntPtr.Zero, DI_MASK);//カーソル位置に白四角
            //DrawIconEx(memDC, cursorClientX, cursorClientY, hicon, 0, 0, 0, IntPtr.Zero, DI_IMAGE);//カーソル位置に白四角





            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(iCONINFO.hbmColor);
            DeleteObject(iCONINFO.hbmMask);
            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);
            DeleteObject(cursor);
            DestroyIcon(hicon);
            ReleaseDC(IntPtr.Zero, maskHdc);
            ReleaseDC(IntPtr.Zero, iconDC);

            return source;
        }
        private void ActiveWindowCapture2()
        {
            //タイマー設定
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += Timer_Tick2;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);//0.01秒間隔
            timer.Start();
        }

        private void Timer_Tick2(object sender, EventArgs e)
        {
            //プリントスクリーンキーで判定
            //現在押されていた場合
            if (((GetAsyncKeyState(0x2c) & 0x8000) >> 15) == 1)
            {
                MyImage.Source = Capture3();
            }
            //前回から押された形跡があった場合
            //if ((GetAsyncKeyState(0x2c) & 1) == 1) { Beep(1500, 10); }
        }

        private void ButtonBitmapSource4_Click(object sender, RoutedEventArgs e)
        {
            ActiveWindowCapture2();
        }



        [DllImport("user32.dll")]
        private static extern IntPtr GetCursor();
        [DllImport("user32.dll")]
        private static extern IntPtr DrawIcon(IntPtr hDC, int x, int y, IntPtr hIcon);
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        private static extern IntPtr DrawIconEx(IntPtr hDC, int x, int y, IntPtr hIcon, int cxWidth, int cyWidth, int istepIfAniCur, IntPtr hbrFlickerFreeDraw, int diFlags);
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


        private static BitmapSource Capture4()
        {
            BitmapSource bmp = Clipboard.GetImage();

            IntPtr screenDC = GetDC(IntPtr.Zero);//画面全体のDC
            IntPtr memDC = CreateCompatibleDC(screenDC);//目的のウィンドウ用DC
            //SystemParameters.VirtualScreenWidthはマルチモニタ環境で使うと意味がある
            //IntPtr hBitmap = CreateCompatibleBitmap(screenDC, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);
            GetClientRect(GetForegroundWindow(), out RECT rECT);//アクティブウィンドウのクライアント領域Rect取得
            int w = rECT.Right - rECT.Left;//実際はtopとleftは必ず0なので、rightとbottomだけでいい
            int h = rECT.Bottom - rECT.Top;
            IntPtr hBitmap = CreateCompatibleBitmap(screenDC, w, h);
            ClientToScreen(GetForegroundWindow(), out POINT pointWnd);//クライアント領域の左上座標

            SelectObject(memDC, hBitmap);
            BitBlt(memDC, 0, 0, w, h, screenDC, pointWnd.X, pointWnd.Y, SRCCOPY);
            ////カーソルの描画、形状が変化してしまう            
            //IntPtr cursor = GetCursor();
            //GetCursorPos(out POINT cursorPoint);
            //int mpX = cursorPoint.X - pointWnd.X;
            //int mpY = cursorPoint.Y - pointWnd.Y;
            //DrawIcon(memDC, mpX, mpY, cursor);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);
            return source;
        }
        private void ActiveWindowCapture3()
        {
            //タイマー設定
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += Timer_Tick3;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);//0.01秒間隔
            timer.Start();
        }

        private void Timer_Tick3(object sender, EventArgs e)
        {
            //プリントスクリーンキーで判定
            //現在押されていた場合
            if (((GetAsyncKeyState(0x2c) & 0x8000) >> 15) == 1)
            {
                if (Clipboard.ContainsImage() == false) return;
                var bmp = Clipboard.GetImage();
                bmp = new FormatConvertedBitmap(bmp, PixelFormats.Bgr24, null, 0);
                MyImage.Source = bmp;
            }
            //前回から押された形跡があった場合
            //if ((GetAsyncKeyState(0x2c) & 1) == 1) { Beep(1500, 10); }
        }
        private void ButtonCursor_Click(object sender, RoutedEventArgs e)
        {
            ActiveWindowCapture3();

        }


        //マウスカーソルのマスク画像取得
        private BitmapSource GetIconMaskBitmap()
        {
            //カーソルインフォ → コピーアイコン → アイコンインフォのビットマップマスク画像でI型アイコンの元？の画像取得できた
            CURSORINFO curInfo = new CURSORINFO();
            curInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            GetCursorInfo(out curInfo);
            IntPtr hicon = CopyIcon(curInfo.hCursor);
            GetIconInfo(hicon, out ICONINFO icInfo);
            BitmapSource imask2 = Imaging.CreateBitmapSourceFromHBitmap(icInfo.hbmMask, IntPtr.Zero, new Int32Rect(), BitmapSizeOptions.FromEmptyOptions());

            DestroyIcon(hicon);

            return imask2;
        }

        private void ButtonIconMaskBmp_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = GetIconMaskBitmap();
        }

        //マウスカーソルのマスク画像取得、タイマー版
        private void ButtonIconMaskBmp2_Click(object sender, RoutedEventArgs e)
        {
            MyTimer.Start();
            MyTimer.Tick += MyTimer_Tick;
        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            //プリントスクリーンキーで判定
            //現在押されていた場合
            if (((GetAsyncKeyState(0x2c) & 0x8000) >> 15) == 1)
            {
                MyImage.Source = GetIconMaskBitmap();
            }
            //前回から押された形跡があった場合
            //if ((GetAsyncKeyState(0x2c) & 1) == 1) { Beep(1500, 10); }
        }


        //Bgra32とスケーリングしている場合は？ホットスポット？

        private BitmapSource MixBitmap(BitmapSource source, BitmapSource cursor, POINT cursorLocate, int xOffset, int yOffset)
        {
            BitmapSource resultBmp = null;
            int sideLenght = cursor.PixelWidth;//32、横一辺の長さ
            int maskHeight = cursor.PixelHeight;//64、縦は横の2倍のはず
            if (sideLenght * 2 != maskHeight) return resultBmp;//2倍じゃなければnullを返して終了

            //マスク画像を上下半分に切り出して画素値を配列化、マスク画像は白と黒の2色の1bpp画像のはず
            //ピクセルフォーマットをbgra32に変換して計算しやすくする
            BitmapSource maskBmp1 = new CroppedBitmap(cursor, new Int32Rect(0, 0, sideLenght, sideLenght));
            FormatConvertedBitmap m1 = new FormatConvertedBitmap(maskBmp1, PixelFormats.Bgra32, null, 0);
            //var m11 = new FormatConvertedBitmap(maskBmp1, PixelFormats.Bgr24, null, 0);
            int maskStride = (m1.PixelWidth * 32 + 7) / 8;
            byte[] mask1Pixels = new byte[m1.PixelHeight * maskStride];
            m1.CopyPixels(mask1Pixels, maskStride, 0);

            BitmapSource maskBmp2 = new CroppedBitmap(cursor, new Int32Rect(0, sideLenght, sideLenght, sideLenght));
            var m2 = new FormatConvertedBitmap(maskBmp2, PixelFormats.Bgra32, null, 0);
            byte[] mask2Pixels = new byte[m2.PixelHeight * maskStride];
            m2.CopyPixels(mask2Pixels, maskStride, 0);

            int w = source.PixelWidth;
            int h = source.PixelHeight;
            int bpp = source.Format.BitsPerPixel;//1ビクセルあたりのbit数、bgra32は4になるはず
            int stride = (w * bpp + 7) / 8;
            byte[] pixels = new byte[h * stride];
            source.CopyPixels(pixels, stride, 0);

            int beginX = cursorLocate.X - xOffset;
            int beginY = cursorLocate.Y - yOffset;
            int endX = beginX + sideLenght;
            int endY = beginY + sideLenght;
            if (endX > w) endX = w;
            if (endY > h) endY = h;

            int yCount = 0;
            int xCount = 0;
            int nekocount = 0;
            for (int y = beginY; y < endY; y++)
            {
                for (int x = beginX; x < endX; x++)
                {
                    var p = y * stride + x * 4;
                    var pp = yCount * maskStride + xCount * 4;
                    //pixels[p] = 0;
                    //pixels[p+1] = 0;
                    //pixels[p+2] = 0;


                    //マスク1が黒なら画像も黒にする
                    if (mask1Pixels[pp] == 0)
                    {
                        pixels[p] = 0;
                        pixels[p + 1] = 0;
                        pixels[p + 2] = 0;
                    }

                    //マスク2が白なら色反転
                    if (mask2Pixels[pp] == 255)
                    {
                        pixels[p] = (byte)(255 - pixels[p]);
                        pixels[p + 1] = (byte)(255 - pixels[p + 1]);
                        pixels[p + 2] = (byte)(255 - pixels[p + 2]);
                        nekocount++;
                    }
                    xCount++;
                }
                yCount++;
                xCount = 0;
            }

            return BitmapSource.Create(w, h, source.DpiX, source.DpiY, source.Format, source.Palette, pixels, stride);

        }

        private void Buttonクライアント領域取得_Click(object sender, RoutedEventArgs e)
        {
            MyTimer.Tick += MyTimer_Tick1;
            MyTimer.Start();
        }

        private void MyTimer_Tick1(object sender, EventArgs e)
        {
            //プリントスクリーンキーで判定
            //現在押されていた場合
            if (((GetAsyncKeyState(0x2c) & 0x8000) >> 15) == 1)
            {
                MyImage.Source = CaptureClientWithoutCursor();
            }
            //前回から押された形跡があった場合
            //if ((GetAsyncKeyState(0x2c) & 1) == 1) { Beep(1500, 10); }
        }
        private BitmapSource CaptureClientWithoutCursor()
        {
            IntPtr actWindDC = GetDC(GetForegroundWindow());//アクティブウィンドウのDC取得
            IntPtr memDC = CreateCompatibleDC(actWindDC);//画像コピー先のDC作成

            //アクティブウィンドウのクライアント領域
            GetClientRect(GetForegroundWindow(), out RECT clientRect);//Rect取得
            int w = clientRect.Right - clientRect.Left;//横幅
            int h = clientRect.Bottom - clientRect.Top;//縦

            IntPtr hBitmap = CreateCompatibleBitmap(actWindDC, w, h);//コピー先のbitmap作成
            SelectObject(memDC, hBitmap);//よくわからん、コピー先のDCのオブジェクトにbitmapを指定している？
            //コピー実行
            BitBlt(memDC, 0, 0, w, h, actWindDC, 0, 0, SRCCOPY);
            //BitBlt(コピー先DC、left、top、横、縦、コピー元、left、top、コピー方法)

            //コピー先のbitmapからbitmapsource作成
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            //後片付け
            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, actWindDC);
            ReleaseDC(IntPtr.Zero, memDC);

            return source;//alphaが0になってしまう？半透明に不具合あり

            //alphaを255にするためにBgr24に変換してみたけどchromeが真っ黒になる
            //FormatConvertedBitmap format = new FormatConvertedBitmap(source, PixelFormats.Bgr24, null, 0);
            //return format;//
        }

        private BitmapSource CaptureClientWithoutCursorFromDesktop()
        {
            //SystemParameters.VirtualScreenWidthはマルチモニタ環境で使うと意味がある
            //IntPtr hBitmap = CreateCompatibleBitmap(screenDC, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);

            //アクティブウィンドウのクライアント領域
            IntPtr actWindow = GetForegroundWindow();
            GetClientRect(actWindow, out RECT clientRect);//Rect取得
            int clientWidth = clientRect.Right - clientRect.Left;//横幅、実際はtopとleftは必ず0なので、rightとbottomだけでいい
            int clientHeight = clientRect.Bottom - clientRect.Top;//縦
            ClientToScreen(actWindow, out POINT ClientLocate);//画面全体の中での左上座標

            IntPtr screenDC = GetDC(IntPtr.Zero);//画面全体のDC
            IntPtr memDC = CreateCompatibleDC(screenDC);//画像コピー先DC作成
            IntPtr hBitmap = CreateCompatibleBitmap(screenDC, clientWidth, clientHeight);//コピー先のbitmap作成
            SelectObject(memDC, hBitmap);
            //コピー実行
            BitBlt(memDC, 0, 0, clientWidth, clientHeight, screenDC, ClientLocate.X, ClientLocate.Y, SRCCOPY);
            //BitBlt(コピー先DC、left、top、横、縦、コピー元、left、top、コピー方法)

            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);

            return source;
        }

        private void Buttonクライアント領域取得2_Click(object sender, RoutedEventArgs e)
        {
            MyTimer.Tick += MyTimer_Tick2;
            MyTimer.Start();
        }

        private void MyTimer_Tick2(object sender, EventArgs e)
        {
            //プリントスクリーンキーで判定
            //現在押されていた場合
            if (((GetAsyncKeyState(0x2c) & 0x8000) >> 15) == 1)
            {
                MyImage.Source = CaptureClientWithoutCursorFromDesktop();
            }
            //前回から押された形跡があった場合
            //if ((GetAsyncKeyState(0x2c) & 1) == 1) { Beep(1500, 10); }
        }

        private void Buttonクライアント領域取得カーソル付き_Click(object sender, RoutedEventArgs e)
        {
            MyTimer.Tick += MyTimer_Tick3;
            MyTimer.Start();
        }

        private void MyTimer_Tick3(object sender, EventArgs e)
        {
            //プリントスクリーンキー(0x2C)で判定
            //右コントロールキー(0xA3)
            //現在押されていた場合、最上位ビットが1になる            
            //if (((GetAsyncKeyState(0xA3) & 0x8000) >> 15) == 1)
            //{
            //    BitmapSource maskBitmap = GetIconMaskBitmap();
            //    BitmapSource screenBitmap = CaptureClientWithoutCursorFromDesktop();
            //    ICONINFO info = GetMyIconInfo2();
            //    //ICONINFO info = GetMyIconInfo();
            //    MyImage.Source = MixBitmap(screenBitmap, maskBitmap, GetCursorPositionInClient(), info.xHotspot, info.yHotspot);
            //    Beep(1500, 10);
            //}

            //前回から押された形跡があった場合、最下位ビットが1
            if ((GetAsyncKeyState(0xA3) & 1) == 1)
            {
                BitmapSource maskBitmap = GetIconMaskBitmap();
                BitmapSource screenBitmap = CaptureClientWithoutCursorFromDesktop();
                ICONINFO info = GetMyIconInfo2();
                //ICONINFO info = GetMyIconInfo();
                MyImage.Source = MixBitmap(screenBitmap, maskBitmap, GetCursorPositionInClient(), info.xHotspot, info.yHotspot);

                Beep(1500, 10);
            }
        }

        //マウスカーソルの座標取得
        private POINT GetCursorPositionInClient()
        {
            //アクティブウィンドウのクライアント領域の左上座標
            ClientToScreen(GetForegroundWindow(), out POINT ClientLocate);
            GetCursorPos(out POINT cursorScreenPoint);//画面上でのカーソル位置
            POINT p = new POINT();
            p.X = cursorScreenPoint.X - ClientLocate.X;
            p.Y = cursorScreenPoint.Y - ClientLocate.Y;
            return p;
        }

        //マウスカーソルのホットスポット、ずれる
        private ICONINFO GetMyIconInfo()
        {
            var c = GetCursor();
            GetIconInfo(c, out ICONINFO info);
            return info;
        }
        //マウスカーソルのホットスポット2、ずれたりずれなかったりする
        private ICONINFO GetMyIconInfo2()
        {
            var cInfo = new CURSORINFO();
            cInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            GetCursorInfo(out cInfo);
            var icon = CopyIcon(cInfo.hCursor);
            GetIconInfo(icon, out ICONINFO info);
            DestroyIcon(icon);
            return info;
        }


    }
}
