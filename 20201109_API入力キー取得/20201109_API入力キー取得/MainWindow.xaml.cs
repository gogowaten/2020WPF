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
        public MainWindow()
        {
            InitializeComponent();

            MyTimer = new DispatcherTimer();
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            MyTimer.Start();
        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public extern static short GetAsyncKeyState(int vKey);

        }


    }

    

}
