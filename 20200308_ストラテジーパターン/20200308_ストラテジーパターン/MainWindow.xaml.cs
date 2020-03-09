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

//C#で学ぶデザインパターン入門 ⑥Prototype - Qiita
//https://qiita.com/toshi0607/items/f4358020befca048d2e0

//Factory Methodに近いけど、これだと大げさかな、ちょっと違う
//Prototype、これのが近いかな
//Builderは違う
//Abstract Factory、近い気もするけど難しすぎてわからん
//Bridge、いい気がするけど少し違うかも、いやかなり近いかも、namespace使わないのがいい


namespace _20200308_ストラテジーパターン
{
    using Framework;


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //var neko = Encoding.GetEncodings();
            //foreach (var item in neko)
            //{
            //    Debug.WriteLine(item.DisplayName);
            //    Debug.WriteLine(item.Name);
            //}

            Display d1 = new Display(new StringDisplayImpl("Hello, Japan."));
            Display d2 = new CountDisplay(new StringDisplayImpl("Hello, World."));
            CountDisplay d3 = new CountDisplay(new StringDisplayImpl("Hello, Universe."));
            d1.Show();
            d2.Show();
            d3.Show();
            d3.MultiDisplay(2);
            var dd = new CountDisplay(new StringDisplayImpl("yukkuri"));
            
        }
    }


    public class Display
    {
        private DisplayImpl impl;
        public Display(DisplayImpl impl)
        {
            this.impl = impl;
        }

        public void Open()
        {
            impl.RawOpen();
        }

        public void Print()
        {
            impl.RawPrint();
        }

        public void Close()
        {
            impl.RawClose();
        }

        public void Show()
        {
            Open();
            Print();
            Close();
        }
    }

    // RefinedAbstraction
    // ・Abstractionに対して機能を追加
    public class CountDisplay : Display
    {
        public CountDisplay(DisplayImpl impl) : base(impl) { }

        public void MultiDisplay(int times)
        {
            Open();
            for (int i = 0; i < times; i++)
            {
                Print();
            }
            Close();
        }
    }

    // Implementor
    // ・実装のクラス階層の最上位クラス
    // ・Abstranctionのインターフェース（API）を規定する
    public abstract class DisplayImpl
    {
        public abstract void RawOpen();
        public abstract void RawPrint();
        public abstract void RawClose();
    }

    // ConcreteImplementator
    // ・Implementatorを具体的に実装する
    public class StringDisplayImpl : DisplayImpl
    {
        private string str;
        private int width;
        public StringDisplayImpl(string str)
        {
            this.str = str;
            Encoding sjisEnc = Encoding.GetEncoding("shift_jis");
            this.width = sjisEnc.GetByteCount(str);
        }

        public override void RawOpen()
        {
            PrintLine();
        }

        public override void RawPrint()
        {
            Console.WriteLine($"|{str}|");
        }

        public override void RawClose()
        {
            PrintLine();
        }

        public void PrintLine()
        {
            Console.Write("+");
            for (int i = 0; i < width; i++)
            {
                Console.Write("-");
            }
            Console.WriteLine("+");
        }
    }
}


namespace Framework
{
    // Prototype
    public interface Product : ICloneable
    {
        void Use(string s);
        Product CreateClone();
    }

    // Client
    public class Manager
    {
        private Dictionary<string, Product> showcase = new Dictionary<string, Product>();
        public void Register(string name, Product proto)
        {
            showcase.Add(name, proto);
        }
        public Product Create(string protoname)
        {
            Product p = showcase[protoname];
            return p.CreateClone();
        }
    }
}





