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
using System.Numerics;

namespace _20200131_減色とデザインパターン
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var inu = new SelectLongSide();

            byte[] pixels = new byte[10];
            var neko = new Cube(pixels, new SelectLongSide(), new SplitMidlle());
            neko.KeyColorBGR = Cube.KeyColor.Blue;
            neko.MaxSideLength = 1;
        }
    }

    public interface ISelect
    {
        Cube Select(List<Cube> cubes);
    }
    public interface ISplit
    {
        void Split();
    }
    public class SelectLongSide : ISelect
    {
        public Cube Select(List<Cube> cubes)
        {
            Cube selected = cubes[0];
            if (cubes.Count == 1) return selected;

            int maxLength = GetSetSelectKeyColorLength(cubes[0]);
            for (int i = 1; i < cubes.Count; i++)
            {
                if (maxLength < GetSetSelectKeyColorLength(cubes[i]))
                {
                    selected = cubes[i];
                }
            }
            return selected;
        }
        //最大長辺の色と長さを設定して、長さを返す
        private int GetSetSelectKeyColorLength(Cube cube)
        {
            int r = cube.MaxR - cube.MinR;
            int g = cube.MaxG - cube.MinG;
            int b = cube.MaxB - cube.MinB;
            int maxLength;
            Cube.KeyColor keyColor;
            if (r > g)
            {
                if (r > b)
                {
                    maxLength = r;
                    keyColor = Cube.KeyColor.Red;
                }
                else
                {
                    maxLength = b;
                    keyColor = Cube.KeyColor.Blue;
                }
            }
            else if (g > b)
            {
                maxLength = g;
                keyColor = Cube.KeyColor.Green;
            }

            else
            {
                maxLength = b;
                keyColor = Cube.KeyColor.Blue;
            }
            cube.MaxSideLength = maxLength;
            cube.KeyColorBGR = keyColor;
            return maxLength;
        }
    }
    public class SplitMidlle : ISplit
    {
        public void Split()
        {
            
        }
    }
    public abstract class CubeBase
    {

    }
    public class Cube : CubeBase
    {
        public enum KeyColor
        {
            None = -1, Blue = 0, Green = 1, Red = 2
        }
        public byte MaxR { get; private set; }
        public byte MaxG { get; private set; }
        public byte MaxB { get; private set; }
        public byte MinR { get; private set; }
        public byte MinG { get; private set; }
        public byte MinB { get; private set; }
        public byte[] RedArray { get; private set; }
        public byte[] GreArray { get; private set; }
        public byte[] BluArray { get; private set; }
        public int ColorCount { get; private set; }
        public KeyColor KeyColorBGR;
        public int MaxSideLength;
        public List<Cube> Cubes { get; private set; }
        private ISelect SelectType;
        private ISplit SplitType;


        public Cube(byte[] redArray, byte[] greenArray, byte[] blueArray, ISelect select, ISplit split)
        {
            ColorCount = redArray.Length;
            Initialize(redArray, greenArray, blueArray);
            SelectType = select;
            SplitType = split;
        }
        public Cube(byte[] pixels, ISelect select, ISplit split)
        {
            ColorCount = pixels.Length / 4;
            SelectType = select;
            SplitType = split;
            var redArray = new byte[ColorCount];
            var greenArray = new byte[ColorCount];
            var blueArray = new byte[ColorCount];
            for (int i = 0; i < ColorCount; i++)
            {
                redArray[i] = pixels[i * 4 + 2];
                greenArray[i] = pixels[i * 4 + 1];
                blueArray[i] = pixels[i * 4];
            }

            Initialize(redArray, greenArray, blueArray);
        }
        private void Initialize(byte[] redArray, byte[] greenArray, byte[] blueArray)
        {
            RedArray = redArray;
            GreArray = greenArray;
            BluArray = blueArray;
            SetMinMax();
            Cubes = new List<Cube>();
            Cubes.Add(this);
            //SetSelectKey();
        }

        private void SetMinMax()
        {
            var simdLength = Vector<byte>.Count;
            if (simdLength > ColorCount)
            {
                SetMinMax2();
                return;
            }

            var vMaxR = new Vector<byte>(byte.MinValue);
            var vMaxG = new Vector<byte>(byte.MinValue);
            var vMaxB = new Vector<byte>(byte.MinValue);
            var vMinR = new Vector<byte>(byte.MaxValue);
            var vMinG = new Vector<byte>(byte.MaxValue);
            var vMinB = new Vector<byte>(byte.MaxValue);

            int myLast = ColorCount - simdLength;
            for (int i = 0; i < myLast; i += simdLength)
            {
                vMaxR = System.Numerics.Vector.Max(vMaxR, new Vector<byte>(RedArray, i));
                vMaxG = System.Numerics.Vector.Max(vMaxG, new Vector<byte>(GreArray, i));
                vMaxB = System.Numerics.Vector.Max(vMaxB, new Vector<byte>(BluArray, i));
                vMinR = System.Numerics.Vector.Min(vMinR, new Vector<byte>(RedArray, i));
                vMinG = System.Numerics.Vector.Min(vMinG, new Vector<byte>(GreArray, i));
                vMinB = System.Numerics.Vector.Min(vMinB, new Vector<byte>(BluArray, i));
            }

            MaxR = vMaxR[0];
            MaxG = vMaxG[0];
            MaxB = vMaxB[0];
            MinR = vMinR[0];
            MinG = vMinG[0];
            MinB = vMinB[0];
            for (int i = 1; i < simdLength; i++)
            {
                if (MaxR < vMaxR[i]) MaxR = vMaxR[i];
                if (MaxG < vMaxG[i]) MaxG = vMaxG[i];
                if (MaxB < vMaxB[i]) MaxB = vMaxB[i];
                if (MinR > vMinR[i]) MinR = vMinR[i];
                if (MinG > vMinG[i]) MinG = vMinG[i];
                if (MinB > vMinB[i]) MinB = vMinB[i];
            }

            for (int i = myLast; i < ColorCount; i++)
            {
                MaxR = Math.Max(MaxR, RedArray[i]);
                MaxG = Math.Max(MaxG, GreArray[i]);
                MaxB = Math.Max(MaxB, BluArray[i]);
                MinR = Math.Min(MinR, RedArray[i]);
                MinG = Math.Min(MinG, GreArray[i]);
                MinB = Math.Min(MinB, BluArray[i]);
            }
        }
        private void SetMinMax2()
        {
            for (int i = 0; i < ColorCount; i++)
            {
                MaxR = Math.Max(MaxR, RedArray[i]);
                MaxG = Math.Max(MaxG, GreArray[i]);
                MaxB = Math.Max(MaxB, BluArray[i]);
                MinR = Math.Min(MinR, RedArray[i]);
                MinG = Math.Min(MinG, GreArray[i]);
                MinB = Math.Min(MinB, BluArray[i]);
            }
        }

        public void Bunkatu(int count)
        {
            SelectType.Select(Cubes);
            SplitType.Split();
        }



    }
}
