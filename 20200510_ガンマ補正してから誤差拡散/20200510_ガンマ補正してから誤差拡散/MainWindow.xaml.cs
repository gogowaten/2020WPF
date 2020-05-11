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
                vCorrect /= colorsCount - 1;
                vCorrect += begin;
                vCorrect *= 255.0;
                correctPixels[i] = vCorrect;
            }
            return correctPixels;
        }
        private BitmapSource D2(BitmapSource source, int colorsCount)
        {
            Dictionary<int, byte> convertTable = MakeTable(colorsCount);
            double stepRange = 1.0 / colorsCount;//0.333

            //画素値の配列作成
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            double gamma = 2.2;
            var gosaPixels = GammaCorrect1(pixels, colorsCount, gamma);

            //誤差拡散計算用
            //double[] gosaPixels = new double[height * stride];
            //Array.Copy(pixels, gosaPixels, height * stride);

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
                vCorrect = Math.Pow(vCorrect, ll);
                vCorrect /= colorsCount - 1;
                vCorrect += begin;
                vCorrect *= 255.0;
                correctPixels[i] = vCorrect;
            }
            return correctPixels;
        }
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


        //ガンマ補正は変換直前にする
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
                    gg = Math.Pow(gg, localGamma[index]);
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


        private double[] MakeLocalGamma3(int colorsCount, double gamma)
        {
            int count = colorsCount - 1;
            double step = 1.0 / count;
            double[] vs = new double[colorsCount];

            double[] temp = new double[colorsCount];
            for (int i = 0; i < count; i++)
            {
                var dd = Math.Pow(step * (i + 1), 1 / 2.2) - Math.Pow(step * i, 1 / 2.2);
                temp[i] = dd * 1.2 + 1;
            }
            temp[1] = 1.2;
            temp[count] = 1.0;
            return temp;
        }
        private double[] GammaCorrect3(byte[] pixels, int colorsCount, double gamma)
        {
            double[] localGamma = MakeLocalGamma3(colorsCount, gamma);
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
            var gosaPixels = GammaCorrect3(pixels, colorsCount, gamma);

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














        /// <summary>
        /// ガンマ補正なしで0 127 255の3色に減色、FloydSteinbergで誤差拡散、PixelFormat.Gray8グレースケール画像専用
        /// 0～84を0に変換、85～170を127、それ以外を255に変換
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private BitmapSource D1_Color3(BitmapSource source)
        {
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
                    //3色に変換
                    if (gosaPixels[p] < 85) pixels[p] = 0;
                    else if (gosaPixels[p] < 171) pixels[p] = 127;
                    else pixels[p] = 255;

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


        /// <summary>
        /// もとの値を逆側にガンマ補正して返す、double型
        /// </summary>
        /// <param name="pixels">もとの値の配列</param>
        /// <param name="gamma">ガンマ値</param>
        /// <returns></returns>
        private double[] InvertGamma2Color(byte[] pixels, double gamma)
        {
            double[] vs = new double[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                double d = pixels[i] / 255.0;
                vs[i] = Math.Pow(d, gamma) * 255.0;
            }
            return vs;
        }

        //0.0～1.0までを一括2.2でガンマ補正してから変換
        /// <summary>
        /// 全体をガンマ値2.2で補正してから減色、誤差拡散
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private BitmapSource D2_Color3Gamma(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //2.2で逆ガンマ補正した値の配列、誤差拡散計算用
            double[] gosaPixels = InvertGamma2Color(pixels, 2.2);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            //  * 7
            //3 5 1
            //￣16￣
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //注目ピクセルのインデックス
                    p = y * stride + x;
                    //3色に変換
                    if (gosaPixels[p] < 85) pixels[p] = 0;
                    else if (gosaPixels[p] < 171) pixels[p] = 127;
                    else pixels[p] = 255;

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }



        /// <summary>
        /// 3色用ガンマ補正用、0～0.5までと0.5～1.0までを、それぞれ2.2で補正した配列を返す
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="gamma"></param>
        /// <returns></returns>
        private double[] InvertGamma3Color(byte[] pixels, double gamma)
        {
            double[] vs = new double[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                double d = pixels[i] / 255.0;
                if (d <= 0.5)
                {
                    vs[i] = (Math.Pow(d * 2, gamma) / 2.0 * 255);
                }
                else
                {
                    vs[i] = ((0.5 + Math.Pow((d - 0.5) * 2, gamma) / 2.0) * 255);
                }
            }
            return vs;
        }

        //0.0～0.5と0.5～1.0に2分して別々にガンマ補正、ガンマ値は両方とも2.2
        private BitmapSource D3_Color3EachGamma(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);

            //2.2で逆ガンマ補正した値の配列、誤差拡散計算用
            double[] gosaPixels = InvertGamma3Color(pixels, 2.2);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            //  * 7
            //3 5 1
            //￣16￣
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //注目ピクセルのインデックス
                    p = y * stride + x;
                    //3色に変換
                    if (gosaPixels[p] < 85) pixels[p] = 0;
                    else if (gosaPixels[p] < 171) pixels[p] = 127;
                    else pixels[p] = 255;

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }

        //E:\オレ\エクセル\画像処理.xlsm_ガンマ補正_$E$71

        /// <summary>
        /// 3色用ガンマ補正、0～0.5までと0.5～1.0までを指定したガンマ値で補正する
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="gamma1">0.0～0.5までのガンマ値</param>
        /// <param name="gamma2">0.5～1.0までのガンマ値</param>
        /// <returns></returns>
        private double[] InvertEachGamma3Color(byte[] pixels, double gamma1, double gamma2)
        {
            double[] vs = new double[pixels.Length];
            //0～255を0～1に変換して補正してから、また0～255に戻す
            for (int i = 0; i < pixels.Length; i++)
            {
                double d = pixels[i] / 255.0;
                if (d <= 0.5)
                {
                    vs[i] = Math.Pow(d * 2, gamma1) / 2.0 * 255;
                }
                else
                {
                    //0.5～1.0区間は
                    // = 開始値 + ((入力値 - 開始値) * 分割数) ^ ガンマ値 / 分割数
                    // = 0.5 + ((入力値 - 0.5) * 2) ^ ガンマ値 / 2
                    vs[i] = (0.5 + (Math.Pow((d - 0.5) * 2, gamma2) / 2.0)) * 255;
                }
            }
            return vs;
        }

        //0.0～0.5と0.5～1.0を別々にガンマ補正、ガンマ値は
        //0.0～0.5は2.2
        //0.5～1.0は計算して1.3243119で補正
        private BitmapSource D4_Color3EachGamma2(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);
            //0～0.5区間で使うガンマ値は初期の2.2
            double gamma1 = 2.2;

            //0.5～1.0区間で使うガンマ値を計算
            //開始値は0.5これを初期γで補正する

            //補正後 = 0.72974005
            // = 開始値 ^ (1 / 初期γ)
            // = 0.5 ^ (1 / 2.2)
            // = 0.72974005
            //0.5のガンマ補正後の値 = 0.7297401
            double correct = Math.Pow(0.5, 1.0 / gamma1);

            //0.5～1.0区間で使うガンマ値を、0.5の補正後値を使って計算すると
            // = (-ガンマ値全体距離 * 補正後) + 初期γ
            // = -(2.2 - 1.0) * 0.72974005) + 2.2
            // = 1.3243119
            double gamma2 = -(gamma1 - 1.0) * correct + gamma1;

            //逆ガンマ補正した値の配列、誤差拡散計算用
            double[] gosaPixels = InvertEachGamma3Color(pixels, gamma1, gamma2);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)

            //  * 7
            //3 5 1
            //￣16￣
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //注目ピクセルのインデックス
                    p = y * stride + x;
                    //3色に変換
                    if (gosaPixels[p] < 85) pixels[p] = 0;
                    else if (gosaPixels[p] < 171) pixels[p] = 127;
                    else pixels[p] = 255;

                    //誤差拡散
                    gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                    if (x != width - 1)
                        gosaPixels[p + 1] += gosa * 7;
                    if (y < height - 1)
                    {
                        p += stride;
                        gosaPixels[p] += gosa * 5;
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 3;
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 1;
                    }
                }
            }
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, stride);
        }


        private BitmapSource D5_Color3EachGamma2蛇行走査(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width;
            byte[] pixels = new byte[height * stride];//もとの値Pixels
            source.CopyPixels(pixels, stride, 0);
            //0～0.5区間で使うガンマ値は初期の2.2
            double gamma1 = 2.2;

            //0.5～1.0区間で使うガンマ値を計算
            //開始値は0.5これを初期γで補正する

            //補正後 = 0.72974005
            // = 開始値 ^ (1 / 初期γ)
            // = 0.5 ^ (1 / 2.2)
            // = 0.72974005
            //0.5のガンマ補正後の値 = 0.7297401
            //double correct = Math.Pow(0.5, gamma1);
            double correct = Math.Pow(0.5, 1.0 / gamma1);

            //0.5～1.0区間で使うガンマ値を、0.5の補正後値を使って計算すると
            // = (-ガンマ値全体距離 * 補正後) + 初期γ
            // = -(2.2 - 1.0) * 0.72974005) + 2.2
            // = 1.3243119
            double gamma2 = -(gamma1 - 1.0) * correct + gamma1;

            //逆ガンマ補正した値の配列、誤差拡散計算用
            double[] gosaPixels = InvertEachGamma3Color(pixels, gamma1, gamma2);

            int p;//座標を配列のインデックスに変換した値用
            double gosa;//誤差(変換前 - 変換後)


            for (int y = 0; y < height; y++)
            {
                if (y % 2 == 0)
                {
                    //  * 7
                    //3 5 1
                    //￣16￣
                    for (int x = 0; x < width; x++)
                    {
                        //注目ピクセルのインデックス
                        p = y * stride + x;
                        //3色に変換
                        if (gosaPixels[p] < 85) pixels[p] = 0;
                        else if (gosaPixels[p] < 171) pixels[p] = 127;
                        else pixels[p] = 255;

                        //誤差拡散
                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != width - 1)
                            gosaPixels[p + 1] += gosa * 7;
                        if (y < height - 1)
                        {
                            p += stride;
                            gosaPixels[p] += gosa * 5;
                            if (x != 0)
                                gosaPixels[p - 1] += gosa * 3;
                            if (x != width - 1)
                                gosaPixels[p + 1] += gosa * 1;
                        }
                    }
                }
                else
                {
                    //7 *
                    //1 5 3
                    //￣16￣
                    for (int x = width - 1; x >= 0; x--)
                    {
                        //注目ピクセルのインデックス
                        p = y * stride + x;
                        //3色に変換
                        if (gosaPixels[p] < 85) pixels[p] = 0;
                        else if (gosaPixels[p] < 171) pixels[p] = 127;
                        else pixels[p] = 255;

                        //誤差拡散
                        gosa = (gosaPixels[p] - pixels[p]) / 16.0;
                        if (x != 0)
                            gosaPixels[p - 1] += gosa * 7;
                        if (y < height - 1)
                        {
                            p += stride;
                            gosaPixels[p] += gosa * 5;
                            if (x != 0)
                                gosaPixels[p - 1] += gosa * 1;
                            if (x != width - 1)
                                gosaPixels[p + 1] += gosa * 3;
                        }
                    }
                }
            }
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
    }
}
