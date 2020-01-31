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

            var neko = new CCC(new BBB());
            neko.Select();

            var inu = new CCC(new AAA());
            inu.Select();

        }
    }


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
        public void Select() => Z.Select();
    }



    #region 失敗
    //public interface IZZZ
    //{
    //    void Select();
    //    void Split();
    //}
    //public class AAA : IZZZ
    //{
    //    public void Select() => MessageBox.Show("AAA");
    //    void IZZZ.Split() => MessageBox.Show("AAA");
    //}
    //public class BBB : IZZZ
    //{
    //    public void Split() => MessageBox.Show("BBB");
    //    void IZZZ.Select() => MessageBox.Show("BBB");
    //}
    //public class CCC
    //{
    //    private IZZZ ZZZ;

    //    public void Execute()
    //    {
    //        ZZZ.Select();
    //    }
    //}
    #endregion
}
