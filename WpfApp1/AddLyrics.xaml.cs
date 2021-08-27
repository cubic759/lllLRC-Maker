using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    /// <summary>
    /// AddLyrics.xaml 的交互逻辑
    /// </summary>
    public partial class AddLyrics : Window
    {
        private String originalLyric = "";
        private String translatedLyric = "";
        private bool shouldHaveTranslate = false;
        public AddLyrics()
        {
            InitializeComponent();
        }

        #region 标题栏事件
        /// <summary>
        /// 窗口移动事件
        /// </summary>
        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (isClickOnTitle && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private bool isClickOnTitle = false;
        /// <summary>
        /// 标题栏双击事件
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isClickOnTitle = true;
        }

        /// <summary>
        /// 标题栏松开事件
        /// </summary>
        private void DockPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isClickOnTitle = false;
        }
        #endregion 标题栏事件

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            originalLyric = originalText.Text;
            translatedLyric = TranslatedText.Text;
            if (originalLyric != "")
            {
                DialogResult = true;
            }
            else
            {
                DialogResult = false;
            }
            Close();
        }

        private void removeEmptyLines_Click(object sender, RoutedEventArgs e)
        {
            string[] stringArray = originalText.Text.Replace("\r\n", "\n").Split('\n');
            string result = "";
            for (int i = 0; i < stringArray.Length; i++)
            {
                if (stringArray[i] != "")
                {
                    result += stringArray[i];
                    if (i != stringArray.Length - 1)
                    {
                        result += "\r\n";
                    }
                }
            }
            originalText.Text = result;
            stringArray = null;
            result = "";
            stringArray = TranslatedText.Text.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < stringArray.Length; i++)
            {
                if (stringArray[i] != "")
                {
                    result += stringArray[i];
                    if (i != stringArray.Length - 1)
                    {
                        result += "\r\n";
                    }
                }
            }
            TranslatedText.Text = result;
        }

        public String getOriginalLyric()
        {
            return originalLyric;
        }

        public String getTranslatedLyric()
        {
            return translatedLyric;
        }

        public void SetOriginalLyric(String text)
        {
            originalText.Text = text;
        }

        public void setTranslatedLyric(String text)
        {
            TranslatedText.Text = text;
        }

        public void setHasTranslation(bool setting)
        {
            setTranslate.IsChecked = setting;
            shouldHaveTranslate = setting;
            TranslatedText.IsEnabled = setting;
        }

        public bool hasTranslatedLyric()
        {
            return shouldHaveTranslate;
        }

        private void setTranslate_Click(object sender, RoutedEventArgs e)
        {
            if (setTranslate.IsChecked == true)
            {
                TranslatedText.IsEnabled = true;
                shouldHaveTranslate = true;
            }
            else
            {
                TranslatedText.IsEnabled = false;
                shouldHaveTranslate = false;
            }
        }

        private void removeTags_Click(object sender, RoutedEventArgs e)
        {
            string[] stringArray = originalText.Text.Replace("\r\n", "\n").Split('\n');
            string result = "";
            Regex regex = new Regex(@"\[\d{2}:\d{2}.\d{1,3}\]");
            for (int i = 0; i < stringArray.Length; i++)
            {
                result += regex.Split(stringArray[i])[regex.Matches(stringArray[i]).Count].Trim();
                if (i != stringArray.Length - 1)
                {
                    result += "\r\n";
                }
            }
            originalText.Text = result;
            stringArray = null;
            stringArray = TranslatedText.Text.Replace("\r\n", "\n").Split('\n');
            result = "";
            for (int i = 0; i < stringArray.Length; i++)
            {
                result += regex.Split(stringArray[i])[regex.Matches(stringArray[i]).Count].Trim();
                if (i != stringArray.Length - 1)
                {
                    result += "\r\n";
                }
            }
            TranslatedText.Text = result;
        }

        private void compressTags_Click(object sender, RoutedEventArgs e)
        {
            string[] stringArray = originalText.Text.Replace("\r\n", "\n").Split('\n');
            Regex regex = new Regex(@"\[\d{2}:\d{2}.\d{1,3}\]");
            bool allHaveTags = true;
            for (int i = 0; i < stringArray.Length; i++)
            {
                if (!regex.IsMatch(stringArray[i]))
                {
                    allHaveTags = false;
                    break;
                }
            }
            if (allHaveTags)
            {
                int counter = 0;
                string result = "";
                Hashtable hashtable = new Hashtable();
                for (int i = 0; i < stringArray.Length; i++)
                {
                    int ceiling = regex.Matches(stringArray[i]).Count;
                    string lyric = regex.Split(stringArray[i])[ceiling].Trim();
                    if (!hashtable.Contains(lyric))
                    {
                        hashtable.Add(lyric, counter);
                        counter++;
                    }
                    else
                    {
                        int htIndex = (int)hashtable[lyric];
                        stringArray[htIndex] = SortingTags(regex.Matches(stringArray[htIndex]), regex.Matches(stringArray[i])) + " " + lyric;
                    }
                }
                for (int i = 0; i < counter; i++)
                {
                    result += stringArray[i];
                    if (i != stringArray.Length - 1)
                    {
                        result += "\r\n";
                    }
                }
                originalText.Text = result;
            }
            else
            {
                MessageBox.Show("标签还没有打完，应该打完再压缩标签");
            }
        }

        private string SortingTags(MatchCollection collection1, MatchCollection collection2)
        {
            int ceiling1 = collection1.Count;
            int ceiling2 = collection2.Count;
            double[] b = new double[ceiling1 + ceiling2];
            string result = "";
            for (int i = 0; i < ceiling1; i++)
            {
                b[i] = TimeSpan.ParseExact(collection1[i].ToString().Substring(1).Split(']')[0], @"mm\:ss\.ff", null).TotalMilliseconds;
            }
            for (int i = 0; i < ceiling2; i++)
            {
                b[i + ceiling1] = TimeSpan.ParseExact(collection2[i].ToString().Substring(1).Split(']')[0], @"mm\:ss\.ff", null).TotalMilliseconds;
            }
            b = MergeSort.Sort(b);
            for (int i = 0; i < b.Length; i++)
            {
                result += "[" + TimeSpan.FromMilliseconds(b[i]).ToString(@"mm\:ss\.ff") + "]";
            }
            return result;
        }

        private void decompressTags_Click(object sender, RoutedEventArgs e)
        {
            string[] stringArray = originalText.Text.Replace("\r\n", "\n").Split('\n');

        }

        private void synchronizeTags_Click(object sender, RoutedEventArgs e)
        {
            string[] stringArray = originalText.Text.Replace("\r\n", "\n").Split('\n');
            string[] stringArray1 = TranslatedText.Text.Replace("\r\n", "\n").Split('\n');
            string result = "";
            Regex regex = new Regex(@"\[\d{2}:\d{2}.\d{1,3}\]");
            for (int i = 0; i < stringArray1.Length; i++)
            {
                int ceiling = regex.Matches(stringArray[i]).Count;
                for (int j = 0; j < ceiling; j++)
                {
                    result += regex.Matches(stringArray[i])[j];
                    if (j == ceiling - 1)
                    {
                        result += " ";
                    }
                }
                result += stringArray1[i].Trim();
                if (i != stringArray1.Length - 1)
                {
                    result += "\r\n";
                }
            }
            TranslatedText.Text = result;
        }

        private void ScrollViewer_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            scroll1.ScrollToVerticalOffset(scroll.VerticalOffset);
        }

        private void ScrollViewer1_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            scroll.ScrollToVerticalOffset(scroll1.VerticalOffset);
        }

        private void scroll_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)// /|
            {
                scroll1.ScrollToVerticalOffset(scroll.VerticalOffset - 50);
            }
            else if (e.Delta < 0)// \|
            {
                scroll1.ScrollToVerticalOffset(scroll.VerticalOffset + 50);
            }
        }

        private void scroll1_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)// /|
            {
                scroll.ScrollToVerticalOffset(scroll1.VerticalOffset - 50);
            }
            else if (e.Delta < 0)// \|
            {
                scroll.ScrollToVerticalOffset(scroll1.VerticalOffset + 50);
            }
        }

    }
}
