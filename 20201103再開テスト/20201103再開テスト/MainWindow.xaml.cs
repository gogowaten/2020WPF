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

namespace _20201103再開テスト
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> MyDirList = new List<string>();
        public MainWindow()
        {
            InitializeComponent();

            MyDirList.Add(@"C:\Users\waten\Documents");
            MyDirList.Add(@"D:\ブログ用");

            ComboBoxSaveDir.ItemsSource = MyDirList;

        }
    }
}
