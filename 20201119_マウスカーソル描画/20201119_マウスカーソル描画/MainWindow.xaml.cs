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

using System.Runtime.InteropServices;//Imagingで使っている
using System.Windows.Interop;//CreateBitmapSourceFromHBitmapで使っている
using System.Windows.Threading;//DispatcherTimerで使っている

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
        private RECT MyRectAncestor;//ウィンドウのRect
        private POINT MyCursorPoint;//キャプチャ時のカーソル座標
        private BitmapSource MyCursorBitmap;//マウスカーソル画像

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

            //右Ctrlキーが押されたら
            if ((key1state & 1) == 1)
            //右Ctrlキー＋右Shiftキーが押されていたら
            //if ((key1state & 0x8000) >> 15 == 1 & ((key2state & 1) == 1))
            {
                //画面全体画像取得
                MyBitmap = ScreenCapture();
                MyImage.Source = MyBitmap;

                //最前面のアプリのウィンドウの見た目上のRECT取得                
                DwmGetWindowAttribute(
                    GetAncestor(
                        GetForegroundWindow(),
                        GETANCESTOR_FLAGS.GA_ROOTOWNER),
                    DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                    out MyRectAncestor,
                    Marshal.SizeOf(typeof(RECT)));


                GetCursorPos(out MyCursorPoint);
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


        //クロップ用のRect作成、左上座標がマイナスのときは0に変換して作成
        private Int32Rect MakeCropRect(RECT rect)
        {
            int left = (rect.left < 0) ? 0 : rect.left;
            int top = (rect.top < 0) ? 0 : rect.top;
            return new Int32Rect(left,
                                 top,
                                 rect.right - left,
                                 rect.bottom - top);
        }
        private void UpdateImage()
        {
            if (MyRadioBtnWithCursor.IsChecked == true)
            {
                //カーソル画像
                //CURSORINFO：カーソルの表示の有無、カーソルのハンドル、座標
                //ICONINFO：trueでアイコン falseでカーソルの識別、ホットスポット座標、アイコンマスク画像のハンドル、アイコンカラー画像のハンドル

                //GetCursor
                IntPtr hCursor = GetCursor();
                BitmapSource cursorBitmap = Imaging.CreateBitmapSourceFromHIcon(hCursor, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                GetIconInfo(hCursor, out ICONINFO iconInfo);
                BitmapSource corsorMaskBitmap = Imaging.CreateBitmapSourceFromHBitmap(iconInfo.hbmMask, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                BitmapSource corsorColorBitmap = Imaging.CreateBitmapSourceFromHBitmap(iconInfo.hbmColor, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());


                //GetCursorInfo
                var cInfo = new CURSORINFO();
                cInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                GetCursorInfo(out cInfo);
                BitmapSource cursorInfoBitmap = Imaging.CreateBitmapSourceFromHIcon(cInfo.hCursor, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                GetIconInfo(cInfo.hCursor, out ICONINFO corsorInfoIconInfo);
                BitmapSource cursorInfoMaskBitmap = Imaging.CreateBitmapSourceFromHBitmap(corsorInfoIconInfo.hbmMask, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                BitmapSource cursorInfoColorBitmap;
                if (corsorInfoIconInfo.hbmColor != IntPtr.Zero)
                {
                    cursorInfoColorBitmap = Imaging.CreateBitmapSourceFromHBitmap(corsorInfoIconInfo.hbmColor, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }

                BitmapSource source = MixBitmap(MyBitmap, cursorBitmap, MyCursorPoint, 0, 0);
                MyImage.Source = new CroppedBitmap(source, MakeCropRect(MyRectAncestor));


            }
            else if (MyRadioBtnWithoutCursor.IsChecked == true)
            {
                MyImage.Source = new CroppedBitmap(MyBitmap, MakeCropRect(MyRectAncestor));

            }
        }

        private BitmapSource MixBitmap(BitmapSource source, BitmapSource cursor, POINT cursorLocate, int hotspotX, int hotspotY)
        {
            BitmapSource resultBmp = null;

            //ピクセルフォーマットをbgra32に変換して計算しやすくする            
            cursor = new FormatConvertedBitmap(cursor, PixelFormats.Bgra32, null, 0);
            int cWidth = cursor.PixelWidth;
            int cHeight = cursor.PixelHeight;
            int maskStride = (cWidth * 32 + 7) / 8;
            byte[] cursorPixels = new byte[cHeight * maskStride];
            cursor.CopyPixels(cursorPixels, maskStride, 0);

            int w = source.PixelWidth;
            int h = source.PixelHeight;
            int bpp = source.Format.BitsPerPixel;
            int stride = (w * bpp + 7) / 8;
            byte[] pixels = new byte[h * stride];
            source.CopyPixels(pixels, stride, 0);

            int beginX = cursorLocate.X;
            int beginY = cursorLocate.Y;
            int endX = beginX + cWidth;
            int endY = beginY + cHeight;
            if (endX > w) endX = w;
            if (endY > h) endY = h;

            int yCount = 0;
            int xCount = 0;
            for (int y = beginY; y < endY; y++)
            {
                for (int x = beginX; x < endX; x++)
                {
                    var p = y * stride + x * 4;
                    var pp = yCount * maskStride + xCount * 4;

                    if (cursorPixels[pp + 3] != 0)
                    {
                        //不透明部分はカーソルで上書き
                        if (cursorPixels[pp + 3] == 255)
                        {
                            pixels[p] = cursorPixels[pp];
                            pixels[p + 1] = cursorPixels[pp + 1];
                            pixels[p + 2] = cursorPixels[pp + 2];
                        }
                        //半透明部分は合成
                        else
                        {
                            double maskA = 1 - (cursorPixels[pp + 3] / 255.0);
                            pixels[p] = (byte)(pixels[p] * maskA);
                            pixels[p + 1] = (byte)(pixels[p + 1] * maskA);
                            pixels[p + 2] = (byte)(pixels[p + 2] * maskA);
                        }

                    }
                    //else
                    //{
                    //    pixels[p] = mask1Pixels[pp];
                    //    pixels[p+1] = mask1Pixels[pp+1];
                    //    pixels[p+2] = mask1Pixels[pp+2];
                    //}


                    ////カーソルの完全不透明部分はカーソルで上書き
                    ////白以外ならアルファ値で引き算

                    ////透明以外は上書き
                    //byte maskAlpha = mask1Pixels[pp + 3];
                    //if (maskAlpha == 255)
                    //{
                    //    pixels[p] = mask1Pixels[pp];
                    //    pixels[p + 1] = mask1Pixels[pp + 1];
                    //    pixels[p + 2] = mask1Pixels[pp + 2];
                    //    //pixels[p + 3] = 255;// mask1Pixels[pp + 3];
                    //}
                    //else
                    //{
                    //    pixels[p] = pixels[p];
                    //    pixels[p + 1] = mask1Pixels[pp + 1];
                    //    pixels[p + 2] = mask1Pixels[pp + 2];
                    //    pixels[p + 3] = 255;// mask1Pixels[pp + 3];
                    //}

                    ////カーソルが黒なら画像も黒にする
                    //if (mask1Pixels[pp] == 0)
                    //{
                    //    pixels[p] = 0;
                    //    pixels[p + 1] = 0;
                    //    pixels[p + 2] = 0;
                    //}

                    xCount++;
                }
                yCount++;
                xCount = 0;
            }

            return BitmapSource.Create(w, h, source.DpiX, source.DpiY, source.Format, source.Palette, pixels, stride);

        }
        private BitmapSource MixBitmap2(BitmapSource source, BitmapSource cursor, POINT cursorLocate, int xOffset, int yOffset)
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
    }
}
