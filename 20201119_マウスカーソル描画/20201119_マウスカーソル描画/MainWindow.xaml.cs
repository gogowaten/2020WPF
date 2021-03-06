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


//画像にマウスカーソル画像を重ねて表示、アルファブレンドとビット演算のANDとXOR - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2020/11/23/052201

namespace _20201119_マウスカーソル描画
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

        private BitmapSource MyBitmap;//全体画面保持用
        private RECT MyRect;//ウィンドウのRect

        //マウスカーソル
        private POINT MyCursorPoint;//座標        
        private int MyCursorHotspotX;//ホットスポット
        private int MyCursorHotspotY;//ホットスポット
        private BitmapSource MyBitmapCursor;//画像
        private BitmapSource MyBitmapCursorMask;//マスク画像
        private bool IsMaskUse;//マスク画像使用の有無判定用

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

            Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MyRadioBtnWithCursor.Click += (s, e) => { UpdateImage(); };
            MyRadioBtnWithoutCursor.Click += (s, e) => { UpdateImage(); };
        }

        //右Ctrlキー＋右Shiftキーが押されたら
        //全体画面取得
        //各RECT取得
        //キャプチャ画像更新
        private void MyTimer_Tick(object sender, EventArgs e)
        {
            //キー入力取得用
            //Keyを仮想キーコードに変換
            //int vKey1 = KeyInterop.VirtualKeyFromKey(Key.PrintScreen);
            int vKey1 = KeyInterop.VirtualKeyFromKey(Key.RightCtrl);
            int vKey2 = KeyInterop.VirtualKeyFromKey(Key.RightShift);
            //キーの状態を取得
            short key1state = GetAsyncKeyState(vKey1);
            short key2state = GetAsyncKeyState(vKey2);

            //keyが押されていたら
            if ((key2state & 1) == 1)
            //右Ctrlキー＋右Shiftキーが押されていたら
            //if ((key1state & 0x8000) >> 15 == 1 & ((key2state & 1) == 1))
            {

                //カーソル座標取得
                GetCursorPos(out MyCursorPoint);

                //カーソル画像取得
                SetCursorInfo();

                //画面全体画像取得
                MyBitmap = ScreenCapture();

                //RECT取得
                SetRect();

                UpdateImage();
            }
        }

        //ウィンドウのRECTを取得して保持
        private void SetRect()
        {
            //ウィンドウハンドルの取得
            //ウィンドウ名がついているもの(GetWindowTextの戻り値が0以外)を走査
            //最前面ウィンドウから開始、Parentへ10回まで辿っていく
            //見つからなかった場合は最前面ウィンドウ

            IntPtr hForeWnd = GetForegroundWindow();
            var wText = new StringBuilder(65535);
            int count = 0;
            IntPtr hWnd = hForeWnd;
            while (GetWindowText(hWnd, wText, 65535) == 0)
            {
                hWnd = GetParent(hWnd);
                count++;
                if (count > 10)
                {
                    hWnd = hForeWnd;
                    break;
                }
            }
            //見た目通りのRectを取得
            DwmGetWindowAttribute(hWnd,
                                  DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                                  out MyRect,
                                  Marshal.SizeOf(typeof(RECT)));
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


        /// <summary>
        /// 画像の切り抜き、RECTが画像の範囲を超えていたら調整する
        /// </summary>
        /// <param name="source">切り抜き対象画像</param>
        /// <param name="rect">左上隅と右下隅の座標</param>
        /// <returns></returns>
        private BitmapSource CroppedBitmapEx(BitmapSource source, RECT rect)
        {
            int left = (rect.left < 0) ? 0 : rect.left;
            int top = (rect.top < 0) ? 0 : rect.top;
            int width = rect.right - rect.left;
            if (left + width > source.PixelWidth) width = source.PixelWidth - left;
            int height = rect.bottom - rect.top;
            if (height > source.PixelHeight) height = source.PixelHeight - top;

            Int32Rect iRect = new Int32Rect(left, top, width, height);
            return new CroppedBitmap(source, iRect);
        }

        private void UpdateImage()
        {
            if (MyBitmap == null) return;

            if (MyRadioBtnWithCursor.IsChecked == true)
            {
                //BitmapSource source;
                //source = CursorOnScreen();
                MyImage.Source = CroppedBitmapEx(CursorOnScreen(), MyRect);
            }
            else if (MyRadioBtnWithoutCursor.IsChecked == true)
            {
                MyImage.Source = CroppedBitmapEx(MyBitmap, MyRect);
            }
        }

        /// <summary>
        /// マウスカーソルの情報をフィールドに格納
        /// </summary>
        private void SetCursorInfo()
        {
            CURSORINFO cInfo = new CURSORINFO();
            cInfo.cbSize = Marshal.SizeOf(cInfo);
            GetCursorInfo(out cInfo);
            GetIconInfo(cInfo.hCursor, out ICONINFO iInfo);
            //カーソル画像
            MyBitmapCursor =
                Imaging.CreateBitmapSourceFromHIcon(cInfo.hCursor,
                                                    Int32Rect.Empty,
                                                    BitmapSizeOptions.FromEmptyOptions());
            //カーソルマスク画像
            MyBitmapCursorMask =
                Imaging.CreateBitmapSourceFromHBitmap(iInfo.hbmMask,
                                                      IntPtr.Zero,
                                                      Int32Rect.Empty,
                                                      BitmapSizeOptions.FromEmptyOptions());
            //マスク画像を使うかどうかの判定
            //2色画像 かつ 高さが幅の2倍ならマスク画像使用
            IsMaskUse = (MyBitmapCursorMask.Format == PixelFormats.Indexed1) &
                (MyBitmapCursorMask.PixelHeight == MyBitmapCursorMask.PixelWidth * 2);

            //マスク画像のピクセルフォーマットはIndexed1なんだけど、計算しやすいようにBgra32に変換しておく
            MyBitmapCursorMask = new FormatConvertedBitmap(MyBitmapCursorMask,
                                                           PixelFormats.Bgra32,
                                                           null,
                                                           0);
           
            //ホットスポット保持
            MyCursorHotspotX = iInfo.xHotspot;
            MyCursorHotspotY = iInfo.yHotspot;


            MyImageCursor.Source = MyBitmapCursor;
            MyImageCursorMask.Source = MyBitmapCursorMask;


        }

        //画像の上にカーソル画像を合成
        private BitmapSource CursorOnScreen()//(BitmapSource source, BitmapSource cursor, POINT cursorLocate, int hotspotX, int hotspotY)
        {

            int width, height, stride;
            byte[] pixels;

            //マスクが必要なカーソルの場合
            if (IsMaskUse == true)
            {
                //カーソルマスク画像と合成
                //マスク画像の2枚は上下に連結された状態なので、上下に分割
                int maskWidth = MyBitmapCursorMask.PixelWidth;
                int maskHeight = MyBitmapCursorMask.PixelHeight / 2;
                //分割
                var mask1Bitmap = new CroppedBitmap(MyBitmapCursorMask,
                                              new Int32Rect(0, 0, maskWidth, maskHeight));
                var mask2Bitmap = new CroppedBitmap(MyBitmapCursorMask,
                                              new Int32Rect(0, maskHeight, maskWidth, maskHeight));
                //画素をbyte配列で取得
                int maskStride = (maskWidth * 32 + 7) / 8;
                byte[] mask1Pixels = new byte[maskHeight * maskStride];
                byte[] mask2Pixels = new byte[maskHeight * maskStride];
                mask1Bitmap.CopyPixels(mask1Pixels, maskStride, 0);
                mask2Bitmap.CopyPixels(mask2Pixels, maskStride, 0);

                //キャプチャ画像をbyte配列で取得
                width = MyBitmap.PixelWidth;
                height = MyBitmap.PixelHeight;
                stride = (width * 32 + 7) / 8;
                pixels = new byte[height * stride];
                MyBitmap.CopyPixels(pixels, stride, 0);

                //処理範囲の開始点と終了点設定、開始点はカーソルのホットスポットでオフセット
                int beginX = MyCursorPoint.X - MyCursorHotspotX;
                int beginY = MyCursorPoint.Y - MyCursorHotspotY;
                int endX = beginX + maskWidth;
                int endY = beginY + maskHeight;
                if (endX > width) endX = width;
                if (endY > height) endY = height;

                //最初にマスク画像上とAND合成、続けてマスク画像下とXOR
                int yCount = 0;
                for (int y = beginY; y < endY; y++)
                {
                    int xCount = 0;
                    for (int x = beginX; x < endX; x++)
                    {
                        int p = (y * stride) + (x * 4);
                        int pp = (yCount * maskStride) + (xCount * 4);
                        //AND
                        pixels[p] &= mask1Pixels[pp];
                        pixels[p + 1] &= mask1Pixels[pp + 1];
                        pixels[p + 2] &= mask1Pixels[pp + 2];
                        //XOR
                        pixels[p] ^= mask2Pixels[pp];
                        pixels[p + 1] ^= mask2Pixels[pp + 1];
                        pixels[p + 2] ^= mask2Pixels[pp + 2];

                        xCount++;
                    }
                    yCount++;
                }
            }

            //マスクが必要ない場合はアルファブレンドする
            else
            {
                //カーソル画像
                int cWidth = MyBitmapCursor.PixelWidth;
                int cHeight = MyBitmapCursor.PixelHeight;
                int maskStride = (cWidth * 32 + 7) / 8;
                byte[] cursorPixels = new byte[cHeight * maskStride];
                MyBitmapCursor.CopyPixels(cursorPixels, maskStride, 0);

                //キャプチャ画像
                width = MyBitmap.PixelWidth;
                height = MyBitmap.PixelHeight;
                stride = (width * 32 + 7) / 8;
                pixels = new byte[height * stride];
                MyBitmap.CopyPixels(pixels, stride, 0);

                //処理範囲の開始点と終了点設定
                int beginX = MyCursorPoint.X - MyCursorHotspotX;
                int beginY = MyCursorPoint.Y - MyCursorHotspotY;
                int endX = beginX + cWidth;
                int endY = beginY + cHeight;
                if (endX > width) endX = width;
                if (endY > height) endY = height;

                int yCount = 0;
                for (int y = beginY; y < endY; y++)
                {
                    int xCount = 0;
                    for (int x = beginX; x < endX; x++)
                    {
                        int p = (y * stride) + (x * 4);
                        int pp = (yCount * maskStride) + (xCount * 4);
                        //アルファブレンド
                        //                    効果
                        //http://www.charatsoft.com/develop/otogema/page/05d3d/effect.html
                        //求める画素値 = もとの画素値 + ((カーソル画素値 - もとの画素値) * (カーソルのアルファ値 / 255))
                        double alpha = cursorPixels[pp + 3] / 255.0;
                        byte r = pixels[p + 2];
                        byte g = pixels[p + 1];
                        byte b = pixels[p];
                        pixels[p + 2] = (byte)(r + ((cursorPixels[pp + 2] - r) * alpha));
                        pixels[p + 1] = (byte)(g + ((cursorPixels[pp + 1] - g) * alpha));
                        pixels[p] = (byte)(b + ((cursorPixels[pp] - b) * alpha));

                        xCount++;
                    }
                    yCount++;
                }
            }

            return BitmapSource.Create(width,
                                       height,
                                       MyBitmap.DpiX,
                                       MyBitmap.DpiY,
                                       MyBitmap.Format,
                                       MyBitmap.Palette,
                                       pixels,
                                       stride);

        }

        private void MyButtonToClipboard_Click(object sender, RoutedEventArgs e)
        {
            var image = MyImage.Source;
            if (image == null) return;
            Clipboard.SetImage((BitmapSource)image);
        }
    }
}
