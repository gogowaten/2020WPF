using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using System.Runtime.InteropServices;//Imagingで使っている
using System.Windows.Interop;//CreateBitmapSourceFromHBitmapで使っている
using System.Windows.Threading;//DispatcherTimerで使っている
using System.Text;
//WinAPIのGetAncestorでリボンメニューを開いた状態のエクセルウィンドウをキャプチャ - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2020/11/19/005250

namespace _20201117_エクセルのメニューごとキャプチャ
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
            DWMWA_EXTENDED_FRAME_BOUNDS,//ウィンドウのRect
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


        #endregion コピペ呪文ここまで^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        //タイマー用
        private DispatcherTimer MyTimer;

        private BitmapSource MyBitmap;//全体画面保持用

        //RECT保持用
        private RECT MyRectForeground;
        private RECT MyRectParent;
        private RECT[] MyRectRelated;
        private RECT[] MyRectAncestor;
        private RECT MyRectActive;

        public MainWindow()
        {
            InitializeComponent();

            Left = 580; Top = 480;

            //タイマー初期化
            MyTimer = new DispatcherTimer();
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);//0.1秒間隔
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Start();

            //アプリ終了時、タイマーストップ
            this.Closing += (s, e) => { MyTimer.Stop(); };

            //ComboBox初期化
            MyComboBox.ItemsSource = Enum.GetValues(typeof(GETWINDOW_CMD));
            MyComboBox.SelectedIndex = 0;

            MyComboBox2.ItemsSource = Enum.GetValues(typeof(GETANCESTOR_FLAGS));
            MyComboBox2.SelectedIndex = 0;

            MyRectRelated = new RECT[MyComboBox.Items.Count];
            MyRectAncestor = new RECT[MyComboBox2.Items.Count];

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MyComboBox.SelectionChanged += MyComboBox_SelectionChanged;
            MyComboBox2.SelectionChanged += MyComboBox2_SelectionChanged;
            rbForeground.Click += (s, e) => { UpdateImage(); };
            rbAncestor.Click += (s, e) => { UpdateImage(); };
            rbParent.Click += (s, e) => { UpdateImage(); };
            rbRelated.Click += (s, e) => { UpdateImage(); };
            rbActive.Click += (s, e) => { UpdateImage(); };
        }



        private void MyComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rbAncestor.IsChecked = true;
            UpdateImage();
        }

        private void MyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rbRelated.IsChecked = true;
            UpdateImage();
        }


        //右Ctrlキー＋右Shiftキーが押されたら
        //全体画面取得
        //各RECT取得
        //キャプチャ画像更新
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
                //画面全体画像取得
                MyBitmap = ScreenCapture();
                MyImage.Source = MyBitmap;

                

                //最前面ウィンドウのハンドル取得
                IntPtr hForeWnd = GetForegroundWindow();
                //ウィンドウ名表示
                var windowName = new StringBuilder(65535);
                int temp = GetWindowText(hForeWnd, windowName, 65535);
                MyTextBlockWindowText.Text = windowName.ToString();

                //各ウィンドウのRectを更新
                //最前面のウィンドウの見た目通りのRect
                DwmGetWindowAttribute(hForeWnd,
                                      DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                                      out MyRectForeground,
                                      Marshal.SizeOf(typeof(RECT)));
                //最前面のウィンドウの関連ウィンドウ取得
                var neko = MyComboBox.Items;
                for (int i = 0; i < neko.Count; i++)
                {
                    GETWINDOW_CMD cmd = (GETWINDOW_CMD)neko[i];
                    IntPtr hw = GetWindow(hForeWnd, cmd);
                    DwmGetWindowAttribute(hw, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out MyRectRelated[i], Marshal.SizeOf(typeof(RECT)));
                }
                //最前面のウィンドウの親ウィンドウ
                IntPtr hWndParent = GetParent(hForeWnd);
                DwmGetWindowAttribute(hWndParent, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out MyRectParent, Marshal.SizeOf(typeof(RECT)));
                //最前面のウィンドウの祖先Rect
                neko = MyComboBox2.Items;
                for (int i = 0; i < neko.Count; i++)
                {
                    IntPtr hw = GetAncestor(hForeWnd, (GETANCESTOR_FLAGS)neko[i]);
                    DwmGetWindowAttribute(hw, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out MyRectAncestor[i], Marshal.SizeOf(typeof(RECT)));
                }

                MyTextBlock1.Text = $"{MyRectForeground} 最前面のウィンドウ";
                MyTextBlock2.Text = $"{MyRectRelated[MyComboBox.SelectedIndex]} 最前面の関連ウィンドウ";
                MyTextBlock3.Text = $"{MyRectParent} 最前面の親ウィンドウ";
                MyTextBlock4.Text = $"{MyRectAncestor[MyComboBox2.SelectedIndex]} 最前面の祖先ウィンドウ";


                //アクティブウィンドウテスト
                IntPtr aw = GetActiveWindow();
                DwmGetWindowAttribute(aw, DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, out MyRectActive, Marshal.SizeOf(typeof(RECT)));
                MyTextBlock5.Text = $"{MyRectActive} アクティブウィンドウ";

                //表示画像更新
                UpdateImage();
            }
        }

        //仮想画面全体の画像取得
        private BitmapSource ScreenCapture()
        {
            var screenDC = GetDC(IntPtr.Zero);//仮想画面全体のDC、コピー元
            var memDC = CreateCompatibleDC(screenDC);//コピー先DC作成
            int width = (int)SystemParameters.VirtualScreenWidth;
            int height = (int)SystemParameters.VirtualScreenHeight;
            var hBmp = CreateCompatibleBitmap(screenDC, width, height);//コピー先のbitmapオブジェクト作成
            SelectObject(memDC, hBmp);//コピー先DCにbitmapオブジェクトを指定

            //コピー元からコピー先へビットブロック転送
            //通常のコピーなのでSRCCOPYを指定
            BitBlt(memDC, 0, 0, width, height, screenDC, 0, 0, SRCCOPY);
            //bitmapオブジェクトからbitmapSource作成
            BitmapSource source =
                Imaging.CreateBitmapSourceFromHBitmap(hBmp,
                                                      IntPtr.Zero,
                                                      Int32Rect.Empty,
                                                      BitmapSizeOptions.FromEmptyOptions());
            //後片付け
            DeleteObject(hBmp);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);

            //画像
            return source;
        }

        private Int32Rect MakeCropRect(RECT rect)
        {
            return new Int32Rect(rect.left,
                                 rect.top,
                                 rect.right - rect.left,
                                 rect.bottom - rect.top);
        }

        private BitmapSource CaptureWindow()
        {
            //画面全体画像取得
            BitmapSource screenBitmap = ScreenCapture();

            //最前面のウィンドウのハンドル取得
            IntPtr hForeWnd = GetForegroundWindow();

            //最前面のウィンドウの祖先ウィンドウのハンドル取得
            IntPtr hAncestorWnd = GetAncestor(hForeWnd, GETANCESTOR_FLAGS.GA_ROOTOWNER);

            //ウィンドウ枠を含む見た目通りのRECTを取得
            DwmGetWindowAttribute(hAncestorWnd,
                                  DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                                  out RECT rectAncestor,
                                  Marshal.SizeOf(typeof(RECT)));

            //RECTから切り抜き用のRect作成
            Int32Rect myRect = MakeCropRect(rectAncestor);

            //全体画面画像から切り抜き
            return new CroppedBitmap(screenBitmap, myRect);

        }

        //ウィンドウキャプチャ
        //画面全体画像からウィンドウのRect領域を切り抜いて表示
        private void UpdateImage()
        {
            Int32Rect cropRect;
            //RECT選択
            if (rbForeground.IsChecked == true)
            {
                cropRect = MakeCropRect(MyRectForeground);
            }
            else if (rbParent.IsChecked == true)
            {
                cropRect = MakeCropRect(MyRectParent);
            }
            else if (rbRelated.IsChecked == true)
            {
                RECT rECT = MyRectRelated[MyComboBox.SelectedIndex];
                MyTextBlock2.Text = $"{rECT} 最前面の関連ウィンドウ";
                cropRect = MakeCropRect(rECT);
            }
            else if (rbAncestor.IsChecked == true)
            {
                RECT rECT = MyRectAncestor[MyComboBox2.SelectedIndex];
                MyTextBlock4.Text = $"{rECT} 最前面の祖先ウィンドウ";
                cropRect = MakeCropRect(rECT);
            }
            else if (rbActive.IsChecked == true)
            {
                cropRect = MakeCropRect(MyRectActive);
            }

            //表示画像更新
            if (cropRect.IsEmpty)
            {
                MyImage.Source = null;
            }
            else
            {
                //全体画像から切り抜いた画像を表示
                MyImage.Source = new CroppedBitmap(MyBitmap, cropRect);
            }

        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = null;
            MyTextBlock1.Text = "";
            MyTextBlock2.Text = string.Empty;
            MyTextBlock3.Text = null;
            MyTextBlock4.Text = string.Empty;
            MyTextBlock5.Text = string.Empty;
        }

        private void MyButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Clipboard.SetImage((BitmapSource)MyImage.Source);
        }
    }
}
