using System;
using System.Text.RegularExpressions;
using System.Windows;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            originalLyric = originalText.Text;
            translatedLyric = TranslatedText.Text;
            if (originalLyric != "")
            {
                if (shouldHaveTranslate)
                {
                    if (translatedLyric != "")
                    {
                        DialogResult = true;
                    }
                    else
                    {
                        DialogResult = false;
                    }
                }
                else
                {
                    DialogResult = true;
                }
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
            translatedLyric = text;
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
            for (int i = 0; i < stringArray.Length; i++)
            {
                Regex regex = new Regex(@"\[\d{2}:\d{2}.\d{1,3}\]");
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
                Regex regex = new Regex(@"\[\d{2}:\d{2}.\d{1,3}\]");
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
            string result = "";
            for (int i = 0; i < stringArray.Length; i++)
            {
                Regex regex = new Regex(@"\[\d{2}:\d{2}.\d{1,3}\]");
                result += regex.Split(stringArray[i])[regex.Matches(stringArray[i]).Count].Trim();
                if (i != stringArray.Length - 1)
                {
                    result += "\r\n";
                }
            }
            originalText.Text = result;
        }

        private void decompressTags_Click(object sender, RoutedEventArgs e)
        {

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
            }else if (e.Delta < 0)// \|
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

        private void synchronizeTags_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
