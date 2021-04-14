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
        YouTubeHandle yt;
        string searchTerm;
        List<string> favouritesToAdd;
        public YouTubeWindow()
        {
            InitializeComponent();
            yt = new YouTubeHandle("AIzaSyDvi23J4hoKVVtjVC-1XzW-s_PPjHGe_cA");
            favouritesToAdd = new List<string>();
            setDisplayVideoList();
        }
        public YouTubeWindow(string term)
        {
            InitializeComponent();
            yt = new YouTubeHandle("AIzaSyDvi23J4hoKVVtjVC-1XzW-s_PPjHGe_cA");
            searchTerm = term;
            setDisplayVideoList();
        }

        public async void setDisplayVideoList()
        {
            var videos = await yt.SearchVideos(this.searchTerm, 50);
            displayVideoList.ItemsSource = videos;
        }

        private void OpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            var url = "https://www.youtube.com/watch?v=" + ((Button)sender).Tag.ToString();
            System.Diagnostics.Process.Start(url);
        }

        private void Favourites_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}