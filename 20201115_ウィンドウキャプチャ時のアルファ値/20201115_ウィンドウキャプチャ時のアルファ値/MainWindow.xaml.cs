using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;//Imagingで使っている
using System.Windows.Interop;//CreateBitmapSourceFromHBitmapで使っている
using System.Windows.Threading;//DispatcherTimerで使っている

//ウィンドウDCからのキャプチャではアルファ値が変なので、画面全体をキャプチャして切り抜き - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2020/11/16/005641

namespace _20201115_ウィンドウキャプチャ時のアルファ値
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


        //ウィンドウのRect取得
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        //手前にあるウィンドウのハンドル取得
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

     

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

     

        [DllImport("user32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr ho);




        //ウィンドウ系のAPI
        //Windows（Windowsおよびメッセージ）-Win32アプリ | Microsoft Docs
        // https://docs.microsoft.com/en-us/windows/win32/winmsg/windows


        #endregion Regionのことレギオンって読んでたけど、リージョンだったｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗｗ



        //タイマー用
        DispatcherTimer MyTimer;

        public MainWindow()
        {
            InitializeComponent();

            //タイマー初期化
            MyTimer = new DispatcherTimer();
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);//0.1秒間隔
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Start();

            this.Closing += (s, e) => { MyTimer.Stop(); };

        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            //キー入力取得用
            //Keyを仮想キーコードに変換
            int vKey1 = KeyInterop.VirtualKeyFromKey(Key.RightCtrl);
            int vKey2 = KeyInterop.VirtualKeyFromKey(Key.RightShift);
            //キーの状態を取得
            short key1state = GetAsyncKeyState(vKey1);
            short key2state = GetAsyncKeyState(vKey2);
            //右Ctrlキー＋右Shiftキーが押されていたら
            if ((key1state & 0x8000) >> 15 == 1 & ((key2state & 1) == 1))
            {
                //一番手前のウィンドウ
                IntPtr hWindowForeground = GetForegroundWindow();//ハンドル取得
                RECT rectWindowForeground;
                GetWindowRect(hWindowForeground, out rectWindowForeground);//Rect取得
                MyTextBlock1.Text = $"{rectWindowForeground} 一番手前(アクティブ)ウィンドウRect";

                //画面全体の画像からウィンドウRect範囲を切り出す
                if (rbScreen.IsChecked == true)
                {
                    var screenDC = GetDC(IntPtr.Zero);//画面全体のDC、コピー元
                    var memDC = CreateCompatibleDC(screenDC);//コピー先DC作成
                    int width = rectWindowForeground.right - rectWindowForeground.left;
                    int height = rectWindowForeground.bottom - rectWindowForeground.top;
                    var hBmp = CreateCompatibleBitmap(screenDC, width, height);//コピー先のbitmapオブジェクト作成
                    SelectObject(memDC, hBmp);//コピー先DCにbitmapオブジェクトを指定
                    //ビットブロック転送、コピー元からコピー先へ
                    BitBlt(memDC, 0, 0, width, height, screenDC, rectWindowForeground.left, rectWindowForeground.top, SRCCOPY);
                    //bitmapオブジェクトからbitmapSource作成
                    BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(hBmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                    //後片付け
                    DeleteObject(hBmp);
                    ReleaseDC(IntPtr.Zero, screenDC);
                    ReleaseDC(IntPtr.Zero, memDC);

                    //画像表示
                    MyImage.Source = source;
                }

                //ウィンドウDCからコピー
                else if (rbWindow.IsChecked == true)
                {
                    var screenDC = GetDC(hWindowForeground);//ウィンドウのDC
                    var memDC = CreateCompatibleDC(screenDC);
                    int width = rectWindowForeground.right - rectWindowForeground.left;
                    int height = rectWindowForeground.bottom - rectWindowForeground.top;
                    var hBmp = CreateCompatibleBitmap(screenDC, width, height);
                    SelectObject(memDC, hBmp);
                    BitBlt(memDC, 0, 0, width, height, screenDC, 0, 0, SRCCOPY);
                    var source = Imaging.CreateBitmapSourceFromHBitmap(hBmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                    DeleteObject(hBmp);
                    ReleaseDC(IntPtr.Zero, screenDC);
                    ReleaseDC(IntPtr.Zero, memDC);

                    MyImage.Source = source;
                }

                //ウィンドウDCからコピーしてアルファ値を255にする
                else if (rbWindowAlpah255.IsChecked == true)
                {
                    var screenDC = GetDC(hWindowForeground);//ウィンドウのDC
                    var memDC = CreateCompatibleDC(screenDC);
                    int width = rectWindowForeground.right - rectWindowForeground.left;
                    int height = rectWindowForeground.bottom - rectWindowForeground.top;
                    var hBmp = CreateCompatibleBitmap(screenDC, width, height);
                    SelectObject(memDC, hBmp);
                    BitBlt(memDC, 0, 0, width, height, screenDC, 0, 0, SRCCOPY);
                    var source = Imaging.CreateBitmapSourceFromHBitmap(hBmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                    DeleteObject(hBmp);
                    ReleaseDC(IntPtr.Zero, screenDC);
                    ReleaseDC(IntPtr.Zero, memDC);

                    //ピクセルフォーマットをBgr24に変換することでアルファ値を255に見立てている
                    source = new FormatConvertedBitmap(source, PixelFormats.Bgr24, source.Palette, 0);
                    MyImage.Source = source;
                }

            }
        }


    }
}
