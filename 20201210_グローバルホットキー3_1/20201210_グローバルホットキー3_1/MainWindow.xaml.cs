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
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace _20201210_グローバルホットキー3_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int HOTKEY_ID1 = 0x0001;
        private const int HOTKEY_ID2 = 0x0002;

        private const int WM_HOTKEY = 0x0312;

        private IntPtr MyWindowHandle;

        [DllImport("user32.dll")]
        private static extern int RegisterHotKey(IntPtr hWnd, int id, int modKey, int vKey);
        [DllImport("user32.dll")]
        private static extern int UnregisterHotKey(IntPtr hWnd, int id);


        public MainWindow()
        {
            InitializeComponent();

            var host = new WindowInteropHelper(this);
            MyWindowHandle = host.Handle;

            MyInitialize();

            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;

            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            UnregisterHotKey(MyWindowHandle, HOTKEY_ID1);
            UnregisterHotKey(MyWindowHandle, HOTKEY_ID2);
            ComponentDispatcher.ThreadPreprocessMessage -= ComponentDispatcher_ThreadPreprocessMessage;
        }

        private void MyInitialize()
        {
            MyComboBoxKey.ItemsSource = Enum.GetValues(typeof(Key));
            MyComboBoxKey.SelectedIndex = 0;
            MyComboBoxModKey.ItemsSource = Enum.GetValues(typeof(ModifierKeys));
            MyComboBoxModKey.SelectedIndex = 0;
        }

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

       


        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            var mod = (ModifierKeys)MyComboBoxModKey.SelectedValue;
            var key = (Key)MyComboBoxKey.SelectedValue;
            UnregisterHotKey(MyWindowHandle, HOTKEY_ID1);
            if (RegisterHotKey(MyWindowHandle, HOTKEY_ID1, (int)mod, KeyInterop.VirtualKeyFromKey(key)) == 0)
            {
                MessageBox.Show("登録に失敗");
            }
        }

    }
}
