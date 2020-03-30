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

namespace _20200330_誤差拡散
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource MyBitmapSource;
        byte[] MyPixels;
        byte[] MyPalette;
        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();

            MyInitialize();


        }
        private void MyInitialize()
        {
            string imagePath;
            imagePath = @"D:\ブログ用\チェック用2\WP_20200328_11_22_52_Pro_2020_03_28_午後わてん.jpg";
            imagePath = @"D:\ブログ用\テスト用画像\grayScale.bmp";
            //imagePath = @"D:\ブログ用\テスト用画像\Michelangelo's_David_-_63_grijswaarden.bmp";
            imagePath = @"D:\ブログ用\テスト用画像\gray128.png";//0と255の中間みたい、pixelformats.blackwhiteだと市松模様になる
            imagePath = @"D:\ブログ用\テスト用画像\gray127.png";//これは中間じゃないみたい
            imagePath = @"D:\ブログ用\テスト用画像\gray250.png";
            //imagePath = @"D:\ブログ用\テスト用画像\ﾈｺは見ている.png";
            imagePath = @"D:\ブログ用\テスト用画像\NEC_2097_.jpg";
            imagePath = @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん_256x192.png";

            (MyPixels, MyBitmapSource) = MakeBitmapSourceAndPixelData(imagePath, PixelFormats.Gray8, 96, 96);
            MyImage.Source = MyBitmapSource;

            MyPalette = new byte[] { 32, 96, 160, 224 };
            //MyPalette = new byte[] { 0, 128, 255 };
            //MyPalette = new byte[] { 64,192 };
            //MyPalette = new byte[] { 0, 255 };

        }


        private BitmapSource GosaKakusan(byte[] palette, byte[] pixels, int width, int height, int stride)
        {
            byte[] replacePixels = new byte[pixels.Length];
            double[] gosaPixels = new double[pixels.Length];
            Array.Copy(pixels, gosaPixels, pixels.Length);
            Array.Copy(pixels, replacePixels, pixels.Length);
            double gosa;
            double value;
            int ww = 0;//右端判定用
            int bottom = stride * (height - 1);
            for (int i = 0; i < pixels.Length; i++)
            {
                value = gosaPixels[i];
                double distance = Math.Abs(value - palette[0]);
                byte color = MyPalette[0];
                for (int k = 1; k < palette.Length; k++)
                {
                    double temp = Math.Abs(value - palette[k]);
                    if (distance > temp)
                    {
                        distance = temp;
                        color = palette[k];
                    }
                }
                //誤差記録
                gosa = (gosaPixels[i] - color) / 16;

                //右端判定して誤差拡散
                if (ww != stride - 1) { gosaPixels[i + 1] += gosa * 7; }

                if (i < bottom)
                {
                    gosaPixels[i + width] += gosa * 5;//真下
                    if (ww != 0)
                        gosaPixels[i + width - 1] += gosa * 3;//左下
                    if (ww != stride - 1)
                        gosaPixels[i + width + 1] += gosa * 1;//右下                        
                }

                //右端判定用
                if (ww != stride - 1) { ww++; }
                else { ww = 0; }

                //色の置き換え
                replacePixels[i] = color;
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, replacePixels, width);
        }

        //誤差を0～255の範囲に収めて計算、多分これが一番いいと思います
        private BitmapSource GosaKakusan2(byte[] palette, byte[] pixels, int width, int height, int stride)
        {
            byte[] replacePixels = new byte[pixels.Length];
            double[] gosaPixels = new double[pixels.Length];
            Array.Copy(pixels, gosaPixels, pixels.Length);
            Array.Copy(pixels, replacePixels, pixels.Length);
            double gosa;
            double value;
            int p;
            int ww = 0;//右端判定用
            int bottom = stride * (height - 1);
            for (int i = 0; i < pixels.Length; i++)
            {
                value = gosaPixels[i];
                double distance = Math.Abs(value - palette[0]);
                byte color = MyPalette[0];
                for (int k = 1; k < palette.Length; k++)
                {
                    double temp = Math.Abs(value - palette[k]);
                    if (distance > temp)
                    {
                        distance = temp;
                        color = palette[k];
                    }
                }
                //誤差記録
                gosa = (gosaPixels[i] - color) / 16;
                //右端判定して誤差拡散
                p = i + 1;
                if (ww != stride - 1)
                {
                    gosaPixels[p] = LimitedGosa(gosaPixels[p], gosa * 7);
                }

                if (i < bottom)
                {
                    p += stride;
                    gosaPixels[p - 1] = LimitedGosa(gosaPixels[p - 1], gosa * 5);

                    if (ww != 0)
                        gosaPixels[p - 2] = LimitedGosa(gosaPixels[p - 2], gosa * 3);//左下

                    if (ww != stride - 1)
                        gosaPixels[p] = LimitedGosa(gosaPixels[p], gosa * 1);
                }

                //右端判定用
                if (ww != stride - 1) { ww++; }
                else { ww = 0; }

                //色の置き換え
                replacePixels[i] = color;
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, replacePixels, width);
        }


        private double LimitedGosa(double value, double gosa)
        {
            double result = value + gosa;
            if (result > 255)
                result = 255;
            else if (result < 0)
                result = 0;
            return result;
        }

        //誤差を足した結果を四捨五入
        private BitmapSource GosaKakusan3(byte[] palette, byte[] pixels, int width, int height, int stride)
        {
            byte[] replacePixels = new byte[pixels.Length];
            var gosaPixels = new byte[pixels.Length];
            Array.Copy(pixels, gosaPixels, pixels.Length);
            Array.Copy(pixels, replacePixels, pixels.Length);
            double gosa;
            byte value;
            int p;
            int ww = 0;//右端判定用
            int bottom = stride * (height - 1);
            for (int i = 0; i < pixels.Length; i++)
            {
                value = gosaPixels[i];
                var distance = Math.Abs(value - palette[0]);
                byte color = MyPalette[0];
                for (int k = 1; k < palette.Length; k++)
                {
                    var temp = Math.Abs(value - palette[k]);
                    if (distance > temp)
                    {
                        distance = temp;
                        color = palette[k];
                    }
                }
                //誤差記録
                gosa = (gosaPixels[i] - color) / 16;
                //右端判定して誤差拡散
                p = i + 1;
                if (ww != stride - 1)
                {
                    SetLimitedGosaRound(gosaPixels, p, gosa * 7);
                }

                if (i < bottom)
                {
                    p += stride;
                    SetLimitedGosaRound(gosaPixels, p - 1, gosa * 5);
                    if (ww != 0)
                        SetLimitedGosaRound(gosaPixels, p - 2, gosa * 3);//左下

                    if (ww != stride - 1)
                        SetLimitedGosaRound(gosaPixels, p, gosa * 1);
                }

                //右端判定用
                if (ww != stride - 1) { ww++; }
                else { ww = 0; }

                //色の置き換え
                replacePixels[i] = color;
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, replacePixels, width);
        }

        private void SetLimitedGosaRound(byte[] pixels, int p, double gosa)
        {
            double result = pixels[p] + gosa;
            if (result > 255)
                result = 255;
            else if (result < 0)
                result = 0;
            pixels[p] = (byte)Math.Round(result, MidpointRounding.AwayFromZero);
        }

        //誤差を足した結果を小数点以下切り捨て、これは四捨五入と同じ結果になる
        private BitmapSource GosaKakusan4(byte[] palette, byte[] pixels, int width, int height, int stride)
        {
            byte[] replacePixels = new byte[pixels.Length];
            var gosaPixels = new byte[pixels.Length];
            Array.Copy(pixels, gosaPixels, pixels.Length);
            Array.Copy(pixels, replacePixels, pixels.Length);
            double gosa;
            byte value;
            int p;
            int ww = 0;//右端判定用
            int bottom = stride * (height - 1);
            for (int i = 0; i < pixels.Length; i++)
            {
                value = gosaPixels[i];
                var distance = Math.Abs(value - palette[0]);
                byte color = MyPalette[0];
                for (int k = 1; k < palette.Length; k++)
                {
                    var temp = Math.Abs(value - palette[k]);
                    if (distance > temp)
                    {
                        distance = temp;
                        color = palette[k];
                    }
                }
                //誤差記録
                gosa = (gosaPixels[i] - color) / 16;
                //右端判定して誤差拡散
                p = i + 1;
                if (ww != stride - 1)
                {
                    SetLimitedGosaRoundCut(gosaPixels, p, gosa * 7);
                }

                if (i < bottom)
                {
                    p += stride;
                    SetLimitedGosaRoundCut(gosaPixels, p - 1, gosa * 5);
                    if (ww != 0)
                        SetLimitedGosaRoundCut(gosaPixels, p - 2, gosa * 3);//左下

                    if (ww != stride - 1)
                        SetLimitedGosaRoundCut(gosaPixels, p, gosa * 1);
                }

                //右端判定用
                if (ww != stride - 1) { ww++; }
                else { ww = 0; }

                //色の置き換え
                replacePixels[i] = color;
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, replacePixels, width);
        }

        private void SetLimitedGosaRoundCut(byte[] pixels, int p, double gosa)
        {
            double result = pixels[p] + gosa;
            if (result > 255)
                result = 255;
            else if (result < 0)
                result = 0;
            pixels[p] = (byte)result;
        }

        //蛇行＋誤差蓄積、これは画像が崩れる
        private BitmapSource ReplaceColor蛇行(byte[] palette, byte[] pixels, int width, int height, int stride)
        {
            byte[] replacePixels = new byte[pixels.Length];
            double[] gosaPixels = new double[pixels.Length];
            Array.Copy(pixels, gosaPixels, pixels.Length);
            Array.Copy(pixels, replacePixels, pixels.Length);
            double gosa;
            int p;
            int pp = width;//逆走査用
            int ww = 0;//右端判定用
            int rightEndIndex = stride - 1;//右端判定用
            int bottom = stride * (height - 1);//画像右下の一個上座標
            bool isGoRight = true;//走査方向
            for (int i = 0; i < pixels.Length; i++)
            {

                if (isGoRight == true)
                //走査方向→の時
                {
                    //パレットの中から一番近い色を取得
                    byte color = GetNearColor(gosaPixels[i], palette);
                    //色の置き換え
                    replacePixels[i] = color;

                    //ここから誤差拡散
                    //誤差記録
                    gosa = (gosaPixels[i] - color) / 16;
                    //右端判定して誤差拡散
                    if (ww != rightEndIndex)
                        gosaPixels[i + 1] += gosa * 7;//右隣

                    if (i < bottom)
                    {
                        p = i + stride;//一段下
                        gosaPixels[p] += gosa * 5;//真下
                        if (ww != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (ww != rightEndIndex)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                    if (ww == rightEndIndex) isGoRight = false;
                }
                //走査方向←の時
                else
                {
                    p = i + pp - 1;//位置調整
                    byte color = GetNearColor(gosaPixels[p], palette);
                    replacePixels[p] = color;
                    gosa = (gosaPixels[p] - color) / 16.0;

                    p += -1;//左隣
                    if (ww != 0) gosaPixels[p] += gosa * 7;
                    if (p + 1 < bottom)
                    {
                        p += +stride + 1;//一段下
                        gosaPixels[p] += gosa * 5;//真下
                        if (ww != 0) gosaPixels[p + 1] += gosa * 3;//右下
                        if (ww != rightEndIndex) gosaPixels[p - 1] += gosa * 1;//左下
                    }
                    pp -= 2;
                    //左端に到達したら切り替え
                    if (pp == -stride)
                    {
                        pp = width;
                        isGoRight = true;
                    }
                }
                //右端判定用、右端に到達するまで＋＋、到達したら0に戻す
                if (ww != rightEndIndex)
                    ww++;
                else ww = 0;
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, replacePixels, width);
        }
        //パレットの中から一番近い色を返す
        private byte GetNearColor(double value, byte[] palette)
        {
            double distance = 255;//初期値は最大距離
            byte color = MyPalette[0];
            for (int k = 0; k < palette.Length; k++)
            {
                double temp = Math.Abs(value - palette[k]);
                if (distance > temp)
                {
                    distance = temp;
                    color = palette[k];
                }
            }
            return color;
        }

        //誤差値を0～255の範囲に収めて計算
        private BitmapSource ReplaceColor蛇行2(byte[] palette, byte[] pixels, int width, int height, int stride)
        {
            byte[] replacePixels = new byte[pixels.Length];
            double[] gosaPixels = new double[pixels.Length];
            Array.Copy(pixels, gosaPixels, pixels.Length);
            Array.Copy(pixels, replacePixels, pixels.Length);
            double gosa;
            int p;
            int pp = width;//逆走査用
            int ww = 0;//右端判定用
            int rightEndIndex = stride - 1;//右端判定用
            int bottom = stride * (height - 1);//画像右下の一個上座標
            bool isGoRight = true;//走査方向
            for (int i = 0; i < pixels.Length; i++)
            {

                if (isGoRight == true)
                //走査方向→の時
                {
                    //パレットの中から一番近い色を取得
                    byte color = GetNearColor(gosaPixels[i], palette);
                    //色の置き換え
                    replacePixels[i] = color;

                    //ここから誤差拡散
                    //誤差記録
                    gosa = (gosaPixels[i] - color) / 16;
                    //右端判定して誤差拡散
                    if (ww != rightEndIndex)
                        SetLimitedGosa(gosaPixels, i + 1, gosa * 7);//右隣
                    if (i < bottom)
                    {
                        p = i + stride;//一段下
                        SetLimitedGosa(gosaPixels, p, gosa * 5);//真下
                        if (ww != 0)
                            SetLimitedGosa(gosaPixels, p - 1, gosa * 3);//左下
                        if (ww != rightEndIndex)
                            SetLimitedGosa(gosaPixels, p + 1, gosa * 1);//右下
                    }
                    if (ww == rightEndIndex) isGoRight = false;
                }
                //走査方向←の時
                else
                {
                    p = i + pp - 1;//位置調整
                    byte color = GetNearColor(gosaPixels[p], palette);
                    replacePixels[p] = color;
                    gosa = (gosaPixels[p] - color) / 16.0;

                    p += -1;//左隣
                    if (ww != 0) SetLimitedGosa(gosaPixels, p, gosa * 7);
                    if (p + 1 < bottom)
                    {
                        p += +stride + 1;//一段下
                        SetLimitedGosa(gosaPixels, p, gosa * 5);//真下
                        if (ww != 0) SetLimitedGosa(gosaPixels, p + 1, gosa * 3);//右下
                        if (ww != rightEndIndex) SetLimitedGosa(gosaPixels, p - 1, gosa * 1);//左下
                    }
                    pp -= 2;
                    //左端に到達したら切り替え
                    if (pp == -stride)
                    {
                        pp = width;
                        isGoRight = true;
                    }
                }
                //右端判定用、右端に到達するまで＋＋、到達したら0に戻す
                if (ww != rightEndIndex)
                    ww++;
                else ww = 0;
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, replacePixels, width);
        }
        private void SetLimitedGosa(double[] pixels, int p, double gosa)
        {
            double result = pixels[p] + gosa;
            if (result > 255)
                result = 255;
            else if (result < 0)
                result = 0;
            pixels[p] = result;
        }

        //誤差値をパレットの最小値、最大値の範囲に収めて計算
        private BitmapSource ReplaceColor蛇行3(byte[] palette, byte[] pixels, int width, int height, int stride)
        {
            byte[] replacePixels = new byte[pixels.Length];
            double[] gosaPixels = new double[pixels.Length];
            Array.Copy(pixels, gosaPixels, pixels.Length);
            Array.Copy(pixels, replacePixels, pixels.Length);
            double gosa;
            int p;
            int pp = width;//逆走査用
            int ww = 0;//右端判定用
            int rightEndIndex = stride - 1;//右端判定用
            int bottom = stride * (height - 1);//画像右下の一個上座標
            bool isGoRight = true;//走査方向
            var (min, max) = GetPaletteMinMax(palette);
            for (int i = 0; i < pixels.Length; i++)
            {

                if (isGoRight == true)
                //走査方向→の時
                {
                    //パレットの中から一番近い色を取得
                    byte color = GetNearColor(gosaPixels[i], palette);
                    //色の置き換え
                    replacePixels[i] = color;

                    //ここから誤差拡散
                    //誤差記録
                    gosa = (gosaPixels[i] - color) / 16;
                    //右端判定して誤差拡散
                    if (ww != rightEndIndex)
                        SetLimitedPaletteGosa(gosaPixels, i + 1, gosa * 7, min, max);//右隣
                    if (i < bottom)
                    {
                        p = i + stride;//一段下
                        SetLimitedPaletteGosa(gosaPixels, p, gosa * 5, min, max);//真下
                        if (ww != 0)
                            SetLimitedPaletteGosa(gosaPixels, p - 1, gosa * 3, min, max);//左下
                        if (ww != rightEndIndex)
                            SetLimitedPaletteGosa(gosaPixels, p + 1, gosa * 1, min, max);//右下
                    }
                    if (ww == rightEndIndex) isGoRight = false;
                }
                //走査方向←の時
                else
                {
                    p = i + pp - 1;//位置調整
                    byte color = GetNearColor(gosaPixels[p], palette);
                    replacePixels[p] = color;
                    gosa = (gosaPixels[p] - color) / 16.0;

                    p += -1;//左隣
                    if (ww != 0) SetLimitedPaletteGosa(gosaPixels, p, gosa * 7, min, max);
                    if (p + 1 < bottom)
                    {
                        p += +stride + 1;//一段下
                        SetLimitedPaletteGosa(gosaPixels, p, gosa * 5, min, max);//真下
                        if (ww != 0) SetLimitedPaletteGosa(gosaPixels, p + 1, gosa * 3, min, max);//右下
                        if (ww != rightEndIndex) SetLimitedPaletteGosa(gosaPixels, p - 1, gosa * 1, min, max);//左下
                    }
                    pp -= 2;
                    //左端に到達したら切り替え
                    if (pp == -stride)
                    {
                        pp = width;
                        isGoRight = true;
                    }
                }
                //右端判定用、右端に到達するまで＋＋、到達したら0に戻す
                if (ww != rightEndIndex)
                    ww++;
                else ww = 0;
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, replacePixels, width);
        }
        private void SetLimitedPaletteGosa(double[] pixels, int p, double gosa, byte min, byte max)
        {
            double result = pixels[p] + gosa;
            if (result > max)
                result = max;
            else if (result < min)
                result = min;
            pixels[p] = result;
        }
        //パレットの最小値と最大値を返す
        private (byte min, byte max) GetPaletteMinMax(byte[] palette)
        {
            byte min = 255; byte max = 0;
            foreach (var item in palette)
            {
                if (min > item) min = item;
                if (max < item) max = item;
            }
            return (min, max);
        }



        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Clipboard.SetImage((BitmapSource)MyImage.Source);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = GosaKakusan(MyPalette, MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        #region 画像読み込み
        /// <summary>
        /// 画像ファイルからbitmapと、そのbyte配列を返す、ピクセルフォーマットは指定したものに変換
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsを指定、null指定ならBgra32で作成する</param>
        /// <param name="dpiX">96が基本、指定なしなら元画像と同じにする</param>
        /// <param name="dpiY">96が基本、指定なしなら元画像と同じにする</param>
        /// <returns></returns>
        private (byte[] pixels, BitmapSource source) MakeBitmapSourceAndPixelData(
            string filePath,
            PixelFormat pixelFormat,
            double dpiX = 0, double dpiY = 0)
        {
            byte[] pixels = null;//PixelData
            BitmapSource source = null;
            if (pixelFormat == null) { pixelFormat = PixelFormats.Bgra32; }
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var frame = BitmapFrame.Create(fs);
                    var tempBitmap = new FormatConvertedBitmap(frame, pixelFormat, null, 0);
                    int w = tempBitmap.PixelWidth;
                    int h = tempBitmap.PixelHeight;
                    int stride = (w * pixelFormat.BitsPerPixel + 7) / 8;
                    pixels = new byte[h * stride];
                    tempBitmap.CopyPixels(pixels, stride, 0);
                    //dpi指定がなければ元の画像と同じdpiにする
                    if (dpiX == 0) { dpiX = frame.DpiX; }
                    if (dpiY == 0) { dpiY = frame.DpiY; }
                    //dpiを指定してBitmapSource作成
                    source = BitmapSource.Create(
                        w, h, dpiX, dpiY,
                        tempBitmap.Format,
                        tempBitmap.Palette, pixels, stride);
                };
            }
            catch (Exception)
            {
            }
            return (pixels, source);
        }

        #endregion

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = GosaKakusan2(MyPalette, MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = GosaKakusan3(MyPalette, MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }


        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            //MyImage.Source = new FormatConvertedBitmap((BitmapSource)MyImage.Source, PixelFormats.BlackWhite, null, 0);
            MyImage.Source = GosaKakusan4(MyPalette, MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = ReplaceColor蛇行(MyPalette, MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = ReplaceColor蛇行2(MyPalette, MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = ReplaceColor蛇行3(MyPalette, MyPixels, MyBitmapSource.PixelWidth, MyBitmapSource.PixelHeight, MyBitmapSource.PixelWidth);
        }
    }
}
