using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace _20201104_アプリの設定保存_文字列リスト
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppData MyAppData = new AppData();//設定データ用
        private string AppConfigFilePath;//設定ファイルのパス用

        public MainWindow()
        {
            InitializeComponent();

            MyAppData = new AppData();
            this.DataContext = MyAppData;
            //設定ファイルのパスを設定
            //アプリの実行ファイルがあるフォルダのパスを取得してファイル名を追加
            AppConfigFilePath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            AppConfigFilePath += "\\" + "AppConfig.xml";

        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveAppConfig();
        }
        //シリアライズして保存
        private void SaveAppConfig()
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(AppData));
            try
            {
                using (var stream = new System.IO.StreamWriter(AppConfigFilePath, false, new UTF8Encoding(false)))
                {
                    serializer.Serialize(stream, MyAppData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存できなかった\n{ex.Message}");
            }
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadAppConfig();
        }
        private void LoadAppConfig()
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(AppData));
            try
            {
                using (var stream = new System.IO.StreamReader(AppConfigFilePath, new UTF8Encoding(false)))
                {
                    MyAppData = (AppData)serializer.Deserialize(stream);
                }
                //読み込んだデータをアプリのDataContextに指定
                this.DataContext = MyAppData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"読み込みできなかった\n{ex.Message}");
            }
        }

        //リストにテキストボックスの文字列追加
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            MyAppData.StringList.Add(MyTextBox.Text);
            MyList.Items.Refresh();//リストの表示更新が必要
        }
    }



    /// <summary>
    /// アプリの設定のデータ用
    /// </summary>
    [Serializable]//これは付けなくても動いた
    public class AppData
    {
        public List<string> StringList { get; set; }
        public double WindowTop { get; set; }//ウィンドウのY座標用
        public double WindowLeft { get; set; }//X座標用
        public AppData()
        {
            StringList = new List<string>();
        }
    }
}
