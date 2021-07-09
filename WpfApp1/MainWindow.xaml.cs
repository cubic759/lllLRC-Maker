using AngleSharp;
using Microsoft.Win32;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.WaveFormRenderer;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        Timer timer = new Timer();
        //命令
        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)//执行条件
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)//执行之后做的事
        {
            MessageBox.Show("The New command was invoked");
        }

        static WaveOutEvent waveOut = new WaveOutEvent();
        public static AudioFileReader musicFile;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //加载本地文件
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "音频文件|*.mp3;*.wav;*.flac| 所有文件(*.*) | *.*";
            ofd.ShowDialog();
            String fileName = ofd.FileName;
            txtTitle.Text += " - " + System.IO.Path.GetFileName(fileName);
            if (fileName != "")
            {
                musicFile = new AudioFileReader(fileName);//加载音频文件
                waveOut.Init(musicFile);//初始化音频文件
                mainView(fileName);
            }
        }
        String root = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "lllLRC Maker");
        private async void linkButton(object sender, RoutedEventArgs e)
        {
            //获取网易云音频
            String originalURL = link.Text;
            if (originalURL != "")
            {
                String id = originalURL.Split('=')[1];
                String address = @"https://music.163.com/song?id=" + id;
                var config = Configuration.Default.WithDefaultLoader();
                var context = BrowsingContext.New(config);
                var document = await context.OpenAsync(address);
                String title = document.Title.Replace(" - 单曲 - 网易云音乐", "");
                txtTitle.Text += " - " + title;
                String url = "https://music.163.com/song/media/outer/url?id=" + id + ".mp3";
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.AllowAutoRedirect = true;
                try
                {
                    HttpWebResponse myHttpWebResponse = (HttpWebResponse)myRequest.GetResponse();
                    String location = myHttpWebResponse.ResponseUri.AbsoluteUri;//获取url地址
                    //新建目录
                    if (!Directory.Exists(root))
                    {
                        Directory.CreateDirectory(root);
                    }
                    var destination = Path.Combine(root, "music.mp3");

                    await new WebClient().DownloadFileTaskAsync(new Uri(location), destination);//下载MP3文件到目标目录
                    musicFile = new AudioFileReader(destination);//加载音频文件
                    waveOut.Init(musicFile);//初始化音频文件
                    mainView(destination);
                }
                catch (System.Net.WebException)
                {
                    MessageBox.Show("无法解析链接，请重试");
                }
            }
            else
            {
                MessageBox.Show("请输入链接");
            }
        }
        private void mainView(String name)
        {
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            //移除控件
            stack.Visibility = Visibility.Hidden;
            stack1.Visibility = Visibility.Visible;
            playStack.Visibility = Visibility.Visible;
            //生成波形保存到目标目录
            //TODO:更改波形样式
            //var maxPeakProvider = new MaxPeakProvider();
            var rmsPeakProvider = new RmsPeakProvider(200); // e.g. 200
            //var samplingPeakProvider = new SamplingPeakProvider(200); // e.g. 200
            //var averagePeakProvider = new AveragePeakProvider(4); // e.g. 4
            var myRendererSettings = new StandardWaveFormRendererSettings();
            myRendererSettings.Width = 640;
            myRendererSettings.TopHeight = 32;
            myRendererSettings.BottomHeight = 32;
            var renderer = new WaveFormRenderer();
            var image = renderer.Render(name, rmsPeakProvider, myRendererSettings);
            var destination = Path.Combine(root, "myfile.png");
            image.Save(destination, ImageFormat.Png);

            img.Source = new BitmapImage(new Uri(destination, UriKind.RelativeOrAbsolute));
            img.Height = 200;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            scrollViewer.ScrollToHorizontalOffset(100);
            scrollViewer.Height = 200;
            img1.Source = new BitmapImage(new Uri(destination, UriKind.RelativeOrAbsolute));

            //TODO:新建进度条

            var t = musicFile.TotalTime;
            var c = waveOut.GetPositionTimeSpan().ToString().Substring(3);
            String totalTime = "";
            if (t < TimeSpan.FromHours(1))
            {
                totalTime = t.ToString().Substring(3, 8);
            }
            else
            {
                //TODO:大于1h的文件
            }
            total.Content = totalTime;
            current.Content = c.Insert(5, ".00");
        }

        private void Timer_Timer(object sender, EventArgs e)
        {
            try
            {
                current.Dispatcher.Invoke(
                    new Action(
                        delegate
                        {
                            String currentTime = waveOut.GetPositionTimeSpan().ToString();//00:00:00.000000
                            String totalTime = musicFile.TotalTime.ToString().Substring(3, 8);
                            if (currentTime.Length > 8)//TODO:大于1h
                            {
                                currentTime = currentTime.Substring(3, 8);
                            }
                            else
                            {
                                currentTime = currentTime.Substring(3).Insert(5, ".00");
                            }
                            current.Content = currentTime;
                            if (currentTime == totalTime)
                            {
                                waveOut.Stop();
                                waveOut.Dispose();
                                timer.Stop();
                                play.Content = ">";
                                play.IsCancel = true;
                            }
                        }
                    )
                );
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
            }
        }

        #region 标题栏事件

        /// <summary>
        /// 窗口移动事件
        /// </summary>
        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        int i = 0;
        /// <summary>
        /// 标题栏双击事件
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            i += 1;
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; i = 0; };
            timer.IsEnabled = true;

            if (i % 2 == 0)
            {
                timer.IsEnabled = false;
                i = 0;
                this.WindowState = this.WindowState == WindowState.Maximized ?
                              WindowState.Normal : WindowState.Maximized;
            }
        }

        /// <summary>
        /// 窗口最小化
        /// </summary>
        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized; //设置窗口最小化
        }

        /// <summary>
        /// 窗口最大化与还原
        /// </summary>
        private void btn_max_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal; //设置窗口还原
            }
            else
            {
                this.WindowState = WindowState.Maximized; //设置窗口最大化
            }
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion 标题栏事件

        private void GoToTheFront(object sender, RoutedEventArgs e)
        {
            musicFile.CurrentTime = TimeSpan.Zero;
        }

        private void PlayOrPause(object sender, RoutedEventArgs e)
        {
            if (play.Tag.ToString()=="true")
            {
                if (musicFile.CurrentTime == musicFile.TotalTime)
                {
                    musicFile.CurrentTime = TimeSpan.Zero;
                }
                timer.Interval = 1;
                timer.Elapsed += Timer_Timer;
                timer.Start();
                waveOut.Play();
                play.Content = "||";
                play.Tag = "false";

            }
            else
            {
                waveOut.Pause();
                timer.Stop();
                play.Content = ">";
                play.Tag = "true";
            }


        }
        private void GoToTheBack(object sender, RoutedEventArgs e)
        {
            musicFile.CurrentTime = musicFile.TotalTime;
            //TODO:
        }

    }
}