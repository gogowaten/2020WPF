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
        private AppData MyAppData = new AppData();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            MyTabControl.SelectedIndex = 1;
            //Test1();
            Test2();


        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {            
            
        }

        private void Test2()
        {
            MyAppData.DirList.Add("dir1");
            MyAppData.DirList.Add("dir2");
            DataContext = MyAppData;
            MyAppData.DirList.Add("dir3");

            MyAppData.JpegQuality = 96;
            MyAppData.WindowTop = 500;
            MyAppData.WindowLeft = 500;

            MyAppData.WindowWidth = 450;
            MyAppData.WindowHeight = 350;
            var b = new Binding();
            b.Mode = BindingMode.TwoWay;
            b.Path = new PropertyPath("WindowWidth");
            BindingOperations.SetBinding(this, WidthProperty, b);
            b = new Binding();
            b.Mode = BindingMode.TwoWay;
            b.Path = new PropertyPath("WindowHeight");
            BindingOperations.SetBinding(this, HeightProperty, b);

            //MyAppData.WindowWidth = this.Width;
            //          MyAppData.WindowHeight = 350;
            
        }
        private void Test1()
        {
            MyDirList.Add(@"C:\Users\waten\Documents");
            MyDirList.Add(@"D:\ブログ用");

            //ComboBoxSaveDir.ItemsSource = MyDirList;
            DataContext = MyDirList;
            MyDirList.Add("test");

        }

        private void ButtenTest_Click(object sender, RoutedEventArgs e)
        {
            //MyDirList.Add("add2");
            MyAppData.DirList.Add("dir4");
            
            ComboBoxSaveDir.Items.Refresh();

        }

        private void ButtenTest2_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
        }
        private bool SaveConfig()
        {
            string savePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + "text.xml";
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(AppData));
            try
            {
                using (var stream = new System.IO.StreamWriter(savePath, false, new System.Text.UTF8Encoding(false)))
                {
                    serializer.Serialize(stream, MyAppData);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存できなかった\n{ex.Message}");
                return false;
            }
        }

        private void ButtonStartOrStop_Click(object sender, RoutedEventArgs e)
        {
            ButtonStartOrStop.Content = "停止する";
            RadioInOperation.IsChecked = true;
            RadioInOperation.IsEnabled = true;
            RadioInOperationNot.IsEnabled = false;
        }
    }


    [Serializable]
    public class AppData
    {
        public AppData()
        {
            DirList = new List<string>();
        }
        public List<string> DirList { get; set; }

        public int JpegQuality { get; set; }
        public double WindowTop { get; set; }
        public double WindowLeft { get; set; }
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
    }
}
