using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace 画像の埋め込みと取得
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            string path = "画像の埋め込みと取得.grayScale.bmp";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(path))
            {
                if (stream != null)
                {
                    MyImage.Source = BitmapFrame.Create(stream);
                }
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            string path = "画像の埋め込みと取得.Myフォルダー.HSVRectHue000.png";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(path))
            {
                if (stream != null)
                {
                    MyImage.Source = BitmapFrame.Create(stream);
                }
            }
        }

        //未使用
        private void Button3_Click(object sender, RoutedEventArgs e)
        {  
            var bmp = Properties.Resources.HSVRectH90;
            MyImage.Source = BitmapSource.Create(
                256, 256, 96, 96, PixelFormats.Bgra32, null, bmp, 256 * 4);
        }
    }
}
