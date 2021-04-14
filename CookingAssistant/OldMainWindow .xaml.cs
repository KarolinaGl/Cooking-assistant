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
using System.Windows.Navigation;
using System.Windows.Shapes;
using YouTubeLib;

namespace CookingAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string input;
        YouTubeHandle youTubeHandle;
        Dictionary<string, List<YouTubeUtils.Video>> cachedSearchResults;
        List<YouTubeUtils.Video> cachedFavouriteVideos;

        public MainWindow()
        {
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.input != null)
            {
                var ytWindow = new YouTubeWindow(input);
                ytWindow.Show();
                ytWindow.Owner = this;
            }
        }

        private void InputChanged(object sender, RoutedEventArgs e)
        {
            this.input = TextBoxInput.Text;
        }
    }
}
