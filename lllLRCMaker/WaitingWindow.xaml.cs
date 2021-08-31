using System;
using System.Windows;

namespace lllLRCMaker
{
    /// <summary>
    /// WaitingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WaitingWindow : Window
    {
        public WaitingWindow()
        {
            InitializeComponent();
        }
        public void setText(String text)
        {
            Text.Text = text;
        }
    }
}
