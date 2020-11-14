using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace _20201112_APIでスクリーンショット
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
                return $"{left}, {top}, {right}, {bottom}";
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
        private static extern IntPtr GetWindow(IntPtr hWnd,uint uCmd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hWnd,uint gaFlags);


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


        DispatcherTimer MyTimer;
        public MainWindow()
        {
            InitializeComponent();

            this.Left = 100;
            this.Top = 650;

            MyTimer = new DispatcherTimer();
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 2000);
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Start();

            this.Closing += (s, e) => { MyTimer.Stop(); };
        }


        private void MyTimer_Tick(object sender, EventArgs e)
        {
            //MyImage.Source= CaptureForegroundWindow();
            //MyImage.Source = CaptureWindow();
            //MyImage.Source = CaptureDWMWindow();
            MyImage.Source = CaptureActiveWindow();
            //MyImage.Source = CaptureActiveClient();
            //MyImage.Source = CaptureControlUnderCursor();
            //MyImage.Source = CaptureControlClientUnderCursor();
            //MyImage.Source = CaptureTestExcel();

        }

        //ウィンドウのテキストをストリングに変換、ウィンドウタイトル
        private string GetWindowString(IntPtr hWnd)
        {
            StringBuilder str = new StringBuilder(65535);
            GetWindowText(hWnd, str, 65535);
            return str.ToString();
        }
        private void MyButton_Click(object sender, RoutedEventArgs e)
        {

            MyImage.Source = CaptureScreen();
        }

        private void MyButtonStart_Click(object sender, RoutedEventArgs e)
        {
            MyTimer.Start();
        }

        private void MyButtonStop_Click(object sender, RoutedEventArgs e)
        {
            MyTimer.Stop();
        }

        //画面全体のスクリーンショット画像取得
        private BitmapSource CaptureScreen()
        {
            IntPtr screenDC = GetDC(IntPtr.Zero);
            var b = SystemParameters.Border;
            var bw = SystemParameters.BorderWidth;
            int screenWidth = (int)SystemParameters.VirtualScreenWidth;
            int screenHeight = (int)SystemParameters.VirtualScreenHeight;


            IntPtr hBitmap = CreateCompatibleBitmap(screenDC, screenWidth, screenHeight);
            IntPtr memDC = CreateCompatibleDC(screenDC);
            SelectObject(memDC, hBitmap);

            BitBlt(memDC, 0, 0, screenWidth, screenHeight, screenDC, 0, 0, SRCCOPY);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);

            return source;
        }

        //フォアグラウンドウィンドウのスクリーンショット
        private BitmapSource CaptureForegroundWindow()
        {
            var hWnd = GetForegroundWindow();
            IntPtr screenDC = GetDC(hWnd);
            GetWindowRect(hWnd, out RECT wRect);
            int screenWidth = wRect.right - wRect.left;
            int screenHeight = wRect.bottom - wRect.top;

            IntPtr hBitmap = CreateCompatibleBitmap(screenDC, screenWidth, screenHeight);
            IntPtr memDC = CreateCompatibleDC(screenDC);
            SelectObject(memDC, hBitmap);

            BitBlt(memDC, 0, 0, screenWidth, screenHeight, screenDC, 0, 0, SRCCOPY);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);

            return source;
        }

        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = CaptureForegroundWindow();

            var hWnd = GetForegroundWindow();
            GetWindowRect(hWnd, out RECT wRect);
            MyTextBlock1.Text = $"left {wRect.left}, top {wRect.top}, right {wRect.right}, bottom{wRect.bottom}";
        }

        private BitmapSource CaptureWindow()
        {
            GetCursorPos(out POINT sp);
            var handleWin = WindowFromPoint(sp);
            GetClientRect(handleWin, out RECT clientRect);
            GetWindowRect(handleWin, out RECT windowRect);
            var screenDC = GetDC(IntPtr.Zero);
            DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRect, Marshal.SizeOf(typeof(RECT)));

            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            var hBitmap = CreateCompatibleBitmap(screenDC, width, height);
            var memDC = CreateCompatibleDC(screenDC);
            SelectObject(memDC, hBitmap);

            BitBlt(memDC, 0, 0, width, height, screenDC, windowRect.left, windowRect.top, SRCCOPY);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            MyTextBlock1.Text = $"横 {windowRect.right - windowRect.left}、縦 {windowRect.bottom - windowRect.top} GetWindowRect {windowRect}";
            MyTextBlock2.Text = $"横 {dwmRect.right - dwmRect.left}、縦 {dwmRect.bottom - dwmRect.top} WdmExtendRect {dwmRect}";
            MyTextBlock3.Text = $"横 {source.PixelWidth}、縦 {source.PixelHeight} bitmap";
            MyTextBlock4.Text = $"横 {clientRect.right}、縦 {clientRect.bottom} GetClientRect {clientRect}";


            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);

            return source;
        }

        //カーソル下のウィンドウ
        private BitmapSource CaptureDWMWindow()
        {
            GetCursorPos(out POINT sp);
            var handleWin = WindowFromPoint(sp);
            GetClientRect(handleWin, out RECT clientRect);
            GetWindowRect(handleWin, out RECT windowRect);
            GetWindowRect(GetForegroundWindow(), out RECT foreWindowRect);

            long hResult = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRect, Marshal.SizeOf(typeof(RECT)));
            long hResult2 = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_BUTTON_BOUNDS, out RECT dwmCaptionRect, Marshal.SizeOf(typeof(RECT)));
            //hResultが0なら成功、それ以外は失敗で16進数にして
            //0x8007_0006だった場合はハンドル無効エラーERROR_INVALID_HANDLE
            //if (hResult == 0x8007_0006) { MessageBox.Show("ハンドルが無効でExtendedFrameBoundsが取得できなかった"); };
            //
            var width = dwmRect.right - dwmRect.left;
            var height = dwmRect.bottom - dwmRect.top;

            var screenDC = GetDC(IntPtr.Zero);
            var hBitmap = CreateCompatibleBitmap(screenDC, width, height);
            var memDC = CreateCompatibleDC(screenDC);
            SelectObject(memDC, hBitmap);


            BitBlt(memDC, 0, 0, width, height, screenDC, dwmRect.left, dwmRect.top, SRCCOPY);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());


            MyTextBlock1.Text = $"横 {windowRect.right - windowRect.left}、縦 {windowRect.bottom - windowRect.top} GetWindowRect {windowRect}";
            MyTextBlock2.Text = $"横 {dwmRect.right - dwmRect.left}、縦 {dwmRect.bottom - dwmRect.top} WdmExtendRect {dwmRect}";
            MyTextBlock3.Text = $"横 {source.PixelWidth}、縦 {source.PixelHeight} bitmap";
            MyTextBlock4.Text = $"横 {clientRect.right}、縦 {clientRect.bottom} GetClientRect {clientRect}";
            MyTextBlock5.Text = $"横 {foreWindowRect.right - foreWindowRect.left}、縦 {foreWindowRect.bottom - foreWindowRect.top} ForeWindowRect {dwmRect}";


            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);

            return source;
        }

        private BitmapSource CaptureActiveWindow()
        {
            GetCursorPos(out POINT sp);
            var handleWin = WindowFromPoint(sp);
            GetClientRect(handleWin, out RECT clientRect);
            GetWindowRect(handleWin, out RECT windowRect);
            var handleForeW = GetForegroundWindow();
            GetWindowRect(handleForeW, out RECT foreWindowRect);

            long hResult = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRect, Marshal.SizeOf(typeof(RECT)));
            long hResultEx = DwmGetWindowAttribute(handleForeW, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRectEx, Marshal.SizeOf(typeof(RECT)));
            long hResult2 = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_BUTTON_BOUNDS, out RECT wdmCaptionRect, Marshal.SizeOf(typeof(RECT)));
            //hResultが0なら成功、それ以外は失敗で16進数にして
            //0x8007_0006だった場合はハンドル無効エラーERROR_INVALID_HANDLE
            //if (hResult == 0x8007_0006) { MessageBox.Show("ハンドルが無効でExtendedFrameBoundsが取得できなかった"); };
            //
            int width = dwmRectEx.right - dwmRectEx.left;
            int height = dwmRectEx.bottom - dwmRectEx.top;

            var screenDC = GetDC(IntPtr.Zero);
            var hBitmap = CreateCompatibleBitmap(screenDC, width, height);
            var memDC = CreateCompatibleDC(screenDC);
            SelectObject(memDC, hBitmap);

            BitBlt(memDC, 0, 0, width, height, screenDC, dwmRectEx.left, dwmRectEx.top, SRCCOPY);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());


            MyTextBlock1.Text = $"横 {windowRect.right - windowRect.left}、縦 {windowRect.bottom - windowRect.top} GetWindowRect {windowRect}";
            MyTextBlock2.Text = $"横 {dwmRect.right - dwmRect.left}、縦 {dwmRect.bottom - dwmRect.top} WdmExtendRect {dwmRect}";
            MyTextBlock3.Text = $"横 {source.PixelWidth}、縦 {source.PixelHeight} bitmap";
            MyTextBlock4.Text = $"横 {clientRect.right}、縦 {clientRect.bottom} GetClientRect {clientRect}";
            MyTextBlock5.Text = $"横 {foreWindowRect.right - foreWindowRect.left}、縦 {foreWindowRect.bottom - foreWindowRect.top} ForeWindowRect {foreWindowRect}";
            MyTextBlock6.Text = $"横 {dwmRectEx.right - dwmRectEx.left}、縦 {dwmRectEx.bottom - dwmRectEx.top} ForeWindowRectEx {dwmRectEx}";


            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);

            FormatConvertedBitmap cb = new FormatConvertedBitmap(source, PixelFormats.Bgr24, null, 0);
            return cb;
        }
        private BitmapSource CaptureActiveClient()
        {
            GetCursorPos(out POINT sp);
            var handleWin = WindowFromPoint(sp);
            GetClientRect(handleWin, out RECT clientRect);
            GetWindowRect(handleWin, out RECT windowRect);
            var handleForeW = GetForegroundWindow();
            GetWindowRect(handleForeW, out RECT foreWindowRect);
            GetClientRect(handleForeW, out RECT foreWindowClientRect);

            long hResult = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRect, Marshal.SizeOf(typeof(RECT)));
            long hResultEx = DwmGetWindowAttribute(handleForeW, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRectEx, Marshal.SizeOf(typeof(RECT)));
            //long hResult2 = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_BUTTON_BOUNDS, out RECT wdmCaptionRect, Marshal.SizeOf(typeof(RECT)));
            //hResultが0なら成功、それ以外は失敗で16進数にして
            //0x8007_0006だった場合はハンドル無効エラーERROR_INVALID_HANDLE
            //if (hResult == 0x8007_0006) { MessageBox.Show("ハンドルが無効でExtendedFrameBoundsが取得できなかった"); };
            //
            int width = foreWindowClientRect.right;
            int height = foreWindowClientRect.bottom;
            ClientToScreen(handleForeW, out POINT clientPoint);

            var screenDC = GetDC(IntPtr.Zero);
            var hBitmap = CreateCompatibleBitmap(screenDC, width, height);
            var memDC = CreateCompatibleDC(screenDC);
            SelectObject(memDC, hBitmap);

            BitBlt(memDC, 0, 0, width, height, screenDC, clientPoint.x, clientPoint.y, SRCCOPY);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());


            MyTextBlock1.Text = $"横 {windowRect.right - windowRect.left}、縦 {windowRect.bottom - windowRect.top} GetWindowRect {windowRect}";
            MyTextBlock2.Text = $"横 {dwmRect.right - dwmRect.left}、縦 {dwmRect.bottom - dwmRect.top} WdmExtendRect {dwmRect}";
            MyTextBlock3.Text = $"横 {source.PixelWidth}、縦 {source.PixelHeight} bitmap";
            MyTextBlock4.Text = $"横 {clientRect.right}、縦 {clientRect.bottom} GetClientRect {clientRect}";
            MyTextBlock5.Text = $"横 {foreWindowRect.right - foreWindowRect.left}、縦 {foreWindowRect.bottom - foreWindowRect.top} ForeWindowRect {foreWindowRect}";
            MyTextBlock6.Text = $"横 {dwmRectEx.right - dwmRectEx.left}、縦 {dwmRectEx.bottom - dwmRectEx.top} ForeWindowRectEx {dwmRectEx}";


            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);


            return source;
        }

        //カーソル下のコントロール
        private BitmapSource CaptureControlUnderCursor()
        {
            GetCursorPos(out POINT sp);
            var handleWin = WindowFromPoint(sp);
            GetClientRect(handleWin, out RECT clientRect);
            GetWindowRect(handleWin, out RECT windowRect);
            var handleForeW = GetForegroundWindow();
            GetWindowRect(handleForeW, out RECT foreWindowRect);
            GetClientRect(handleForeW, out RECT foreWindowClientRect);

            long hResult = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRect, Marshal.SizeOf(typeof(RECT)));
            long hResultEx = DwmGetWindowAttribute(handleForeW, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRectEx, Marshal.SizeOf(typeof(RECT)));
            //long hResult2 = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_BUTTON_BOUNDS, out RECT wdmCaptionRect, Marshal.SizeOf(typeof(RECT)));
            //hResultが0なら成功、それ以外は失敗で16進数にして
            //0x8007_0006だった場合はハンドル無効エラーERROR_INVALID_HANDLE
            //if (hResult == 0x8007_0006) { MessageBox.Show("ハンドルが無効でExtendedFrameBoundsが取得できなかった"); };

            int width, height;
            int offsetX, offsetY;
            POINT clientPoint;
            ClientToScreen(handleWin, out clientPoint);
            if (hResult == 0)
            {
                width = dwmRect.right - dwmRect.left;
                height = dwmRect.bottom - dwmRect.top;
                offsetX = dwmRect.left;
                offsetY = dwmRect.top;
            }
            else
            {
                width = clientRect.right;
                height = clientRect.bottom;
                offsetX = clientPoint.x;
                offsetY = clientPoint.y;
            }


            var screenDC = GetDC(IntPtr.Zero);
            var hBitmap = CreateCompatibleBitmap(screenDC, width, height);
            var memDC = CreateCompatibleDC(screenDC);
            SelectObject(memDC, hBitmap);

            BitBlt(memDC, 0, 0, width, height, screenDC, offsetX, offsetY, SRCCOPY);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());


            MyTextBlock1.Text = $"横 {windowRect.right - windowRect.left}、縦 {windowRect.bottom - windowRect.top} GetWindowRect {windowRect}";
            MyTextBlock2.Text = $"横 {dwmRect.right - dwmRect.left}、縦 {dwmRect.bottom - dwmRect.top} WdmExtendRect {dwmRect}";
            MyTextBlock3.Text = $"横 {source.PixelWidth}、縦 {source.PixelHeight} bitmap";
            MyTextBlock4.Text = $"横 {clientRect.right}、縦 {clientRect.bottom} GetClientRect {clientRect}";
            MyTextBlock5.Text = $"横 {foreWindowRect.right - foreWindowRect.left}、縦 {foreWindowRect.bottom - foreWindowRect.top} ForeWindowRect {foreWindowRect}";
            MyTextBlock6.Text = $"横 {dwmRectEx.right - dwmRectEx.left}、縦 {dwmRectEx.bottom - dwmRectEx.top} ForeWindowRectEx {dwmRectEx}";


            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);


            return source;
        }

        //カーソル下のコントロールのクライアント領域
        private BitmapSource CaptureControlClientUnderCursor()
        {
            GetCursorPos(out POINT sp);
            var handleWin = WindowFromPoint(sp);
            GetClientRect(handleWin, out RECT clientRect);
            GetWindowRect(handleWin, out RECT windowRect);
            var handleForeW = GetForegroundWindow();
            GetWindowRect(handleForeW, out RECT foreWindowRect);
            GetClientRect(handleForeW, out RECT foreWindowClientRect);

            long hResult = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRect, Marshal.SizeOf(typeof(RECT)));
            long hResultEx = DwmGetWindowAttribute(handleForeW, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRectEx, Marshal.SizeOf(typeof(RECT)));
            //long hResult2 = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_BUTTON_BOUNDS, out RECT wdmCaptionRect, Marshal.SizeOf(typeof(RECT)));
            //hResultが0なら成功、それ以外は失敗で16進数にして
            //0x8007_0006だった場合はハンドル無効エラーERROR_INVALID_HANDLE
            //if (hResult == 0x8007_0006) { MessageBox.Show("ハンドルが無効でExtendedFrameBoundsが取得できなかった"); };

            int width, height;
            int offsetX, offsetY;
            POINT clientPoint;
            ClientToScreen(handleWin, out clientPoint);
            //0は対象がウィンドウだった場合、0以外は対象がウィンドウ以外のコントロールだった場合
            if (hResult == 0)
            {
                width = clientRect.right;
                height = clientRect.bottom;
                offsetX = clientPoint.x;
                offsetY = clientPoint.y;
            }
            else
            {
                width = clientRect.right;
                height = clientRect.bottom;
                offsetX = clientPoint.x;
                offsetY = clientPoint.y;
            }


            var screenDC = GetDC(IntPtr.Zero);
            var hBitmap = CreateCompatibleBitmap(screenDC, width, height);
            var memDC = CreateCompatibleDC(screenDC);
            SelectObject(memDC, hBitmap);

            BitBlt(memDC, 0, 0, width, height, screenDC, offsetX, offsetY, SRCCOPY);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());


            MyTextBlock1.Text = $"横 {windowRect.right - windowRect.left}、縦 {windowRect.bottom - windowRect.top} GetWindowRect {windowRect}";
            MyTextBlock2.Text = $"横 {dwmRect.right - dwmRect.left}、縦 {dwmRect.bottom - dwmRect.top} WdmExtendRect {dwmRect}";
            MyTextBlock3.Text = $"横 {source.PixelWidth}、縦 {source.PixelHeight} bitmap";
            MyTextBlock4.Text = $"横 {clientRect.right}、縦 {clientRect.bottom} GetClientRect {clientRect}";
            MyTextBlock5.Text = $"横 {foreWindowRect.right - foreWindowRect.left}、縦 {foreWindowRect.bottom - foreWindowRect.top} ForeWindowRect {foreWindowRect}";
            MyTextBlock6.Text = $"横 {dwmRectEx.right - dwmRectEx.left}、縦 {dwmRectEx.bottom - dwmRectEx.top} ForeWindowRectEx {dwmRectEx}";


            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);


            return source;
        }

        private BitmapSource CaptureTestExcel()
        {
            GetCursorPos(out POINT sp);
            var handleWin = WindowFromPoint(sp);
            GetClientRect(handleWin, out RECT clientRect);
            GetWindowRect(handleWin, out RECT windowRect);
            var handleForeW = GetForegroundWindow();
            GetWindowRect(handleForeW, out RECT foreWindowRect);
            GetClientRect(handleForeW, out RECT foreWindowClientRect);

            //エクセルで右クリックメニューやリボンを開いたときにエクセル全体のウィンドウを取得する
            //var parent = GetParent(handleWin);
            var parent = GetParent(handleForeW);//いまいち
            var owner = GetWindow(handleForeW, 4);//4以外はいまいち
            var ancestor = GetAncestor(handleForeW, 3);//これ！！！！！！！！！！！！！！！！！！
            MyTextBlock7.Text = GetWindowString(ancestor);
            

            long hResult = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRect, Marshal.SizeOf(typeof(RECT)));
            long hResultEx = DwmGetWindowAttribute(handleForeW, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT dwmRectEx, Marshal.SizeOf(typeof(RECT)));
            long hResultExAncestor = DwmGetWindowAttribute(ancestor, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out RECT ancestorExRect, Marshal.SizeOf(typeof(RECT)));
            
            //long hResult2 = DwmGetWindowAttribute(handleWin, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_BUTTON_BOUNDS, out RECT wdmCaptionRect, Marshal.SizeOf(typeof(RECT)));
            //hResultが0なら成功、それ以外は失敗で16進数にして
            //0x8007_0006だった場合はハンドル無効エラーERROR_INVALID_HANDLE
            //if (hResult == 0x8007_0006) { MessageBox.Show("ハンドルが無効でExtendedFrameBoundsが取得できなかった"); };

            int width, height;
            int offsetX, offsetY;
            POINT clientPoint;
            ClientToScreen(handleWin, out clientPoint);
            //0は対象がウィンドウだった場合、0以外は対象がウィンドウ以外のコントロールだった場合
            if (hResultExAncestor == 0)
            {
                width = ancestorExRect.right - ancestorExRect.left;
                height = ancestorExRect.bottom - ancestorExRect.top;
                offsetX = ancestorExRect.left;
                offsetY = ancestorExRect.top;
            }
            else
            {
                width = ancestorExRect.right - ancestorExRect.left;
                height = ancestorExRect.bottom - ancestorExRect.top;
                offsetX = ancestorExRect.left;
                offsetY = ancestorExRect.top;
            }


            var screenDC = GetDC(IntPtr.Zero);
            var hBitmap = CreateCompatibleBitmap(screenDC, width, height);
            var memDC = CreateCompatibleDC(screenDC);
            SelectObject(memDC, hBitmap);

            BitBlt(memDC, 0, 0, width, height, screenDC, offsetX, offsetY, SRCCOPY);
            BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());


            MyTextBlock1.Text = $"横 {windowRect.right - windowRect.left}、縦 {windowRect.bottom - windowRect.top} GetWindowRect {windowRect}";
            MyTextBlock2.Text = $"横 {dwmRect.right - dwmRect.left}、縦 {dwmRect.bottom - dwmRect.top} WdmExtendRect {dwmRect}";
            MyTextBlock3.Text = $"横 {source.PixelWidth}、縦 {source.PixelHeight} bitmap";
            MyTextBlock4.Text = $"横 {clientRect.right}、縦 {clientRect.bottom} GetClientRect {clientRect}";
            MyTextBlock5.Text = $"横 {foreWindowRect.right - foreWindowRect.left}、縦 {foreWindowRect.bottom - foreWindowRect.top} ForeWindowRect {foreWindowRect}";
            MyTextBlock6.Text = $"横 {dwmRectEx.right - dwmRectEx.left}、縦 {dwmRectEx.bottom - dwmRectEx.top} ForeWindowRectEx {dwmRectEx}";


            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);


            return source;
        }

    }
}
