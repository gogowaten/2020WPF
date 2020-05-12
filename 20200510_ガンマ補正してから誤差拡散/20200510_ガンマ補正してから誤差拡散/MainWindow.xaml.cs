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

//Libcacaスタディ-5.グレイスケールディザリング
//http://caca.zoy.org/study/part5.html

namespace _20200510_ガンマ補正してから誤差拡散
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource MyBitmapSource;


        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
            MyGrid.MouseLeftButtonDown += (s, e) => Panel.SetZIndex(MyImageOrigin, 1);
            MyGrid.MouseLeftButtonUp += (s, e) => Panel.SetZIndex(MyImageOrigin, -1);





            MyInitialize();

            //            デバッグビルドでのみ特定のコードがコンパイルされるようにする - .NET Tips(VB.NET, C#...)
            //https://dobon.net/vb/dotnet/programing/define.html

#if DEBUG
            MyInitializeForDebug();
#endif

        }

        //画像ファイルがドロップされた時
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            //Gray8に変換して読み込む
            (_, MyBitmapSource) = MakeBitmapSourceAndPixelData(filePath[0], PixelFormats.Gray8, 96, 96);
            if (MyBitmapSource == null)
            {
                MessageBox.Show("not Image");
            }
            else
            {
                MyImage.Source = MyBitmapSource;
                MyImageOrigin.Source = MyBitmapSource;
            }
        }

        private void MyInitialize()
        {
            //アプリに埋め込んだ画像を読み込んで、Gray8に変換して読み込む
            string path = "_20200507_ガンマ補正してから3色誤差拡散.grayScale.bmp";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(path))
            {
                if (stream != null)
                {
                    var source = BitmapFrame.Create(stream);
                    MyBitmapSource = new FormatConvertedBitmap(source, PixelFormats.Gray8, null, 0);
                    MyImage.Source = source;
                    MyImageOrigin.Source = source;
                }
            }

        }
        private void MyInitializeForDebug()
        {

            string imagePath;
            imagePath = @"D:\ブログ用\チェック用2\WP_20200328_11_22_52_Pro_2020_03_28_午後わてん.jpg";
            //imagePath = @"E:\オレ\携帯\2019スマホ\WP_20200328_11_22_52_Pro.jpg";
            imagePath = @"D:\ブログ用\テスト用画像\grayScale.bmp";
            //imagePath = @"D:\ブログ用\テスト用画像\grayscale256x256.png";
            //imagePath = @"D:\ブログ用\テスト用画像\Michelangelo's_David_-_63_grijswaarden.bmp";
            //imagePath = @"D:\ブログ用\テスト用画像\lena256bw.png";
            //imagePath = @"D:\ブログ用\テスト用画像\gray128.png";//0と255の中間みたい、pixelformats.blackwhiteだと市松模様になる
            //imagePath = @"D:\ブログ用\テスト用画像\gray127.png";//これは中間じゃないみたい
            //imagePath = @"D:\ブログ用\テスト用画像\gray250.png";
            //imagePath = @"D:\ブログ用\テスト用画像\ﾈｺは見ている.png";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_2097_.jpg";
            //imagePath = @"D:\ブログ用\テスト用画像\NEC_1456_2018_03_17_午後わてん_256x192.png";


            (_, MyBitmapSource) = MakeBitmapSourceAndPixelData(imagePath, PixelFormats.Gray8, 96, 96);
            MyImage.Source = MyBitmapSource;
            MyImageOrigin.Source = MyBitmapSource;

        }


        private Dictionary<int, byte> MakeTable(int colorsCount)
        {
            var table = new Dictionary<int, byte>();
            double stepColor = 1.0 / (colorsCount - 1);

            for (int i = 0; i < colorsCount; i++)
            {
                table.Add(i, (byte)((stepColor * i * 255) + 0.5));//四捨五入
                //table.Add(i, (byte)Math.Round(stepColor * i * 255, MidpointRounding.AwayFromZero));
            }
            table.Add(colorsCount, 255);
            return table;
        }

        private BitmapSource D1(BitmapSource source, int colorsCount)
        {
            var convertTable = MakeTable(colorsCount);
            double stepRange = 1.0 / colorsCount;//0.333

            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用
            double[] gosaPixels = new double[height * stride];
            Array.Copy(pixels, gosaPixels, height * stride);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    //変換
                    double neko = gosaPixels[p] / (255 * stepRange);
                    //int inu = (int)neko;
                    int inu = (int)(neko + 0.5);//四捨五入
                    pixels[p] = convertTable[inu];

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        private double[] MakeLocalGamma(int colorsCount, double gamma)
        {
            //int count = colorsCount;
            int count = colorsCount - 1;
            double part = 1.0 / count;
            double[] vs = new double[colorsCount];
            //double[] vs = new double[count];
            vs[0] = gamma;

            double begin;//開始値
            double gCorrect;//補正後γ
            double gLength = gamma - 1.0;//γ全体距離
            for (int i = 1; i < count + 1; i++)
            {
                begin = part * i;
                gCorrect = Math.Pow(begin, 1.0 / gamma);
                var gNew = (-gLength * gCorrect) + gamma;
                vs[i] = gNew;
            }
            return vs;
        }
        private double[] GammaCorrect1(byte[] pixels, int colorsCount, double gamma)
        {
            double[] localGamma = MakeLocalGamma(colorsCount, gamma);
            double[] correctPixels = new double[pixels.Length];
            double step = 1.0 / (colorsCount - 1);
            int index;
            for (int i = 0; i < pixels.Length; i++)
            {
                double v = pixels[i] / 255.0;
                index = (int)(v / step);
                double begin = step * index;
                double vCorrect = (v - begin) * (colorsCount - 1);
                vCorrect = Math.Pow(vCorrect, localGamma[index]);
                if (index == 1) { var inu = 0; }
                vCorrect /= colorsCount - 1;
                vCorrect += begin;
                vCorrect *= 255.0;
                correctPixels[i] = vCorrect;
            }
            return correctPixels;
        }
        //変換前に一括でガンマ補正、可変ガンマ値
        private BitmapSource D2(BitmapSource source, int colorsCount)
        {
            Dictionary<int, byte> convertTable = MakeTable(colorsCount);
            double stepTone = 1.0 / colorsCount * 255;//85

            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            double gamma = 2.2;
            var gosaPixels = GammaCorrect1(pixels, colorsCount, gamma);


            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    //変換
                    double neko = gosaPixels[p] / stepTone;
                    //int inu = (int)neko;
                    int inu = (int)(neko + 0.5);//四捨五入
                    pixels[p] = convertTable[inu];

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }



        private double[] MakeLocalGamma2(int colorsCount, double gamma)
        {
            int count = colorsCount - 1;
            double step = 1.0 / count;
            double[] vs = new double[colorsCount];
            vs[0] = gamma;

            double begin;//開始値
            double gCorrect;//補正後γ
            double gLength = gamma - 1.0;//γ全体距離
            int index = (int)(1.0 / (colorsCount - 1));
            begin = step * index;
            gCorrect = Math.Pow(begin, 1.0 / gamma);
            double lastGamma = (-1.2 * gCorrect) + gamma;

            for (int i = 0; i < count; i++)
            {

                begin = step * i;
                gCorrect = Math.Pow(begin, 1.0 / gamma);
                var gNew = (-gLength * gCorrect) + gamma;
                vs[i] = gNew;
            }
            return vs;
        }
        private double[] GammaCorrect2(byte[] pixels, int colorsCount, double gamma)
        {
            double[] correctPixels = new double[pixels.Length];
            double step = 1.0 / (colorsCount - 1);

            var splitCount = colorsCount - 1;
            double kaisiti = 1.0 - (1.0 / splitCount);
            double hoseigo = Math.Pow(kaisiti, 1.0 / 2.2);

            double ll = (-1.2 * hoseigo) + 2.2;


            for (int i = 0; i < pixels.Length; i++)
            {
                double v = pixels[i] / 255.0;
                int index = (int)(v / step);
                double begin = step * index;
                double vCorrect = (v - begin) * (colorsCount - 1);
                vCorrect = Math.Pow(vCorrect, ll);//ガンマ補正
                vCorrect /= colorsCount - 1;
                vCorrect += begin;
                vCorrect *= 255.0;
                correctPixels[i] = vCorrect;
            }
            return correctPixels;
        }
        //変換前に一括でガンマ補正、固定ガンマ値(最終ガンマ値)
        private BitmapSource D3(BitmapSource source, int colorsCount)
        {
            Dictionary<int, byte> convertTable = MakeTable(colorsCount);
            double stepRange = 1.0 / colorsCount;//0.333

            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用
            double gamma = 2.2;
            var gosaPixels = GammaCorrect2(pixels, colorsCount, gamma);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    //変換
                    double neko = gosaPixels[p] / (255 * stepRange);
                    //int inu = (int)neko;
                    int inu = (int)(neko + 0.5);//四捨五入
                    pixels[p] = convertTable[inu];

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        //変換直前に個別にガンマ補正、可変ガンマ値
        private BitmapSource D4(BitmapSource source, int colorsCount)
        {
            Dictionary<int, byte> convertTable = MakeTable(colorsCount);
            double stepRange = 1.0 / colorsCount;//0.333

            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用
            double gamma = 2.2;
            var gosaPixels = GammaCorrect2(pixels, colorsCount, gamma);
            var localGamma = MakeLocalGamma(colorsCount, gamma);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)
            int splitCount = colorsCount - 1;//分割数
            double stepTone = 1.0 / splitCount;//1階調
                                               //(128*x/65536/255)^(1/2.2)=(100/255)^(1/2.2)
                                               //(128*x/65536/255)=(100/255)
                                               //128*x/65536=100
                                               //128*x=100*65536
                                               //x=100*65536/128

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    double gp = gosaPixels[p] / 255.0;
                    if (gp < 0) gp = 0.0;//マイナスになる場合もある、そのときは0に強制変換
                    if (gp > 1) gp = 1.0;
                    int index = (int)(gp / stepTone);
                    double begin = stepTone * index;
                    double gg = (gp - begin) * splitCount;
                    gg = Math.Pow(gg, localGamma[index]);//ガンマ補正
                    gg /= splitCount;
                    gg += begin;//補正後出力値 = 開始値+((入力値-開始値)*分割数)^(γ値)/分割数

                    //変換
                    double neko = gg / stepRange;//0.333
                    //int inu = (int)(neko + 0.5);//四捨五入
                    int inu = (int)neko;//切り捨て
                    if (inu == 2) { var uu = 0; }
                    pixels[p] = convertTable[inu];

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        private double[] MakeLocalGamma5(int colorsCount, double gamma)
        {
            int count = colorsCount - 1;
            double step = 1.0 / count;

            double[] vs = new double[colorsCount];
            for (int i = 0; i < count; i++)
            {
                var dd = Math.Pow(step * (i + 1), 1 / gamma) - Math.Pow(step * i, 1 / gamma);
                vs[i] = dd * 1.2 + 1;
            }
            vs[count] = 1.0;
            return vs;
        }
        private double[] GammaCorrect5(byte[] pixels, int colorsCount, double gamma)
        {
            double[] localGamma = MakeLocalGamma5(colorsCount, gamma);
            double[] correctPixels = new double[pixels.Length];
            double step = 1.0 / (colorsCount - 1);
            int index;
            for (int i = 0; i < pixels.Length; i++)
            {
                double v = pixels[i] / 255.0;
                index = (int)(v / step);
                double begin = step * index;
                double vCorrect = (v - begin) * (colorsCount - 1);
                vCorrect = Math.Pow(vCorrect, localGamma[index]);
                vCorrect /= colorsCount - 1;
                vCorrect += begin;
                vCorrect *= 255.0;
                correctPixels[i] = vCorrect;
            }
            return correctPixels;
        }
        //変換前に一括でガンマ補正、可変ガンマ値、D3とは計算が違う
        private BitmapSource D5(BitmapSource source, int colorsCount)
        {
            Dictionary<int, byte> convertTable = MakeTable(colorsCount);
            double stepRange = 1.0 / colorsCount;//0.333

            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用
            double gamma = 2.2;
            var gosaPixels = GammaCorrect5(pixels, colorsCount, gamma);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    //変換
                    double neko = gosaPixels[p] / (255 * stepRange);
                    //int inu = (int)neko;
                    int inu = (int)(neko + 0.5);//四捨五入
                    pixels[p] = convertTable[inu];

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        //変換直前に個別にガンマ補正、固定ガンマ値2.2
        private BitmapSource D6(BitmapSource source, int colorsCount)
        {
            Dictionary<int, byte> convertTable = MakeTable(colorsCount);
            double stepRange = 1.0 / colorsCount;//0.333

            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用
            double gamma = 2.2;
            var gosaPixels = GammaCorrect2(pixels, colorsCount, gamma);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)
            int splitCount = colorsCount - 1;//分割数
            double stepTone = 1.0 / splitCount;//1階調
                                               //(128*x/65536/255)^(1/2.2)=(100/255)^(1/2.2)
                                               //(128*x/65536/255)=(100/255)
                                               //128*x/65536=100
                                               //128*x=100*65536
                                               //x=100*65536/128

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    double gp = gosaPixels[p] / 255.0;
                    if (gp < 0) gp = 0.0;//マイナスになる場合もある、そのときは0に強制変換
                    if (gp > 1) gp = 1.0;
                    int index = (int)(gp / stepTone);
                    double begin = stepTone * index;
                    double gg = (gp - begin) * splitCount;
                    gg = Math.Pow(gg, gamma);//ガンマ補正2.2
                    gg /= splitCount;
                    gg += begin;//補正後出力値 = 開始値+((入力値-開始値)*分割数)^(γ値)/分割数

                    //変換
                    double neko = gg / stepRange;//0.333
                    //int inu = (int)(neko + 0.5);//四捨五入
                    int inu = (int)neko;//切り捨て
                    if (inu == 2) { var uu = 0; }
                    pixels[p] = convertTable[inu];

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        //変換直前に個別にガンマ補正、可変ガンマ値
        private BitmapSource D7(BitmapSource source, int colorsCount)
        {
            Dictionary<int, byte> convertTable = MakeTable(colorsCount);
            double stepRange = 1.0 / colorsCount;//0.333

            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用
            double gamma = 2.2;
            var gosaPixels = GammaCorrect2(pixels, colorsCount, gamma);
            var localGamma = MakeLocalGamma5(colorsCount, gamma);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)
            int splitCount = colorsCount - 1;//分割数
            double stepTone = 1.0 / splitCount;//1階調
                                               //(128*x/65536/255)^(1/2.2)=(100/255)^(1/2.2)
                                               //(128*x/65536/255)=(100/255)
                                               //128*x/65536=100
                                               //128*x=100*65536
                                               //x=100*65536/128

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;
                    double gp = gosaPixels[p] / 255.0;
                    if (gp < 0) gp = 0.0;//マイナスになる場合もある、そのときは0に強制変換
                    if (gp > 1) gp = 1.0;
                    int index = (int)(gp / stepTone);
                    double begin = stepTone * index;
                    double gg = (gp - begin) * splitCount;
                    gg = Math.Pow(gg, localGamma[index]);//ガンマ補正
                    gg /= splitCount;
                    gg += begin;//補正後出力値 = 開始値+((入力値-開始値)*分割数)^(γ値)/分割数

                    //変換
                    double neko = gg / stepRange;//0.333
                    //int inu = (int)(neko + 0.5);//四捨五入
                    int inu = (int)neko;//切り捨て
                    if (inu == 2) { var uu = 0; }
                    pixels[p] = convertTable[inu];

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //白黒2値化、0～1に変換してから計算、事前補正
        private BitmapSource D8(BitmapSource source)
        {
            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用
            double gamma = 2.2;
            double[] gps = pixels.Select(x => x / 255.0).ToArray();//
            double[] gosaPixels = gps.Select(x => Math.Pow(x, gamma)).ToArray();

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;

                    //変換、0.5未満＝0、0.5以上＝1
                    if (gosaPixels[p] < 0.5) gps[p] = 0;
                    else gps[p] = 1;

                    //誤差拡散
                    gosa = (gosaPixels[p] - gps[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            pixels = gps.Select(x => (byte)(x * 255)).ToArray();
            //pixels = gps.Select(x => (byte)((x * 255) + 0.5)).ToArray();//四捨五入？しても変化なし
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //0～1に変換してから計算
        private BitmapSource D9(BitmapSource source, int colorsCount)
        {
            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //誤差拡散計算用
            double gamma = 2.2;
            double[] gps = pixels.Select(x => x / 255.0).ToArray();//
            double[] gosaPixels = gps.Select(x => x).ToArray();
            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = y * stride + x;

                    //変換、0.5未満＝0、0.5以上＝1
                    var neko = Math.Pow(gosaPixels[p], gamma);
                    if (neko < 0.5) gps[p] = 0;
                    else gps[p] = 1;

                    //誤差拡散
                    gosa = (gosaPixels[p] - gps[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;//右
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;//下
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;//左下
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;//右下
                    }
                }
            }
            pixels = gps.Select(x => (byte)(x * 255)).ToArray();
            //pixels = gps.Select(x => (byte)((x * 255) + 0.5)).ToArray();//四捨五入？しても変化なし
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
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
            if (pixelFormat == null) { pixelFormat = PixelFormats.Gray8; }
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




        #region クリップボード
        //クリップボードから画像貼り付け
        private void ButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            //8bitグレースケールに変換して読み込む
            BitmapSource bmp = Clipboard.GetImage();
            if (bmp == null)
            {
                MessageBox.Show("not Image");
                return;
            }
            bmp = new FormatConvertedBitmap(bmp, PixelFormats.Gray8, null, 0);
            MyBitmapSource = bmp;
            MyImage.Source = MyBitmapSource;
            MyImageOrigin.Source = MyBitmapSource;
        }

        //クリップボードへ画像貼り付け
        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            if (MyImage.Source == null) return;
            Clipboard.SetImage((BitmapSource)MyImage.Source);
        }
        #endregion クリップボード


        private void ScrollBarCount_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ScrollBarCount.Value += ScrollBarCount.SmallChange;
            else ScrollBarCount.Value -= ScrollBarCount.SmallChange;
        }

        private void TextBlock_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) ScrollBarCount.Value += ScrollBarCount.LargeChange;
            else ScrollBarCount.Value -= ScrollBarCount.LargeChange;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button1.Content = nameof(D1);
            MyImage.Source = D1(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button2.Content = nameof(D2);
            MyImage.Source = D2(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button3.Content = nameof(D3);
            MyImage.Source = D3(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button4.Content = nameof(D4);
            MyImage.Source = D4(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button5.Content = nameof(D5);
            MyImage.Source = D5(MyBitmapSource, (int)ScrollBarCount.Value);
        }


        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button6.Content = nameof(D6);
            MyImage.Source = D6(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button7.Content = nameof(D7);
            MyImage.Source = D7(MyBitmapSource, (int)ScrollBarCount.Value);
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button8.Content = nameof(D8);
            MyImage.Source = D8(MyBitmapSource);
        }

        private void Button9_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapSource == null) return;
            Button9.Content = nameof(D9);
            MyImage.Source = D9(MyBitmapSource, (int)ScrollBarCount.Value);
        }
    }
}
