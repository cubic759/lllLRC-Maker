using System;
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
            if (shouldHaveTranslate)
            {
                if (originalLyric != ""&& translatedLyric!="")
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
                if (originalLyric != "")
                {
                    DialogResult = true;
                }
                else
                {
                    DialogResult = false;
                }
            } 
            Close();
        }

        private void removeEmptyLines_Click(object sender, RoutedEventArgs e)
        {
            String text = originalText.Text;
            originalText.Text = text.Replace("\r\n\r\n", "\r\n");
            Console.WriteLine(text);
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
            translatedLyric=text;
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
    }
}
