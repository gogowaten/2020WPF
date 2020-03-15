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

//だいたいできた、Cube5がそれ

namespace _20200311_減色_デザインパターン
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] MyPixels;
        private const int PIXELS_COUNT = 25500;
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

            var cube = new Cube1(MyPixels);
            cube.SplitCube(3, cube);

            var c3 = new SelecterA3(MyPixels);


            var ccc5 = new Cube5(MyPixels, new SelectCube5_LongSide(), new SplitCube5_SideMid());
            ccc5.ExeGensyoku(8);
            var neko = ccc5.Cubes;
            ccc5 = new Cube5(MyPixels, new SelectCube5_ManyPicexls(), new SplitCube5_Mid());            
            ccc5.ExeGensyoku(8);
            neko = ccc5.Cubes;
        }
    }

    //選択と分割の種類が増えると管理が大変だし、計算に使うプロパティの初期化も関係ないものまで全部計算するか、
    //必要なのものだけその都度計算するかになって、どちらにしても無駄が多くなる
    #region 一括クラス、
    public class Cube1
    {
        public byte[] Values;
        public List<Cube1> Cubes;
        public byte Min;
        public byte Max;
        public int Count;

        public Cube1(byte[] vs)
        {
            Values = vs;
            Cubes = new List<Cube1>();
            Cubes.Add(this);
            Initialize();
        }
        private void Initialize()
        {
            Min = byte.MaxValue;
            Max = byte.MinValue;
            Count = Values.Length;
            for (int i = 0; i < Count; i++)
            {
                var v = Values[i];
                if (Min > v) Min = v;
                if (Max < v) Max = v;
            }
        }

        private Cube1 SelectA(List<Cube1> cubes)
        {
            int side = 0;
            Cube1 cube = cubes[0];
            foreach (var item in cubes)
            {
                if (side < item.Max - item.Min) cube = item;
            }
            return cube;
        }
        private (Cube1 cubeA, Cube1 cubeB) SplitA(Cube1 cube)
        {
            int mid = (int)((cube.Max + cube.Min) / 2.0);
            return (new Cube1(cube.Values.Where((x) => x > mid).ToArray()),
                new Cube1(cube.Values.Where((x) => x <= mid).ToArray()));
        }

        public void SplitCube(int count, Cube1 cube)
        {
            while (cube.Cubes.Count < count)
            {
                var c = SelectA(cube.Cubes);
                var cc = SplitA(c);
                cube.Cubes.Remove(c);
                cube.Cubes.Add(cc.cubeA);
                cube.Cubes.Add(cc.cubeB);
            }

        }
    }



    #endregion
    //__________________________________________
    //組み合わせが増えても問題ないけど、初期化ができないのでその都度計算になって無駄がかなり多くなる
    #region 選択と分割を別クラス。

    public interface ISelecter
    {
        Cube2 Select(List<Cube2> cubes);
    }
    public class SelecterA : ISelecter
    {
        public Cube2 Select(List<Cube2> cubes)
        {
            throw new NotImplementedException();
        }
    }

    public interface ISplitter2
    {
        (Cube2 cubeA, Cube2 cubeB) Split(Cube2 cube);
    }
    public class SplitterA : ISplitter2
    {
        public (Cube2 cubeA, Cube2 cubeB) Split(Cube2 cube)
        {
            throw new NotImplementedException();
        }
    }
    public class Cube2
    {
        private byte[] Pixels;
        private ISelecter Select;
        private ISplitter2 Splitter;
        public List<Cube2> ListCubes = new List<Cube2>();
        public Cube2(byte[] pixesl, ISelecter selecter, ISplitter2 splitter)
        {
            Pixels = pixesl;
            ListCubes.Add(this);
            Select = selecter;
            Splitter = splitter;
        }

        public void Split(int count)
        {
            while (ListCubes.Count < count)
            {
                Splitter.Split(Select.Select(this.ListCubes));
            }
        }
    }

    #endregion

    //必要なプロパティ群を、インターフェースを付加したクラスに置いてみたけど、うまく行かない
    #region
    public interface ISelecter3
    {
        Cube3 Select(List<Cube3> cubes);
    }
    public class SelecterA3 : ISelecter3
    {
        public byte Min;
        public byte Max;
        public SelecterA3(byte[] pixels)
        {
            //ここでminmaxを計算
        }

        public Cube3 Select(List<Cube3> cubes)
        {
            throw new NotImplementedException();
        }
    }

    public interface ISplitter3
    {
        (Cube3 cubeA, Cube3 cubeB) Split(Cube3 cube);
    }
    public class SplitterA3 : ISplitter3
    {
        ISelecter3 Selecter;
        public SplitterA3(ISelecter3 selecter)
        {
            Selecter = selecter;
        }
        public (Cube3 cubeA, Cube3 cubeB) Split(Cube3 cube)
        {
            throw new NotImplementedException();
        }
    }
    //Cube自体にCubeを作成する機能、セレクターとスプリッターの種類のenumをもたせる
    public class Cube3
    {
        public SelectType SelectType;
        public SplitType SplitType;
        public ISelecter3 Selecter;
        public ISplitter3 Splitter;
        private List<Cube3> CubeList = new List<Cube3>();
        private byte[] Pixels;
        public Cube3(byte[] pixels, SelectType selectType, SplitType splitType)
        {
            Pixels = pixels;
            CubeList.Add(this);
            SelectType = selectType;
            SplitType = splitType;
        }
        public void Exe(int count)
        {
            Splitter.Split(Selecter.Select(CubeList));
        }
    }
    public class Tukau3
    {
        private void Test()
        {
            var pixels = new byte[40];
            var neko = new Cube3(pixels, SelectType.SideLong, SplitType.SideMid);

        }
    }
    #endregion

    //____________________________________________________
    #region
    public abstract class SelecterBase
    {
        public byte Min;
        public byte Max;
    }
    public class Selecter4 : SelecterBase
    {
        byte[] Pixels;
        public Selecter4(byte[] pixels)
        {
            Pixels = pixels;
            Min = 0;
            Max = 255;
        }
    }
    public interface ISplitter4
    {
        public (Cube4 cubeA, Cube4 cubeB) Split(Cube4 cube);
    }
    public class SplitterA4 : ISplitter4
    {
        public (Cube4 cubeA, Cube4 cubeB) Split(Cube4 cube)
        {
            throw new NotImplementedException();
        }
    }
    public class Cube4
    {

    }
    #endregion

    //___________________________________________________

    #region
    public interface ISelectCube5
    {
        public void Calc(Cube5 cube);
        public Cube5 Select(Cube5 cube);
    }
    public class SelectCube5_LongSide : ISelectCube5
    {
        public void Calc(Cube5 cube)
        {
            byte min = byte.MaxValue;
            byte max = byte.MinValue;
            foreach (var item in cube.Pixels)
            {
                if (min > item) min = item;
                if (max < item) max = item;
            }
            cube.Min = min; cube.Max = max;
            cube.IsCalcMinMax = true;
        }

        public Cube5 Select(Cube5 cube)
        {

            Cube5 result = cube.Cubes[0];
            int length = result.Max - result.Min;
            foreach (var item in cube.Cubes)
            {
                if (length < item.Max - item.Min) result = item;
            }
            return result;
        }
    }
    public class SelectCube5_ManyPicexls : ISelectCube5
    {
        public void Calc(Cube5 cube)
        {
            cube.Count = cube.Pixels.Length;
            cube.IsCalcCount = true;
        }

        public Cube5 Select(Cube5 cube)
        {
            Cube5 result = cube.Cubes[0];
            foreach (var item in cube.Cubes)
            {
                if (result.Pixels.Length < item.Pixels.Length)
                    result = item;
            }
            return result;
        }
    }
    public interface ISplitCube5
    {
        public void Calc(Cube5 cube);
        public (Cube5 cubeA, Cube5 cubeB) Split(Cube5 cube);
    }
    //辺の中央で分割
    public class SplitCube5_SideMid : ISplitCube5
    {
        public void Calc(Cube5 cube)
        {
            //if (cube.IsCalcMinMax == false)
            //{
            //    new CalcA().Calc(cube);
            //}
        }

        public (Cube5 cubeA, Cube5 cubeB) Split(Cube5 cube)
        {
            if (cube.IsCalcMinMax == false) new SelectCube5_LongSide().Calc(cube);
            var pixA = new List<byte>();
            var pixB = new List<byte>();
            int mid = (int)((cube.Max + cube.Min) / 2.0);
            foreach (var item in cube.Pixels)
            {
                if (item < mid)
                {
                    pixA.Add(item);
                }
                else
                {
                    pixB.Add(item);
                }
            }
            var cuA = new Cube5(pixA.ToArray(), cube.Selecter, cube.Splitter);
            var cuB = new Cube5(pixB.ToArray(), cube.Selecter, cube.Splitter);
            return (cuA, cuB);
        }
    }
    //中央値
    public class SplitCube5_Mid : ISplitCube5
    {
        public void Calc(Cube5 cube)
        {
            //if (cube.IsCalcCount == false) new CalcB().Calc(cube);
        }

        public (Cube5 cubeA, Cube5 cubeB) Split(Cube5 cube)
        {
            if (cube.IsCalcCount == false) new SelectCube5_ManyPicexls().Calc(cube);
            var pixA = new List<byte>();
            var pixB = new List<byte>();
            byte[] neko = cube.Pixels.OrderBy(x => x).ToArray();

            int mid = neko[neko.Length / 2];
            foreach (var item in cube.Pixels)
            {
                if (item < mid)
                {
                    pixA.Add(item);
                }
                else
                {
                    pixB.Add(item);
                }
            }
            var cuA = new Cube5(pixA.ToArray(), cube.Selecter, cube.Splitter);
            var cuB = new Cube5(pixB.ToArray(), cube.Selecter, cube.Splitter);
            return (cuA, cuB);
        }
    }

    public class Cube5
    {
        //public SelectType SelectType;
        //public SplitType SplitType;
        public ISelectCube5 Selecter;
        public ISplitCube5 Splitter;
        public List<Cube5> Cubes = new List<Cube5>();
        public byte[] Pixels;
        public byte Min;
        public byte Max;
        public bool IsCalcMinMax = false;
        public int Count;
        public bool IsCalcCount = false;
        public Cube5(byte[] vs, ISelectCube5 calc, ISplitCube5 splitCalc)
        {
            Pixels = vs;
            //SelectType = selectType;
            //SplitType = splitType;
            Cubes.Add(this);
            Selecter = calc;
            Splitter = splitCalc;
        }
        public void Calc()
        {
            Selecter.Calc(this);
        }
        public void Select()
        {
            var ccc = Selecter.Select(this);
            var neko = ccc;
        }
        public void ExeSplitCalc()
        {
            Splitter.Calc(this);
        }
        public void ExeSplit()
        {
            var ccc = Selecter.Select(this);
            var neko = Splitter.Split(ccc);
        }
        public void ExeGensyoku(int count)
        {
            while (Cubes.Count < count)
            {
                Cube5 ccc = Selecter.Select(this);
                var (cubeA, cubeB) = Splitter.Split(ccc);
                Cubes.Remove(ccc);
                Cubes.Add(cubeA);
                Cubes.Add(cubeB);
            }
        }
        public override string ToString()
        {
            return $"{Pixels.Length}個 Min={Min} Max={Max}";
        }
    }
    public class Tukau5
    {
        private void Test()
        {
            //var pixels = new byte[40];
            //var neko = new Cube5(pixels, SelectType.SideLong, SplitType.SideMid, new CalcA(), new SplitA());

        }

    }










    #endregion



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

        public enum KeyColor
        {
            None = 0,
            Red,
            Green,
            Blue,
        }
    }




    public abstract class Entry
    {
        public byte[] Values;
    }
    public class File : Entry
    {

    }
    public class Files
    {
        List<Entry> entries;
        public void Add(Entry entry) => entries.Add(entry);
    }
    public class Tukau
    {
        private void Test()
        {
            var f = new File();

        }
    }
    //選択タイプ
    public enum SelectType
    {
        SideLong = 1,
        ManyPixels,
        Taiseki,
        Varient,
    }
    //分割タイプ
    public enum SplitType
    {
        SideMid = 1,
        PixelsMid,
        Ootu
    }


}