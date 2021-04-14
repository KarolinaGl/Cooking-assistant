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
using System.Windows.Shapes;
using YouTubeLib;

namespace CookingAssistant
{
    /// <summary>
    /// Interaction logic for YouTubeWindow.xaml
    /// </summary>
    public partial class YouTubeWindow : Window
    {
        List<YouTubeUtils.Video> videos;
        List<string> favouritesToAdd;
        public YouTubeWindow()
        {
            InitializeComponent();
        }

        public void ReceiveVideos(List<YouTubeUtils.Video> videos)
        {
            this.videos = videos;
            displayVideoList.ItemsSource = this.videos;
        }

        private void OpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            var url = "https://www.youtube.com/watch?v=" + ((Button)sender).Tag.ToString();
            System.Diagnostics.Process.Start(url);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
        }

    }
}