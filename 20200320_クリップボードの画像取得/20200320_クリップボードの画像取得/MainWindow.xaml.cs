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

namespace _20200320_クリップボードの画像取得
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Gridの背景に市松模様配置
            MyGrid.Background = MakeTileBrush(MakeCheckeredPattern(10, Colors.LightGray));
            ButtonGetBitmap.Click += ButtonGetBitmap_Click;
            ButtonSetBitmap.Click += ButtonSetBitmap_Click;
        }

        private void ButtonSetBitmap_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Clipboard.SetImage((BitmapSource)MyImage.Source);
            
        }

        private void ButtonGetBitmap_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = GetBitmap();
        }

        private BitmapSource GetBitmap()
        {
            BitmapSource bitmap = null;
            if (!Clipboard.ContainsImage()) return bitmap;

            //var meta = Clipboard.GetDataObject().GetData(DataFormats.EnhancedMetafile);//これを画像で取得できればいいけどできない
            //var metapict= Clipboard.GetDataObject().GetData(DataFormats.MetafilePicture);//セルのコピーでエラーになる

            //エクセルのセル以外のコピー
            var stream = (System.IO.MemoryStream)Clipboard.GetDataObject().GetData("PNG");
            //var stream = (System.IO.MemoryStream)Clipboard.GetDataObject().GetData("GIF");
            if (stream != null)
            {
                bitmap = BitmapFrame.Create(stream);                
                //bitmap = ToDpi96(bitmap);//dpiを96にする
                return bitmap;
            }

            bitmap = Clipboard.GetImage();
            //エクセルのセルならBgr32に変換
            if (IsExcelCell())
            {
                bitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgr32, null, 0);
                return bitmap;
            }

            //全ピクセルのAlphaを走査、全部0なら255にする
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w * 32 / 8;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            if (IsAlphaAll0(pixels))
            {
                AlphaToAll255(pixels);
                bitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
                return bitmap;
            }
            return bitmap;
        }

        private BitmapSource ToDpi96(BitmapSource bitmap)
        {
            //dpiを96にする
            int w = bitmap.PixelWidth;
            int h = bitmap.PixelHeight;
            int stride = w * 32 / 8;
            byte[] pixels = new byte[h * stride];
            bitmap.CopyPixels(pixels, stride, 0);
            return BitmapSource.Create(w, h, 96, 96, bitmap.Format, null, pixels, stride);
        }


        //alphaを255にする
        private void AlphaToAll255(byte[] pixels)
        {
            for (int i = 3; i < pixels.Length; i += 4)
            {
                pixels[i] = 255;
            }
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
        private bool IsExcelCell()
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

        #region 市松模様ブラシ作成
        //無限の透明市松模様をWriteableBitmapとImageBrushのタイル表示で作成(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
        //https://blogs.yahoo.co.jp/gogowaten/15917385.html

        /// <summary>
        /// 市松模様画像作成
        /// </summary>
        /// <param name="cellSize">タイル1辺のサイズ</param>
        /// <param name="gray">白じゃない方の色指定</param>
        /// <returns></returns>
        private WriteableBitmap MakeCheckeredPattern(int cellSize, Color gray)
        {
            int width = cellSize * 2;
            int height = cellSize * 2;
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb24, null);
            int stride = wb.Format.BitsPerPixel / 8 * width;
            byte[] pixels = new byte[stride * height];
            int p = 0;
            Color iro;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if ((y < cellSize & x < cellSize) | (y >= cellSize & x >= cellSize))
                    {
                        iro = Colors.White;
                    }
                    else { iro = gray; }

                    p = y * stride + x * 3;
                    pixels[p] = iro.R;
                    pixels[p + 1] = iro.G;
                    pixels[p + 2] = iro.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return wb;
        }

        //        方法: TileBrush のタイル サイズを設定する | Microsoft Docs
        //https://docs.microsoft.com/ja-jp/dotnet/framework/wpf/graphics-multimedia/how-to-set-the-tile-size-for-a-tilebrush
        /// <summary>
        /// BitmapからImageBrush作成
        /// 引き伸ばし無しでタイル状に敷き詰め
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private ImageBrush MakeTileBrush(BitmapSource bitmap)
        {
            var imgBrush = new ImageBrush(bitmap);
            imgBrush.Stretch = Stretch.Uniform;//これは必要ないかも
            //タイルモード、タイル
            imgBrush.TileMode = TileMode.Tile;
            //タイルサイズは元画像のサイズ
            imgBrush.Viewport = new Rect(0, 0, bitmap.Width, bitmap.Height);
            //タイルサイズ指定方法は絶対値、これで引き伸ばされない
            imgBrush.ViewportUnits = BrushMappingMode.Absolute;
            return imgBrush;
        }
        #endregion
    }


}
