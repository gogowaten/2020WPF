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
using System.Reflection;


//【Unity】Unityで学ぶStrategyパターン！モンスターに色んな技を使わせよう！【C#】【プログラム設計】 | サプライドの技術者BLOG
//https://techlife.supride.jp/archives/1411

namespace _20200131_ストラテジーパターン
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //インターフェース版
            var inu = new LLL(new Selecter1(), new Splitter1());
            inu.Execute();
            inu = new LLL(new Selecter2(), new Splitter1());
            inu.Execute();

            //抽象クラス版
            AbstTestClass neko;
            neko = new AbstTestClass(new AbsClsSelecter1(), new AbsClsSplitter1());
            //neko.Select();
            //neko.Split();
            neko.Execute();
            neko = new AbstTestClass(new AbsClsSelecter2(), new AbsClsSplitter2());
            //neko.Select();
            //neko.Split();
            neko.Execute();
         
        }
    }

    #region インターフェース版
    public interface ISelect
    {
        void Select();
    }
    public class Selecter1 : ISelect
    {
        public void Select() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    }
    public class Selecter2 : ISelect
    {
        public void Select() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    }
    public interface ISplit
    {
        void Split();
    }
    public class Splitter1 : ISplit
    {
        public void Split() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    }
    public class Splitter2 : ISplit
    {
        public void Split() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    }


    public class LLL
    {
        private ISelect Ee;//ここはインターフェースでもいいみたい、これで選択できる
        private ISplit Ff;
        public LLL(ISelect e1, ISplit f1)
        {
            Ee = e1;
            Ff = f1;
        }

        public void Execute()
        {
            Ee.Select();
            Ff.Split();
        }
    }
    #endregion



    #region 抽象クラス版
    public abstract class AbstSelectBase
    {
        public abstract void Select();
    }
    public class AbsClsSelecter1 : AbstSelectBase
    {
        public override void Select() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    }
    public class AbsClsSelecter2 : AbstSelectBase
    {
        public override void Select() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    }

    public abstract class AbstSplitBase
    {
        public abstract void Split();
    }
    public class AbsClsSplitter1 : AbstSplitBase
    {
        public override void Split() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    }
    public class AbsClsSplitter2 : AbstSplitBase
    {
        public override void Split() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    }

    public class AbstTestClass
    {
        private AbstSelectBase SelectBase;
        private AbstSplitBase SplitBase;
        public AbstTestClass(AbstSelectBase selectB, AbstSplitBase splitB)
        {
            this.SelectBase = selectB;
            this.SplitBase = splitB;
        }

        public void Select() => SelectBase.Select();
        public void Split() => SplitBase.Split();
        public void Execute() { SelectBase.Select(); SplitBase.Split(); }
    }
    #endregion


    #region インターフェース版
    ////これは大変、組み合わせの分だけクラスを書く必要がある
    //public interface IZZZ
    //{
    //    void Select();
    //    void Split();
    //}
    //public class AAAI : IZZZ
    //{
    //    public void Select() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    //    void IZZZ.Split() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    //}
    //public class BBBI : IZZZ
    //{
    //    public void Split() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    //    void IZZZ.Select() => MessageBox.Show($"{ToString()}の{MethodBase.GetCurrentMethod()}");
    //}
    //public class CCCI
    //{
    //    private IZZZ ZZZ;
    //    public CCCI(IZZZ iz) => ZZZ = iz;
    //    public void Execute()
    //    {
    //        ZZZ.Select();
    //        ZZZ.Split();
    //    }
    //}
    #endregion
}
