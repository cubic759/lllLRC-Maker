using Microsoft.Win32;
using System;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// AddSongs.xaml 的交互逻辑
    /// </summary>
    public partial class AddSongs : Window
    {
        private String url = "";
        private String fileName = "";
        private bool hasFile = false;
        public AddSongs()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //加载本地文件
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "音频文件|*.mp3;*.wav;*.flac| 所有文件(*.*) | *.*";
            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                fileName = ofd.FileName;
                hasFile = true;
                FileName.Content = fileName;
                selectFileButton.Visibility = Visibility.Hidden;
                fileInfo.Visibility = Visibility.Visible;
            }
        }
        private void linkButton(object sender, RoutedEventArgs e)
        {
            if (!hasFile)//非本地
            {
                url = link.Text;
                if (url == "")
                {
                    DialogResult = false;
                }
                else if (!url.Contains("https://music.163.com/#/song?id="))
                {
                    MessageBox.Show("请输入有效链接");
                }
                else
                {
                    DialogResult = true;
                    Close();
                }
            }
            else
            {
                if (url != "")
                {
                    MessageBox.Show("两个输入都有数据。请清除任意一个文件");
                }
                else
                {
                    url = fileName;
                    DialogResult = true;
                    Close();
                }
            }
        }
        public String getUrl()
        {
            return url;
        }
        public bool HasFile()
        {
            return hasFile;
        }

        private void remove_Click(object sender, RoutedEventArgs e)
        {
            selectFileButton.Visibility = Visibility.Visible;
            fileInfo.Visibility = Visibility.Hidden;
            hasFile = false;
        }
    }
}
