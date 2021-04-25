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

        public async void UpdateFavouriteBinding()
        {
            var youTubeHandle = (this.Owner as MainWindow).youTubeHandle;
            var currentlyChosenRecipe = (this.Owner as MainWindow).currentlyChosenRecipe;
            var favouriteIds = from r in db.Recipes
                               where r.recipeId == currentlyChosenRecipe.recipeId
                               select r.FavouriteVideo.youtubeId;
            var favouriteVideos = new List<YouTubeUtils.Video>();
            try
            {
                favouriteVideos = await youTubeHandle.GetVideosFromIds(favouriteIds.ToList());
            }
            catch
            {
                favouriteVideoPanel.Visibility = Visibility.Collapsed;
            }
            finally
            {
                favouriteVideoPanel.Visibility = Visibility.Visible;
                favouriteVideoList.ItemsSource = favouriteVideos;
            }
        }

        public async void UpdateRecommendedBinding()
        {
            var youTubeHandle = (this.Owner as MainWindow).youTubeHandle;
            var currentlyChosenRecipe = (this.Owner as MainWindow).currentlyChosenRecipe;
            var recommendedVideos = await youTubeHandle.SearchVideos(currentlyChosenRecipe.recipeName + " recipe", 50);
            displayVideoList.ItemsSource = recommendedVideos;
        }

        private void OpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            var url = "https://www.youtube.com/watch?v=" + (sender as Button).Tag.ToString();
            System.Diagnostics.Process.Start(url);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Recipe currentlyChosenRecipe = (this.Owner as MainWindow).currentlyChosenRecipe;
            string videoId = (sender as Button).Tag.ToString();
            Recipe record = (from r in db.Recipes
                             where currentlyChosenRecipe.recipeId == r.recipeId
                             select r).FirstOrDefault();
            record.FavouriteVideo = new FavouriteVideo() { youtubeId = videoId };
            db.SaveChanges();
            UpdateFavouriteBinding();
        }

        private void UnpinButton_Click(object sender, RoutedEventArgs e)
        {
            var currentlyChosenRecipe = (this.Owner as MainWindow).currentlyChosenRecipe;
            Recipe record = (from r in db.Recipes
                             where currentlyChosenRecipe.recipeId == r.recipeId
                             select r).FirstOrDefault();
            record.FavouriteVideo = null;
            db.SaveChanges();
            UpdateFavouriteBinding();
        }
       
    }
}