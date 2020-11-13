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

namespace _20201112_dotNET5Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Core3のときと同じように、参照の追加無しでIntrinsicsが使える
            var v = System.Runtime.Intrinsics.Vector256.Create(5.0);
            var vv = System.Runtime.Intrinsics.X86.Avx.Add(v, v);
        }
    }
}
