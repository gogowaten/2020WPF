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

using System.Diagnostics;


namespace _20200311_減色_デザインパターン
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyPixels;
        private const int PIXELS_COUNT = 40;
        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.GetType().Namespace;// this.ToString();

            MyPixels = new byte[PIXELS_COUNT];
            for (int i = 0; i < PIXELS_COUNT; i++)
            {
                MyPixels[i] = (byte)i;
            }
            var r = new Random();
            r.NextBytes(MyPixels);

            Cube cube = new Cube(MyPixels);
            var c2 = new MyAbSelect(new AbSelectA(cube));
            c2.Exe();
            c2.AbSelect = new AbSelectB(cube);
            c2.Exe();

        }
    }

    public abstract class AbSelect
    {
        public Cube Cube;

        public AbSelect(Cube cube)
        {
            Cube = cube;
        }

        public abstract void Select();

    }
    public class AbSelectA : AbSelect
    {

        public int SideLength { get; private set; }

        public AbSelectA(Cube cube) : base(cube)
        {
            Cube = cube;

        }

        public override void Select()
        {
            Debug.WriteLine($"{GetType().Name}");
        }


    }

    public class AbSelectB : AbSelect
    {
        public AbSelectB(Cube cube) : base(cube)
        {
            Cube = cube;
        }



        public override void Select()
        {
            Debug.WriteLine($"{GetType().Name}");
        }
    }

    public class MyAbSelect
    {
        public AbSelect AbSelect { get; set; }
        public MyAbSelect(AbSelect abSelect)
        {
            this.AbSelect = abSelect;
        }
        public void Exe()
        {
            AbSelect.Select();
        }
    }

    #region インターフェースだとプロパティをもたせられない
    public interface ISelect
    {
        void Select();
    }
    public class SelectA : ISelect
    {
        public void Select() => Debug.WriteLine($"{GetType().Name}  {System.Reflection.MethodBase.GetCurrentMethod().Name}");
    }
    public class SelectB : ISelect
    {
        public void Select() => Debug.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod().Name}");
    }
    public class MySelect
    {
        public ISelect Selecter { get; set; }
        public MySelect(ISelect select)
        {
            Selecter = select;
        }
        public void Exe()
        {
            Selecter.Select();
        }
    }
    #endregion



    public class Cube
    {
        public List<Cube> Cubes { get; set; }
        public int Count { get; private set; }
        public byte Value { get; set; }
        //public KeyColor KeyColor { get; set; }
        public byte Min { get; set; }
        public byte Max { get; set; }
        public int SideLength { get; set; }
        public int[] Histogram { get; set; }


        public Cube(byte[] pixels)
        {
            Count = pixels.Length;
            Cubes = new List<Cube>();
            Cubes.Add(this);
            Min = byte.MaxValue;
            Max = byte.MinValue;
            Histogram = new int[256];
            byte p;
            for (int i = 0; i < Count; i++)
            {
                p = pixels[i];
                if (Min > p) Min = p;
                if (Max < p) Max = p;
                Histogram[p]++;
            }
            SideLength = Max - Min;


        }


        public abstract class Entry
        {
            public byte[] Values;
        }
        public enum KeyColor
        {
            None = 0,
            Red,
            Green,
            Blue,
        }
    }
}