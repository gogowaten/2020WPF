using System;
using System.Windows;
using System.Windows.Media.Imaging;

using System.Runtime.InteropServices;//APIのインポートで使っている
using System.Windows.Interop;//Imaging.CreateBitmapSourceFromHBitmapで使っている
using System.Windows.Threading;//DispatcherTimerで使っている

//表示しているマウスカーソル画像を取得表示してみた、WinAPIとWPF - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2020/11/20/214921

namespace _20201120_マウスカーソル画像取得
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region WindowsAPI^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       
        //座標取得用
        private struct POINT
        {
            public int X;
            public int Y;
        }

      
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
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);//0.1秒間隔
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Start();

            //アプリ終了時、タイマーストップ
            this.Closing += (s, e) => { MyTimer.Stop(); };
        }
        private void MyTimer_Tick(object sender, EventArgs e)
        {
            //カーソル画像
            //CURSORINFO：カーソルの表示の有無、カーソルのハンドル、座標
            //ICONINFO：trueでアイコン falseでカーソルの識別、ホットスポット座標、アイコンマスク画像のハンドル、アイコンカラー画像のハンドル

            //GetCursorから取得
            IntPtr hCursor = GetCursor();
            MyImageCursor.Source = FromIconHandle(hCursor);
            //GetCursor＋GetIconInfoから取得            
            GetIconInfo(hCursor, out ICONINFO iconInfo);
            MyImageCursorMask.Source = FromBitmapHandle(iconInfo.hbmMask);
            MyImageCursorColor.Source = FromBitmapHandle(iconInfo.hbmColor);


            //GetCursorInfoを使って取得
            var cInfo = new CURSORINFO();
            cInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            GetCursorInfo(out cInfo);
            MyImageCursorInfo.Source = FromIconHandle(cInfo.hCursor);
            //GetCursorInfo＋GetIconInfoから取得
            GetIconInfo(cInfo.hCursor, out ICONINFO corsorInfoIconInfo);
            MyImageCursorInfoMask.Source = FromBitmapHandle(corsorInfoIconInfo.hbmMask);
            MyImageCursorInfoColor.Source = FromBitmapHandle(corsorInfoIconInfo.hbmColor);

        }

        //カーソルやアイコンのハンドルからBitmapSourceを適当に作成
        //5～10分くらい経つと「値が期待された範囲内にありません」エラーが出ることがあるのでtry使った
        //原因がわからん
        private BitmapSource FromIconHandle(IntPtr hIcon)
        {
            BitmapSource source = null;
            if (hIcon == IntPtr.Zero) return source;
            try
            {
                source = Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception e)
            {
                MyTimer.Stop();                
                Console.WriteLine(e.Message);
                System.Threading.Thread.Sleep(1000);
                MyTimer.Start();
            }
           
            return source;
        }

        //Bitmapのハンドルから適当にBitmapSourceを作成
        private BitmapSource FromBitmapHandle(IntPtr hBitmap)
        {
            //hbmColorは存在しないことがあるので、そのときはnull            
            BitmapSource source = null;
            if (hBitmap == IntPtr.Zero) return source;
            try
            {
                source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception e)
            {
                MyTimer.Stop();
                Console.WriteLine(e);
                System.Threading.Thread.Sleep(1000);
                MyTimer.Start();
            }

            return source;
            //return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

    }
}
