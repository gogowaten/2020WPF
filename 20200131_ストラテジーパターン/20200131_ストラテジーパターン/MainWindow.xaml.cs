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

            //抽象クラス版
            var neko = new CCC(new BBB());
            neko.Execute();
            neko = new CCC(new AAA());
            neko.Execute();

            //インターフェース版
            var uma = new CCCI(new AAAI());
            uma.Execute();
            uma = new CCCI(new BBBI());
            uma.Execute();

            var inu = new LLL(new Selecter1(), new Splitter1());
            inu.Execute();
            inu = new LLL(new Selecter2(), new Splitter1());
            inu.Execute();

        }
    }


    public interface ISelect
    {
        void Select();
    }
    public interface ISplit
    {
        void Split();
    }
    public class Selecter1 : ISelect
    {
        public void Select() => MessageBox.Show(nameof(Selecter1));
    }
    public class Selecter2 : ISelect
    {
        public void Select() => MessageBox.Show(nameof(Selecter2));
    }
    public class Splitter1 : ISplit
    {
        public void Split() => MessageBox.Show(nameof(Splitter1));
    }
    public class Splitter2 : ISplit
    {
        public void Split() => MessageBox.Show(nameof(Splitter2));
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





    #region 抽象クラス版
    public abstract class ZZZ
    {
        public abstract void Select();
    }
    public class AAA : ZZZ
    {
        public override void Select() => MessageBox.Show("AAA");
    }
    public class BBB : ZZZ
    {
        public override void Select() => MessageBox.Show("BBB");
    }
    public class CCC
    {
        private ZZZ Z;
        public CCC(ZZZ z) => this.Z = z;
        public void Execute() => Z.Select();
    }
    #endregion


    #region インターフェース版
    public interface IZZZ
    {
        void Select();
        void Split();
    }
    public class AAAI : IZZZ
    {
        public void Select() => MessageBox.Show("AAA");
        void IZZZ.Split() => MessageBox.Show("AAA");
    }
    public class BBBI : IZZZ
    {
        public void Split() => MessageBox.Show("BBB");
        void IZZZ.Select() => MessageBox.Show("BBB");
    }
    public class CCCI
    {
        private IZZZ ZZZ;
        public CCCI(IZZZ iz) => ZZZ = iz;
        public void Execute() => ZZZ.Select();
    }
    #endregion
}
