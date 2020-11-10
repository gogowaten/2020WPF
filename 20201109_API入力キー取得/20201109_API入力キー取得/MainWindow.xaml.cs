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
using System.Windows.Threading;

namespace _20201109_API入力キー取得
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer MyTimer;
        private int MyCount;
        public MainWindow()
        {
            InitializeComponent();

            MyTimer = new DispatcherTimer();
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            MyTimer.Start();

            byte[] keys = new byte[256];
            WinAPI.GetKeyboardState(keys);

            //WPF Key を Win32 仮想キーに変換
            var k = System.Windows.Input.KeyInterop.VirtualKeyFromKey(Key.CapsLock);
            //どちらかのShiftキー(0x10)を、wpfのkeyにしたらleftShiftになった
            var vk = KeyInterop.KeyFromVirtualKey(0x10);

            MyComboBox.Items.Add(Key.RightShift);
            MyComboBox.Items.Add(Key.RightCtrl);

        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            //仮想キーコードvirtual key
            //プリントスクリーンキー(0x2C)で判定
            //右コントロールキー(0xA3)
            //CapsLockキー(0x14)

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


            ////前回から押された形跡があった場合、最下位ビットが1
            //if ((GetAsyncKeyState(0xA3) & 1) == 1)
            //{
            //    BitmapSource maskBitmap = GetIconMaskBitmap();
            //    BitmapSource screenBitmap = CaptureClientWithoutCursorFromDesktop();
            //    ICONINFO info = GetMyIconInfo2();
            //    //ICONINFO info = GetMyIconInfo();
            //    MyImage.Source = MixBitmap(screenBitmap, maskBitmap, GetCursorPositionInClient(), info.xHotspot, info.yHotspot);

            //    Beep(1500, 10);
            //}

            //値と押された回数表示
            //short keyState = WinAPI.GetAsyncKeyState(0xA3);
            //TextBlockGetKeyState.Text = WinAPI.GetKeyState(0x14).ToString();
            //if (keyState != 0)
            //{
            //    //得られた値を表示
            //    TextBlockKeyState.Text = keyState.ToString();

            //    //検知回数表示、形跡回数
            //    if ((keyState & 1) == 1)
            //    {
            //        MyCount++;
            //        TextBlockCount.Text = MyCount.ToString();
            //    }
            //}

            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //なんかのキー(key1)を押しながら、別のキー(key2)押したときの検知
            //key2にトグル式のキー(CapsLockとか)を指定するとうまく動かない
            var key1 = 0x11;//11：どちらかのctrl、10：どちらかのShift、91：Scroll Lock
            var key2 = 0x25;//25：カーソルキー左
            short key1AsyncState = WinAPI.GetAsyncKeyState(key1);
            short key2AsyncState = WinAPI.GetAsyncKeyState(key2);
            short key1State = WinAPI.GetKeyState(key1);
            short key2State = WinAPI.GetKeyState(key2);

            TextBlockKey1AsyncState.Text = key1AsyncState.ToString();
            TextBlockKey2AsyncState.Text = key2AsyncState.ToString();
            TextBlockKey1State.Text = key1State.ToString();
            TextBlockKey2State.Text = key2State.ToString();


            if (key1AsyncState != 0 & (key2AsyncState & 1) == 1)
            {
                //WinAPI.Beep(1000, 10);
                MyCount++;
                TextBlockCount.Text = MyCount.ToString();
            }

            //↑でも↓でも同じ結果に見える、上の方が書くのラク

            //if (((key1AsyncState & 0x8000) >> 15 == 1) & ((key2AsyncState & 1) == 1))
            //{
            //    WinAPI.Beep(1000, 10);
            //    MyCount++;
            //    TextBlockCount.Text = MyCount.ToString();
            //}

            ////GetkeyStateとGetAsynckeyStateの組み合わせでもできた
            //if ((key1State & 0x80) >> 7 == 1 & (key2AsyncState & 1) == 1)
            //{
            //    MyCount++;
            //    TextBlockCount.Text = MyCount.ToString();
            //}

            ////失敗例、完全に同時に押されたか、2キーとも押しっぱなしていないとカウントされない
            //if ((key1State & 0x80) >> 7 == 1 & (key2State & 0x80) >> 7 == 1)
            //{
            //    MyCount++;
            //    TextBlockCount.Text = MyCount.ToString();
            //}

            //GetAsynckeyStateとGetAsynckeyState → できた
            //GetkeyStateとGetAsynckeyState → できた
            //GetkeyStateとGetkeyState → むり？

            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }

        private void MyButtonTest_Click(object sender, RoutedEventArgs e)
        {
            var rc = Key.RightCtrl;
            var rcv = KeyInterop.VirtualKeyFromKey(rc);
            var neko = MyComboBox.SelectedItem;
            var inu = KeyInterop.VirtualKeyFromKey((Key)MyComboBox.SelectedItem);
        }


    }
    public static class WinAPI
    {
        //指定したキーの今の状態や、前回の確認から押されたかどうかを取得
        [DllImport("user32.dll")]
        public extern static short GetAsyncKeyState(int vKey);

        //指定したキーの状態を取得、型はshortだけど最上位ビットは8桁目になっている？-128や-127が帰ってくる
        //戻り値の最上位ビットが1ならキーが押された状態を表す
        //戻り値の最下位ビットはキーのトグル状態で1ならオン、0ならオフ状態を表す、NumLockやCapsLockなどで使う
        [DllImport("user32.dll")]
        public extern static short GetKeyState(int nVirtKey);

        //すべてのキーの状態を取得
        //最上位ビットが1の場合、キーが押された状態
        //最下位ビットはCapsLockキーなどのトグルキー専用、1ならトグルオンの状態
        //lpKeyStateは受け取るだけだから、本当はrefとかoutをつけるんだけど、そうすると実行時にアプリが落ちる
        [DllImport("user32.dll")]
        public extern static bool GetKeyboardState(byte[] lpKeyState);

        //不明、KeyboardStateのセットができるみたい？
        //セットしても変化なし
        [DllImport("user32.dll")]
        public extern static bool SetKeyboardState(ref byte[] lpKeyState);

        [DllImport("user32.dll")]
        public extern static uint SendInput(uint cInputs, Input pInputs, int cbSize);
        public struct MouseInput
        {
            public int dx;
            public int dy;
            public short mouseData;
            public short dwFlags;
            public short time;
            public IntPtr dwExtraInfo;
        }
        public struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public short dwFlags;
            public short time;
            public IntPtr dwExtraInfo;
        }
        public struct HardwareInput
        {
            public short uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
        public struct InputUnion
        {
            public MouseInput Mouse;
            public KeyboardInput Keyboard;
            public HardwareInput Hardware;
        }
        public struct Input
        {
            public short Type;
            public InputUnion union;
        }



        //
        [DllImport("kernel32.dll")]
        public extern static bool Beep(uint dwFreq, uint dwDuration);

    }
}
