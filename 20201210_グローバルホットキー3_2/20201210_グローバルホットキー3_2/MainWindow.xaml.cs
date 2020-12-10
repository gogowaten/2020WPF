using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;

//Nine Works　WPFでHotKeyを設定する方法
//http://nineworks2.blog.fc2.com/blog-entry-17.html
//ここからのコピペ改変

//グローバルホットキーに任意のキーを登録
//修飾キーはチェックボックスで指定、普通のキーはコンボボックスから選択、もしくはコンボボックス上でキーを押す
//登録は登録ボタン
namespace _20201210_グローバルホットキー3_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //登録用ID
        private const int HOTKEY_ID1 = 0x0001;
        private const int HOTKEY_ID2 = 0x0002;

        private const int WM_HOTKEY = 0x0312;

        private IntPtr MyWindowHandle;

        //必要なAPI参照
        [DllImport("user32.dll")]
        private static extern int RegisterHotKey(IntPtr hWnd, int id, int modKey, int vKey);
        [DllImport("user32.dll")]
        private static extern int UnregisterHotKey(IntPtr hWnd, int id);


        public MainWindow()
        {
            InitializeComponent();

            var host = new WindowInteropHelper(this);
            MyWindowHandle = host.Handle;

            //コンボボックス初期化
            MyComboBoxKey.ItemsSource = Enum.GetValues(typeof(Key));
            MyComboBoxKey.SelectedIndex = 0;

            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;

            this.Closed += MainWindow_Closed;
        }

        //アプリ終了時に登録解除
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            UnregisterHotKey(MyWindowHandle, HOTKEY_ID1);
            UnregisterHotKey(MyWindowHandle, HOTKEY_ID2);
            ComponentDispatcher.ThreadPreprocessMessage -= ComponentDispatcher_ThreadPreprocessMessage;
        }


        //ホットキーが押されたときの動作
        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message != WM_HOTKEY) return;

            switch (msg.wParam.ToInt32())
            {
                case HOTKEY_ID1:
                    MessageBox.Show("HotKey1");
                    break;
                case HOTKEY_ID2:
                    MessageBox.Show("HotKey2");
                    break;

                default:
                    break;
            }
        }



        //ホットキー登録
        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            int modifier = 0;
            string str = "";
            if (MyChecAlt.IsChecked == true) { str += $" + Alt"; modifier = (int)ModifierKeys.Alt; }
            if (MyChecCtrl.IsChecked == true) { str += $" + Ctrl"; modifier += (int)ModifierKeys.Control; }
            if (MyCheckShift.IsChecked == true) { str += $" + Shift"; modifier += (int)ModifierKeys.Shift; }
            if (MyCheckWin.IsChecked == true) { str += $" + Win"; modifier += (int)ModifierKeys.Windows; }

            var key = (Key)MyComboBoxKey.SelectedValue;
            UnregisterHotKey(MyWindowHandle, HOTKEY_ID1);
            if (RegisterHotKey(MyWindowHandle, HOTKEY_ID1, modifier, KeyInterop.VirtualKeyFromKey(key)) == 0)
            {
                MessageBox.Show("登録に失敗");
            }
            else
            {
                str += $" + {key}";
                str = str.Remove(0, 3);
                MessageBox.Show($"{str} を登録しました");
            }
        }


        //コンボボックス上でキーを押し下げたとき
        //入力されたキー文字は無視
        private void MyComboBoxKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;//無視む～し
        }

        //コンボボックス上でキーが上げられたとき
        //修飾キー以外なら、そのキーと同じキーをコンボボックスで選択する
        //文字は無視
        private void MyComboBoxKey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            if ((key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LWin || key == Key.RWin) == false)
            {
                MyComboBoxKey.SelectedValue = key;
            }

            e.Handled = true;
        }
    }
}
