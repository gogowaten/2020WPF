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
            if (!Clipboard.ContainsImage())
            {
                TextBlock1.Text = "クリップボードに画像なし";
                TextBlockIsExcel.Text = "";
                TextBlockIsAlphaAll0.Text = "";
                TextBlockBitmapHederBpp.Text = "";
                MyImage.Source = null;
                return;
            }

            var bitmap = Clipboard.GetImage();
            MyImage.Source = bitmap;

            TextBlock1.Text = $"PixelFormats = {bitmap.Format}";


            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w * 32 / 8;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);

            bool isExcel = IsExcel();
            TextBlockIsExcel.Text = $"{nameof(IsExcel)} = {isExcel}";
            bool isAlpha0 = IsAlphaAll0(pixels);
            TextBlockIsAlphaAll0.Text = $"{nameof(IsAlphaAll0)} = {isAlpha0}";

            //Alphaが全部0かエクセルデータなら255にする
            if (isAlpha0 || isExcel)
            {
                for (int i = 3; i < pixels.Length; i += 4)
                {
                    pixels[i] = 255;
                }
                var nb = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
                MyImage.Source = nb;
            }

            //Bitmapの情報ヘッダーのbppを表す値を取得して表示
            var stream = Clipboard.GetDataObject().GetData("DeviceIndependentBitmap") as System.IO.MemoryStream;
            if (stream == null) return;
            byte[] dib = stream.ToArray();
            TextBlockBitmapHederBpp.Text = $"BitmapHederBpp={dib[14]}";



        }
        //Alphaが全部0ならtrue、1つでも0以外があるならfalse
        private bool IsAlphaAll0(byte[] pixels)
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
        //エクセル判定、データの中にEnhancedMetafile形式があればエクセルと判定trueを返す
        private bool IsExcel()
        {
            string[] formats = Clipboard.GetDataObject().GetFormats();
            foreach (var item in formats)
            {
                if (item == "EnhancedMetafile")
                {
                    return true;
                }
            }
            return false;
        }
    }
}

//エクセルからのコピー時のAlphaの値
//セルだけなら全部0になるから、全部255に変換すればいい
//グラフは0になってはいないけど、文字や数値部分のAlphaが0になる
//なので、エクセルからのコピーは問答無用で255へ変換したほうがいい