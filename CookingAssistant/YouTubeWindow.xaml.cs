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
    public partial class YouTubeWindow : Window
    {
        private CookingAssistantDBEntities db = new CookingAssistantDBEntities();
        public YouTubeWindow()
        {
            InitializeComponent();
        }
        public void ReceiveVideos(List<YouTubeUtils.Video> videos)
        {
            displayVideoList.ItemsSource = videos;
        }

        private void OpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            var url = "https://www.youtube.com/watch?v=" + ((Button)sender).Tag.ToString();
            System.Diagnostics.Process.Start(url);
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Recipe currentlyChosenRecipe = ((MainWindow)this.Owner).currentlyChosenRecipe;
            string videoId = ((Button)sender).Tag.ToString();
            Recipe record = (from r in db.Recipes.Include("FavouriteVideo")
                             where currentlyChosenRecipe.recipeId == r.recipeId
                             select r).FirstOrDefault();
            record.FavouriteVideo = new FavouriteVideo() { youtubeId = videoId };
            db.SaveChanges();
        }
    }
}