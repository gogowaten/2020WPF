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

using System.Collections.ObjectModel;
//【C#入門】ListViewの使い方(項目の追加、ソートやスクロールの設定) | 侍エンジニア塾ブログ（Samurai Blog） - プログラミング入門者向けサイト
//https://www.sejuku.net/blog/57146
//YKSoftware - for WPF Developers
//http://yujiro15.net/YKSoftware/StandardControls_ItemsControl.html
//ListView using GridView.HeaderTemplate and GridViewColumn.CellTemplate properties : ListView « Windows Presentation Foundation « C# / CSharp Tutorial
//http://www.java2s.com/Tutorial/CSharp/0470__Windows-Presentation-Foundation/ListViewusingGridViewHeaderTemplateandGridViewColumnCellTemplateproperties.htm

    //Listboxとの違いはColumnがあるところ、データは横に並ぶ、レコード形式

namespace _20200321_ListView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //TestList testList = new TestList();
            //MyListView.DataContext = testList.Data;



            var colors = MakeColors(10);
            var data = new List<MyData>();
            foreach (var item in colors)
            {
                data.Add(new MyData() { Brush = new SolidColorBrush(item), Color = item, ColorCode = item.ToString(), Type = "type" });
            }
            var myDataContext = new MyDataList(data);
            MyListView.DataContext = myDataContext.MyDatas;

            
            MyListView2.DataContext = myDataContext.MyDatas;
            MyListView3.DataContext = myDataContext.MyDatas;

        }

        private List<Color> MakeColors(int count)
        {
            var r = new Random();
            var vs = new byte[count * 3];
            var colors = new List<Color>();
            r.NextBytes(vs);
            for (int i = 0; i < vs.Length; i+=3)
            {
                colors.Add(Color.FromRgb(vs[i], vs[i + 1], vs[i + 2]));
            }
            return colors;
        }
    }

    public class MyData
    {
        public string Type { get; set; }
        public SolidColorBrush Brush { get; set; }
        public string ColorCode { get; set; }
        public Color Color { get; set; }
    }
    public class MyDataList
    {
        public ObservableCollection<MyData> MyDatas { get; }
        public MyDataList(List<MyData> datas)
        {
            MyDatas = new ObservableCollection<MyData>();
            for (int i = 0; i < datas.Count; i++)
            {
                MyDatas.Add(datas[i]);
            }
        }
    }




    public class Test
    {
        public string Subj { get; set; }
        public int Points { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
    }

    public class TestList
    {
        // バインディングの指定先プロパティ
        public ObservableCollection<Test> Data { get; }

        // コンストラクタ(データ入力)
        public TestList()
        {
            Data = new ObservableCollection<Test> {
                new Test { Subj="国語", Points=90, Name="田中　一郎", ClassName="A" },
                new Test { Subj="数学", Points=50, Name="鈴木　二郎", ClassName="A" },
                new Test { Subj="英語", Points=90, Name="佐藤　三郎", ClassName="B" }
            };
        }
    }


}
