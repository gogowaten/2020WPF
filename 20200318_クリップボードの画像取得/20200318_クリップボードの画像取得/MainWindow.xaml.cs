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

namespace _20200318_クリップボードの画像取得
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Button1.Click += Button1_Click;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = Clipboard.GetImage();
            MyImage.Source = bitmap;

            var format = bitmap.Format;
            //Alphaが全部0なら255にする、
            if (bitmap.Format == PixelFormats.Bgra32)
            {
                int w = bitmap.PixelWidth;
                int h = bitmap.PixelHeight;
                int stride = w * 32 / 8;
                byte[] pixels = new byte[h * stride];
                bitmap.CopyPixels(pixels, stride, 0);
                if (IsAlphaAll0())
                {
                    for (int i = 3; i < pixels.Length; i += 4)
                    {
                        pixels[i] = 255;
                    }
                    var nb = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
                    MyImage.Source = nb;
                }
                //Alphaが全部0ならtrue、1つでも0以外があるならfalse
                bool IsAlphaAll0()
                {
                    for (int i = 3; i < pixels.Length; i += 4)
                    {
                        if (pixels[i] != 0)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }
    }
}

//エクセルからのコピー時のAlphaの値
//セルだけなら全部0になるから、全部255に変換すればいい
//グラフは0になってはいないけど、文字や数値部分のAlphaが0になる
//なので、エクセルからのコピーは問答無用で255へ変換したほうがいい