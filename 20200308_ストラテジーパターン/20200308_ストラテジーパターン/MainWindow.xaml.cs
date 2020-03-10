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

//    ストラテジー: C#　プログラミング　再入門
//http://dotnetcsharptips.seesaa.net/article/406029274.html?seesaa_related=category

namespace _20200308_ストラテジーパターン
{



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var neko = new Context(new StrategyA());
            neko.Execute();
            neko.Strategy = new StrategyB();
            neko.Execute();

        }
    }

    interface IStrategy
    {
        void DoSomething();
    }

    class StrategyA : IStrategy
    {
        public void DoSomething() => Debug.WriteLine("Strategy A.");
    }

    class StrategyB : IStrategy
    {
        public void DoSomething() => Debug.WriteLine("Strategy B.");
    }

    class Context
    {
        public IStrategy Strategy { set; private get; }
        public Context(IStrategy strategy) => Strategy = strategy;
        public void Execute() => Strategy.DoSomething();
    }


}



