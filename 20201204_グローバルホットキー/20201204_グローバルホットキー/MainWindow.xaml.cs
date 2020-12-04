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
using System.Windows.Interop;

//WPFでホットキーの登録 - SourceChord
//http://sourcechord.hatenablog.com/entry/2017/02/13/005456
//ここのコピペ

namespace _20201204_グローバルホットキー
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HotKeyHelper _hotkey;

        public MainWindow()
        {
            InitializeComponent();

            this._hotkey = new HotKeyHelper(this);
            //this._hotkey.Register(ModifierKeys.Control, Key.A, (_, __) => { MessageBox.Show("HotKey"); });
            this._hotkey.Register(ModifierKeys.Control, Key.A, (s, e) => MessageBox.Show($"HotKey\n{s}"));

            Closed += (s, e) => { this._hotkey.Dispose(); };
        }
        
    }



    public class HotKeyItem
    {
        public ModifierKeys ModifierKeys { get; private set; }
        public Key Key { get; private set; }
        public EventHandler Handler { get; private set; }
        public HotKeyItem(ModifierKeys modKey, Key key, EventHandler handler)
        {
            this.ModifierKeys = modKey;
            this.Key = key;
            this.Handler = handler;
        }
    }

    public class HotKeyHelper : IDisposable
    {
        private IntPtr _windowHandle;
        private Dictionary<int, HotKeyItem> _hotkeyList = new Dictionary<int, HotKeyItem>();

        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern int RegisterHotKey(IntPtr hWnd, int id, int modKey, int vKey);
        [DllImport("user32.dll")]
        private static extern int UnregisterHotKey(IntPtr hWnd, int id);

        public HotKeyHelper(Window window)
        {
            var host = new WindowInteropHelper(window);//using System.Windows.Interop;
            this._windowHandle = host.Handle;

            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }

        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message != WM_HOTKEY) { return; }
            var id = msg.wParam.ToInt32();
            var hotkey = this._hotkeyList[id];

            hotkey?.Handler
                ?.Invoke(this, EventArgs.Empty);
        }

        private int _hotkeyID = 0x0000;
        private bool disposedValue;
        private const int MAX_HOTKEY_ID = 0xC000;

        public bool Register(ModifierKeys modkey, Key key, EventHandler handler)
        {
            var modKeyNum = (int)modkey;
            var vKey = KeyInterop.VirtualKeyFromKey(key);

            while (this._hotkeyID < MAX_HOTKEY_ID)
            {
                var ret = RegisterHotKey(this._windowHandle, this._hotkeyID, modKeyNum, vKey);

                if (ret != 0)
                {
                    var hotkey = new HotKeyItem(modkey, key, handler);
                    this._hotkeyList.Add(this._hotkeyID, hotkey);
                    this._hotkeyID++;
                    return true;
                }
                this._hotkeyID++;
            }

            return false;
        }

        #region Unregister
        public bool Unregister(int id)
        {
            var ret = UnregisterHotKey(this._windowHandle, id);
            return ret == 0;
        }

        public bool Unregister(ModifierKeys modKey, Key key)
        {
            var item = this._hotkeyList.FirstOrDefault(x => x.Value.ModifierKeys == modKey && x.Value.Key == key);
            var isFound = !item.Equals(default(KeyValuePair<int, HotKeyItem>));
            if (isFound)
            {
                var ret = Unregister(item.Key);
                if (ret)
                {
                    this._hotkeyList.Remove(item.Key);
                }
                return ret;
            }
            else
            {
                return false;
            }
        }

        public bool UnregisterAll()
        {
            var result = true;
            foreach (var item in this._hotkeyList)
            {
                result &= this.Unregister(item.Key);
            }
            return result;
        }
        #endregion

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                this.UnregisterAll();
                disposedValue = true;
            }
        }

        // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~HotKeyHelper()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(false);
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
