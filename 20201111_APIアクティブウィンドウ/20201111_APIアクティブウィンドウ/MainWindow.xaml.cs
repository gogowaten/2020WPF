using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;

//GetActiveWindowよりGetForegroundWindowのほうが感覚的に合っている
//GetActiveWindowは、これを呼び出したスレッドが持つウィンドウの中でアクティブなウィンドウが選ばれる、よくわからん
//GetForegroundWindowはOSが管理しているすべてのウィンドウの中からユーザーの操作対象になっているウィンドウが選ばれる

namespace _20201111_APIアクティブウィンドウ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWin, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
      

       


        DispatcherTimer MyTimer;
        public MainWindow()
        {
            InitializeComponent();

            MyTimer = new DispatcherTimer();
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Start();

        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            IntPtr aWin = GetActiveWindow();
            IntPtr fWin = GetForegroundWindow();
            StringBuilder winName = new StringBuilder(65535);
            GetWindowText(fWin, winName, 65535);
            //GetWindowText(aWin, winName, 65535);
            MyTextBlock.Text = winName.ToString();
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
          
        }
    }
}
