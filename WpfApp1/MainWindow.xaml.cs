using AngleSharp;
using AngleSharp.Dom;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.WaveFormRenderer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 变量
        private WaveOutEvent player;
        private AudioFileReader reader;
        private VolumeSampleProvider volumeProvider;
        private CancellationTokenSource cts;
        private bool sliderLock = false; // 逻辑锁，当为true时不更新界面上的进度
        private bool isInitiated = false;
        private double position = 0;
        private Label timerLabel = new Label();
        private string root = "D:\\lllLRC Maker";//根目录
        private Class1 timer = new Class1();
        private WaitingWindow waitingWindow = new WaitingWindow();
        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }
        #region 命令
        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)//执行条件
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)//执行之后做的事
        {
            MessageBox.Show("The New command was invoked");
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.IsActive)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (isInitiated)
            {
                if (player.PlaybackState != PlaybackState.Playing)
                {
                    PlayAction();
                }
                else
                {
                    PauseAction();
                }
            }
        }

        int itemAfter = 0;

        private void InsertTag_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.IsActive)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void InsertTag_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            itemAfter = LyricView.SelectedIndex;
            if (itemAfter != -1)
            {
                ICollectionView view = (ICollectionView)CollectionViewSource.GetDefaultView(LyricView.ItemsSource);
                List<LyricData> items = new List<LyricData>();
                items = (List<LyricData>)view.SourceCollection;
                LyricData a = items[itemAfter];
                a.Tag = (string)timerLabel.Content;
                items.RemoveAt(itemAfter);
                items.Insert(itemAfter, a);
                LyricView.Items.Refresh();
                LyricView.SelectedIndex++;
                LyricScroll.ScrollToVerticalOffset(LyricScroll.VerticalOffset);
            }
        }

        #endregion 命令
        #region 主要方法
        public void initialization(string fileName)
        {
            _ = Dispatcher.BeginInvoke((Action)delegate
            {
                waitingWindow.setText("正在初始化音频...");
            });
            position = 0;
            player = new WaveOutEvent(); // Create player
            reader = new AudioFileReader(fileName); // Create reader
            reader.CurrentTime = TimeSpan.Zero;
            // dsp start
            volumeProvider = new VolumeSampleProvider(reader)
            {
                Volume = 1
            };
            // dsp end
            player.Init(volumeProvider);
            UpdateProgress();
            var duration = reader.TotalTime; // 总时长
            cts = new CancellationTokenSource();
            player.PlaybackStopped += player_OnPlaybackStopped;
            _ = Dispatcher.BeginInvoke((Action)delegate
            {
                total.Content = duration.ToString(@"mm\:ss");
                for (int i = 0; i < preview.Items.Count; i++)
                {
                    if (((TextBlock)preview.Items.GetItemAt(i)).Text == "无音乐")
                    {
                        preview.Items.RemoveAt(i);
                        break;
                    }
                }
            });
            isInitiated = true;
            Task.Run(() => StartUpdateProgress());// 界面更新线程
        }

        private void StartUpdateProgress()
        {
            while (!cts.IsCancellationRequested)
            {
                if (player.PlaybackState == PlaybackState.Playing)
                {
                    if (timer.IsTimeout())
                    {
                        position += 10;
                        UpdateProgress();
                        timer.Start(10);
                    }
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }
        private void UpdateProgress()
        {
            if (!sliderLock)
            {
                var currentTime = reader.CurrentTime; // 当前时间
                var timeTag = TimeSpan.FromMilliseconds(position);
                _ = Dispatcher.BeginInvoke((Action)delegate
                {
                    line.StartPoint = new Point(img.Width * currentTime.TotalMilliseconds / reader.TotalTime.TotalMilliseconds - scroll.HorizontalOffset, 0);
                    line.EndPoint = new Point(img.Width * currentTime.TotalMilliseconds / reader.TotalTime.TotalMilliseconds - scroll.HorizontalOffset, -70);
                    path.Data = line;
                    timerLabel.Content = timeTag.ToString(@"mm\:ss\.ff");
                    current.Content = currentTime.ToString(@"mm\:ss");
                });
            }
        }

        private void mainView(string name, string title)
        {
            _ = Dispatcher.BeginInvoke((Action)delegate
            {
                waitingWindow.setText("正在加载界面...");
            });
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            //生成波形保存到目标目录
            var samplingPeakProvider = new SamplingPeakProvider(200); // e.g. 200
            var myRendererSettings = new StandardWaveFormRendererSettings();
            myRendererSettings.Width = 2400;
            myRendererSettings.TopHeight = 100;
            myRendererSettings.BottomHeight = 100;
            myRendererSettings.BackgroundColor = System.Drawing.Color.Transparent;
            myRendererSettings.TopPeakPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(37, 150, 190));
            myRendererSettings.BottomPeakPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(37, 150, 190));
            var renderer = new WaveFormRenderer();
            var image = renderer.Render(name, samplingPeakProvider, myRendererSettings);
            string titleName = title.Replace(".mp3", "") + ".png";
            var destination = Path.Combine(root, titleName);
            if (!File.Exists(destination))
            {
                image.Save(destination);
            }
            _ = Dispatcher.BeginInvoke((Action)delegate
            {
                if (addSongsWindow.HasFile())
                {
                    albumImg.Source = null;
                }
                img.Source = new BitmapImage(new Uri(destination, UriKind.RelativeOrAbsolute));
                waitingWindow.Hide();
            });
        }
        #endregion
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
            waitingWindow.Close();
        }

        #endregion 标题栏事件
        #region 音频控制
        private void GoToTheFront(object sender, RoutedEventArgs e)
        {
            if (isInitiated)
            {
                reader.CurrentTime = TimeSpan.Zero;
                position = 0;
                current.Content = reader.CurrentTime.ToString(@"mm\:ss");
                timerLabel.Content = TimeSpan.FromMilliseconds(position).ToString(@"mm\:ss\.ff");
            }
        }

        private void GoToTheBack(object sender, RoutedEventArgs e)
        {
            if (isInitiated)
            {
                reader.CurrentTime = reader.TotalTime;
                position = reader.TotalTime.TotalMilliseconds;
                current.Content = reader.CurrentTime.ToString(@"mm\:ss");
                timerLabel.Content = TimeSpan.FromMilliseconds(position).ToString(@"mm\:ss\.ff");
            }
        }

        private void PlayOrPause(object sender, RoutedEventArgs e)
        {
            if (isInitiated)
            {
                if (play.Tag.ToString() == "true")
                {
                    PlayAction();
                }
                else
                {
                    PauseAction();
                }
            }
        }
        private void PlayAction()
        {
            timer.Start(10);
            player.Play();
            if (reader.CurrentTime == reader.TotalTime)
            {
                reader.CurrentTime = TimeSpan.Zero;
                position = 0;
            }
            play.Content = "||";
            play.Tag = "false";
        }

        private void PauseAction()
        {
            player.Pause();
            timer.Stop();
            play.Content = ">";
            play.Tag = "true";
        }
        private void StopAction()
        {
            player.Stop();
            timer.Stop();
            position = 0;
            _ = Dispatcher.BeginInvoke((Action)delegate
            {
                play.Content = ">";
                play.Tag = "true";
            });
        }

        private void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (volumeProvider != null)
            {
                UpdateVolume();
            }
        }
        private void UpdateVolume()
        {
            var volume = volumeSlider.Value / 100;
            volumeProvider.Volume = (float)volume;
        }
        private void player_OnPlaybackStopped(object obj, StoppedEventArgs arg)
        {
            StopAction();
        }
        private void Disposeplayer()
        {
            if (player != null)
            {
                player.PlaybackStopped -= player_OnPlaybackStopped;
                player.Dispose();
            }
        }

        private void DisposeAll()
        {
            if (isInitiated)
            {
                cts.Cancel();
                cts.Dispose();
                reader.Dispose();
                Disposeplayer();
                isInitiated = false;
            }

        }
        #endregion
        private System.Windows.Point StartPoint;
        private LineGeometry line = new LineGeometry();
        #region 线位置显示注释代码
        /*private void img_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            WheelPoint = e.GetPosition(img);
            double value = WheelPoint.X / img.Width * 50;//50 处数值与图像宽度变化数值一致
            if (e.Delta < 0)// \| ->
            {
                if (img.Width > 800)
                {
                    img.Width -= 50;//与这里一致
                    scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset - value);
                }
            }
            else if (e.Delta > 0)// |\ <-
            {
                img.Width += 50;
                scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset + value);
            }
            if (img.Width == 800)
            {
                isEdited = false;
            }
            else
            {
                isEdited = true;
            }
        }
        
        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            if (isEdited)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    EndPoint = e.GetPosition(img);
                    double value = EndPoint.X - StartPoint.X;
                    if (value > 0)//->
                    {
                        scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset - value);
                    }
                    else if (value < 0)//<-
                    {
                        scroll.ScrollToHorizontalOffset(scroll.HorizontalOffset - value);
                    }
                }
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point = e.GetPosition(img);
                    line.StartPoint = new Point(Point.X - scroll.HorizontalOffset, 0);
                    line.EndPoint = new Point(Point.X - scroll.HorizontalOffset, -70);
                    path.Data = line;
                    // 拖动时可以直观看到目标进度
                    current.Content = TimeSpan.FromMilliseconds(Point.X / 800 * reader.TotalTime.TotalMilliseconds).ToString(@"mm\:ss");
                    timerLabel.Content = TimeSpan.FromMilliseconds(Point.X / 800 * reader.TotalTime.TotalMilliseconds).ToString(@"mm\:ss\.ff");
                }
                
                
                if (scroll.HorizontalOffset == 0 || img.Width - scroll.HorizontalOffset == 800)
                {
                    line.StartPoint = new Point(Point.X - scroll.HorizontalOffset, 0);
                    line.EndPoint = new Point(Point.X - scroll.HorizontalOffset, -70);
                }
                else
                {
                    line.StartPoint = new Point(Point.X - scroll.HorizontalOffset + EndPoint.X - StartPoint.X, 0);
                    line.EndPoint = new Point(Point.X - scroll.HorizontalOffset + EndPoint.X - StartPoint.X, -70);
                }
                path.Data = line;
                
                
            }
        }*/
        #endregion 卷帘窗废弃代码

        private void img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StartPoint = e.GetPosition(img);
            // 释放鼠标时，应用目标进度
            reader.CurrentTime = TimeSpan.FromMilliseconds(StartPoint.X / img.Width * reader.TotalTime.TotalMilliseconds);
            position = reader.CurrentTime.TotalMilliseconds;
            UpdateProgress();
        }

        double volBefore = 0;
        private void volume_Click(object sender, RoutedEventArgs e)
        {
            if (volumeSlider.Tag.ToString() == "Pass")
            {
                volume.Content = "音量/";
                volBefore = volumeSlider.Value;
                volumeSlider.Value = 0;
                volumeSlider.Tag = "Mute";
            }
            else
            {
                volume.Content = "音量";
                volumeSlider.Value = volBefore;
                volumeSlider.Tag = "Pass";
            }
        }
        AddSongs addSongsWindow;
        private void addSong_Click(object sender, RoutedEventArgs e)
        {
            addSongsWindow = new AddSongs();
            addSongsWindow.Owner = this;
            addSongsWindow.ShowDialog();
            if (addSongsWindow.DialogResult == true)
            {
                string fileName = addSongsWindow.getUrl();
                bool hasFile = addSongsWindow.HasFile();
                if (fileName != "" && fileName != null)
                {
                    if (isInitiated)
                    {
                        StopAction();
                    }
                    Task.Run(() => GetInformation());
                }
                else
                {
                    MessageBox.Show("无文件");
                }
            }
        }

        private async void GetInformation()
        {
            _ = Dispatcher.BeginInvoke((Action)delegate
              {
                  waitingWindow.setText("请稍候...");
                  waitingWindow.Owner = this;
                  waitingWindow.ShowDialog();
              });
            string fileName = addSongsWindow.getUrl();
            bool hasFile = addSongsWindow.HasFile();
            if (hasFile)
            {
                string title = System.IO.Path.GetFileName(fileName);
                _ = Dispatcher.BeginInvoke((Action)delegate
                {
                    txtTitle.Text = "lllLRC Maker - " + title;
                });
                try
                {
                    DisposeAll();
                    initialization(fileName);
                }
                catch (Exception ex)
                {
                    DisposeAll();
                    MessageBox.Show(ex.Message);
                }
                mainView(fileName, title);
            }
            else
            {
                //获取网易云音频
                _ = Dispatcher.BeginInvoke((Action)delegate
                {
                    waitingWindow.setText("正在下载文件...");
                });
                string originalURL = fileName;
                string id = originalURL.Split('=')[1];
                string address = @"https://music.163.com/song?id=" + id;
                var config = Configuration.Default.WithDefaultLoader();
                var context = BrowsingContext.New(config);

                IDocument document = await context.OpenAsync(address);
                string innerHtml = document.Body.InnerHtml;
                string temp = innerHtml.Substring(innerHtml.IndexOf("歌手："));
                temp = temp.Substring(16);
                temp = temp.Substring(0, temp.IndexOf(@""""));
                if (temp.Contains(" / "))
                {
                    temp = temp.Replace(" / ", ", ");
                }
                string singerName = temp;
                temp = innerHtml.Substring(innerHtml.IndexOf("f-ff2")).Remove(0, 7);
                string songName = temp.Substring(0, temp.IndexOf("<"));
                string title = singerName + " - " + songName;
                temp = innerHtml.Substring(innerHtml.IndexOf("所属专辑："));
                temp = temp.Substring(temp.IndexOf(">"));
                string albumName = temp.Substring(1, temp.IndexOf("<") - 1);
                temp = innerHtml.Substring(innerHtml.IndexOf("data-src")).Remove(0, 10);
                string imageUrl = temp.Substring(0, temp.IndexOf(@""""));
                _ = Dispatcher.BeginInvoke((Action)delegate
                {
                    txtTitle.Text = "lllLRC Maker - " + title + ".mp3";
                });
                string url = "https://music.163.com/song/media/outer/url?id=" + id + ".mp3";
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.AllowAutoRedirect = true;
                try
                {
                    HttpWebResponse myHttpWebResponse = (HttpWebResponse)myRequest.GetResponse();
                    string location = myHttpWebResponse.ResponseUri.AbsoluteUri;//获取url地址
                                                                                //新建目录
                    if (!Directory.Exists(root))
                    {
                        Directory.CreateDirectory(root);
                    }
                    var destination = Path.Combine(root, title + ".mp3");
                    WebClient dc = new WebClient();
                    dc.DownloadFile(new Uri(location), destination);//下载MP3文件到目标目录
                    if (!File.Exists(Path.Combine(root, albumName + ".jpg")))
                    {
                        dc.DownloadFile(new Uri(imageUrl), Path.Combine(root, albumName + ".jpg"));
                    }
                    dc.Dispose();
                    myHttpWebResponse.Close();
                    myRequest.Abort();
                    document.Close();
                    context.Dispose();
                    _ = Dispatcher.BeginInvoke((Action)delegate
                    {
                        albumImg.Source = new BitmapImage(new Uri(Path.Combine(root, albumName + ".jpg"), UriKind.RelativeOrAbsolute));
                    });
                    try
                    {
                        DisposeAll();
                        initialization(destination);
                    }
                    catch (Exception ex)
                    {
                        DisposeAll();
                        MessageBox.Show(ex.Message);
                    }
                    mainView(destination, title);
                }
                catch (System.Net.WebException)
                {
                    MessageBox.Show("无法解析链接，请重试");
                }
            }
        }
        string originalLyric;
        string translatedLyric;
        private void addLyric_Click(object sender, RoutedEventArgs e)
        {
            AddLyrics addLyrics = new AddLyrics();
            addLyrics.Owner = this;
            if (originalLyric != null && originalLyric != "")
            {
                addLyrics.SetOriginalLyric(originalLyric);
            }
            if (addLyrics.hasTranslatedLyric()&&translatedLyric!=null&&translatedLyric!="")
            {
                addLyrics.setTranslatedLyric(translatedLyric);
            }
            
            addLyrics.ShowDialog();
            if (addLyrics.DialogResult == true)
            {
                originalLyric = addLyrics.getOriginalLyric();
                translatedLyric = addLyrics.getTranslatedLyric();
            }
            else
            {
                originalLyric = addLyrics.getOriginalLyric();
            }
            if (originalLyric != "" && originalLyric != null)
            {
                string[] stringArray = originalLyric.Replace("\r\n", "\n").Split('\n');
                List<LyricData> items = new List<LyricData>();
                for (int i = 0; i < stringArray.Length; i++)
                {
                    items.Add(new LyricData() { Tag = "", Content = stringArray[i] });
                }
                LyricView.ItemsSource = items;
                for (int i = 0; i < preview.Items.Count; i++)
                {
                    if (((TextBlock)preview.Items.GetItemAt(i)).Text == "无歌词")
                    {
                        preview.Items.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        private Label a;
        private String s;
        private Point point;
        private void Label_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isLabelMouseDown)
            {
                a = (Label)sender;
                s = (string)a.Content;
                if (s != "" && s != null)
                {
                    this.Cursor = Cursors.SizeNS;
                }
            }
            else if (isLabelMouseDown)
            {
                Label b = (Label)sender;
                if (b == a)
                {
                    a = b;
                    s = (string)a.Content;
                    if (s != null && s != "")
                    {
                        double value = e.GetPosition(a).Y - point.Y;
                        TimeSpan duration = TimeSpan.ParseExact(s, @"mm\:ss\.ff", null);
                        Console.WriteLine(value);
                        if (value > 0)// \|
                        {
                            if (duration > TimeSpan.FromMilliseconds(50))
                            {
                                duration = duration - TimeSpan.FromMilliseconds(10);
                            }
                            else
                            {
                                duration = TimeSpan.Zero;
                            }
                        }
                        else if (value < 0)// |\
                        {
                            if (duration < reader.TotalTime - TimeSpan.FromMilliseconds(50))
                            {
                                duration = duration + TimeSpan.FromMilliseconds(10);
                            }
                            else
                            {
                                duration = reader.TotalTime;
                            }
                        }
                        a.Content = duration.ToString(@"mm\:ss\.ff");
                        point = e.GetPosition(a);
                    }
                }
            }
        }
        bool isLabelMouseDown;
        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isLabelMouseDown = true;
            point = e.GetPosition((Label)sender);
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isLabelMouseDown)
            {
                isLabelMouseDown = false;
                this.Cursor = Cursors.Arrow;
            }
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                this.Cursor = Cursors.Arrow;
            }
        }
    }
    public class LyricData
    {
        public string Tag { get; set; }
        public string Content { get; set; }
    }
}