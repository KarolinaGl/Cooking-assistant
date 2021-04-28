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
        private CookingAssistantDBEntities db = new CookingAssistantDBEntities();
        public YouTubeWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Binds favourite video data received using YouTube Data Api to the related section in YouTubeWindow.
        /// </summary>
        public async void UpdateFavouriteBinding()
        {
            var youTubeHandle = (this.Owner as MainWindow).youTubeHandle;
            var currentlyChosenRecipe = (this.Owner as MainWindow).currentlyChosenRecipe;
            var favouriteIds = (from r in db.Recipes
                                where r.recipeId == currentlyChosenRecipe.recipeId
                                select r.FavouriteVideo.youtubeId).ToList();
            var favouriteVideos = new List<YouTubeUtils.Video>();
            try
            {
                favouriteVideos = await youTubeHandle.GetVideosFromIds(favouriteIds);
                favouriteVideoPanel.Visibility = Visibility.Visible;
                favouriteVideoList.ItemsSource = favouriteVideos;
            }
            catch
            {
                favouriteVideoPanel.Visibility = Visibility.Collapsed;
                if (!favouriteIds.Contains(null))
                {
                    MessageBox.Show("Failed to retrieve the pinned video");
                }
            }
        }

        /// <summary>
        /// Binds videos received from YouTube Data Api to section related to recommended videos in YouTubeWindow.
        /// </summary>
        public async void UpdateRecommendedBinding()
        {
            var youTubeHandle = (this.Owner as MainWindow).youTubeHandle;
            var currentlyChosenRecipe = (this.Owner as MainWindow).currentlyChosenRecipe;
            var recommendedVideos = new List<YouTubeUtils.Video>();
            try
            {
                recommendedVideos = await youTubeHandle.SearchVideos(currentlyChosenRecipe.recipeName + " recipe", 50);
                displayVideoList.ItemsSource = recommendedVideos;
            }
            catch
            {
                MessageBox.Show("Failed to get recommended videos");
            }
        }

        /// <summary>
        /// Opens the video related to the button in default system browser.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            var url = "https://www.youtube.com/watch?v=" + (sender as Button).Tag.ToString();
            System.Diagnostics.Process.Start(url);
        }


        /// <summary>
        /// Saves the video related to the button to the database as the favourite video and refreshes the binding.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Recipe currentlyChosenRecipe = (this.Owner as MainWindow).currentlyChosenRecipe;
            string videoId = (sender as Button).Tag.ToString();
            Recipe record = (from r in db.Recipes
                             where currentlyChosenRecipe.recipeId == r.recipeId
                             select r).FirstOrDefault();
            var favouriteVideo = new FavouriteVideo() { youtubeId = videoId };
            db.FavouriteVideos.Add(favouriteVideo);
            db.SaveChanges();
            record.favouriteVideoId = favouriteVideo.favouriteVideoId;
            db.SaveChanges();
            UpdateFavouriteBinding();
        }

        /// <summary>
        /// Removes the favourite video from the database and refreshes the binding.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnpinButton_Click(object sender, RoutedEventArgs e)
        {
            var currentlyChosenRecipe = (this.Owner as MainWindow).currentlyChosenRecipe;
            Recipe record = (from r in db.Recipes
                             where currentlyChosenRecipe.recipeId == r.recipeId
                             select r).FirstOrDefault();
            db.FavouriteVideos.Remove(record.FavouriteVideo);
            record.favouriteVideoId = null;
            db.SaveChanges();
            UpdateFavouriteBinding();
        }
    }
}