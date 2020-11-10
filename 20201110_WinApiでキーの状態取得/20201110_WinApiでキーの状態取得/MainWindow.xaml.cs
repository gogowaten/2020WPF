using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace _20201110_WinApiでキーの状態取得
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //必要なAPIはGetAsyncKeyStateだけなんだけど
        //似たようなGetKeyStateも使ってみた
        [DllImport("user32.dll")]
        private extern static short GetAsyncKeyState(int vKey);
        [DllImport("user32.dll")]
        private extern static short GetKeyState(int vKey);

        //状態を知りたいキーの仮想キーコードを渡す
        //戻り値の型はshortで、これを2進数にして最上位ビットと最下位ビットで判定する
        //GetAsyncKeyState
        //最上位ビットが1のとき、押されている状態を表す
        //最下位ビットが1のとき、前回取得時から今回までに押された形跡があることを表す
        //  2進数               10進数(最上位、最下位以外が0のとき)
        // 0xxx_xxxx_xxxx_xxx0       0  押されてない、押された形跡無し
        // 0xxx_xxxx_xxxx_xxx1       1  押されてない、押された形跡あり
        // 1xxx_xxxx_xxxx_xxx0  -32768  押されている、押された形跡無し
        // 1xxx_xxxx_xxxx_xxx1  -32767  押されている、押された形跡あり
        //↑のxのところは0か1か決まっていないようだけど、見ていた感じでは常に0だった

        //ビットの判定はビット演算のandを使う
        //最下位ビットの判定、(戻り値 & 1) これが1なら1
        //最上位ビットの判定、(戻り値 & 0x8000)してこれを右に15シフトして1なら1
        //あとは、short型で最上位ビットが1ならマイナスの値になるはず？なので、戻り値 < 0 なら1

        //GetKeyState
        //最上位ビットが1のとき、押されている状態
        //最下位ビットはトグル式のキー(CapsLockとか)用で1のとき、トグルオン状態
        //戻り値はshortのはずなんだけど見た感じだとsbyteっぽい(-128～127)
        //なので最上位ビットの判定はは8桁目で行った

        //違い
        //GetAsyncKeyStateは押された形跡がわかる
        //GetKeyStateはトグル式のキーのトグル状態がわかる

        //イマイチ
        //GetAsyncKeyStateはアプリによっては取得できない(無反応？)
        //タスクマネージャー、エクセル、デスクトップ検索のEverythingなど

        private DispatcherTimer MyTimer;
        private int MyCount;
        public MainWindow()
        {
            InitializeComponent();

            //タイマー初期化
            MyTimer = new DispatcherTimer();
            MyTimer.Tick += MyTimer_Tick;
            //時間間隔、8ミリ秒にしてみた
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 8);
            MyTimer.Start();


            //キー選択用のコンボボックスの初期化
            MyCombo1Key.Items.Add(Key.RightCtrl);
            MyCombo1Key.Items.Add(Key.RightShift);
            MyCombo1Key.Items.Add(Key.RightAlt);
            MyCombo1Key.Items.Add(Key.Right);

            MyCombo2Key.Items.Add(Key.RightCtrl);
            MyCombo2Key.Items.Add(Key.RightShift);
            MyCombo2Key.Items.Add(Key.Right);
            MyCombo2Key.Items.Add(Key.A);

            MyCombo1Key.SelectedIndex = 1;
            MyCombo2Key.SelectedIndex = 3;
        }

        //一定時間間隔でキーの状態を取得して表示
        private void MyTimer_Tick(object sender, EventArgs e)
        {
            //KeyをWindowsAPIで使う仮想キーコードに変換
            var vKey1 = KeyInterop.VirtualKeyFromKey((Key)MyCombo1Key.SelectedItem);
            var vKey2 = KeyInterop.VirtualKeyFromKey((Key)MyCombo2Key.SelectedItem);

            //GetAsyncKeyStateでキーの状態を取得して値を表示
            short key1AsyncState = GetAsyncKeyState(vKey1);
            short key2AsyncState = GetAsyncKeyState(vKey2);
            MyTextBlockKey1AsyncState.Text = "AsyncKey= " + key1AsyncState.ToString();
            MyTextBlockKey2AsyncState.Text = "AsyncKey= " + key2AsyncState.ToString();

            //GetKeyStateでキーの状態を取得して値を表示
            short key1State = GetKeyState(vKey1);
            short key2State = GetKeyState(vKey2);
            MyTextBlockKey1State.Text = "Key= " + key1State.ToString();
            MyTextBlockKey2State.Text = "Key= " + key2State.ToString();

            //Key1が押された状態で、Key2も押されていたらの判定
            // != 0 この判定の仕方は雑だけど問題なさそう
            if (key1AsyncState != 0 & (key2AsyncState & 1) == 1)
            {
                //カウントを増やして回数の表示を更新
                MyCount++;
                MyTextBlockCount.Text = MyCount.ToString() + "回";
            }

            //↑の雑じゃない判定版
            //if (((key1AsyncState & 0x8000) >> 15 == 1) & ((key2AsyncState & 1) == 1)) { }

            //GetKeyStateとGetAsyncKeyState版でもできた
            //if ((key1State & 0x80) >> 7 == 1 & (key2AsyncState & 1)== 1)
            //{
            //    MyCount++;
            //    MyTextBlockCount.Text = MyCount.ToString() + "回";
            //}

            //GetkeyStateだけだとできなかった
            //両キーとも押しっぱなしのときしか判定されない
            //if ((key1State & 0x80) >> 7 == 1 & (key2State & 0x80) >> 7 == 1)
            //{
            //    MyCount++;
            //    MyTextBlockCount.Text = MyCount.ToString() + "回";
            //}

            //GetAsynckeyStateだけ → できた
            //GetkeyStateとGetAsynckeyState → できた
            //GetkeyStateだけ → むり？

        }

        //カウントリセット
        private void MyButtonCountReset_Click(object sender, RoutedEventArgs e)
        {
            MyCount = 0;
            MyTextBlockCount.Text = MyCount.ToString();
        }
    }
}
