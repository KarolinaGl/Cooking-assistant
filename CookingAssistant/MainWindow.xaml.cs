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
        private int selectedRecipeId;
        YouTubeHandle youTubeHandle;
        Dictionary<string, List<YouTubeUtils.Video>> cachedSearchResults;
        List<YouTubeUtils.Video> cachedSavedVideos;
        YouTubeWindow currentYouTubeWindow;
        TimerWindow currentTimerWindow;
        RecipesWindow currentRecipesWindow;
        public MainWindow()
        {
            InitializeComponent();
            youTubeHandle = new YouTubeHandle("AIzaSyDvi23J4hoKVVtjVC - 1XzW - s_PPjHGe_cA");
            cachedSearchResults = new Dictionary<string, List<YouTubeUtils.Video>>();
            cachedSavedVideos = new List<YouTubeUtils.Video>();
            //cachedSavedVideos = youTubeHandle.GetVideosFromIds() pozyskanie ulubionych na podstawie listy id z bazy danych
        }

        private async void RecommendedVideosButton_Click(object sender, RoutedEventArgs e)
        {
            string recipeName = "spaghetti";
            List<YouTubeUtils.Video> videos;
            if (this.cachedSearchResults.ContainsKey(recipeName))
            {
                videos = this.cachedSearchResults[recipeName];
            }
            else
            {
                var searchResult = await youTubeHandle.SearchVideos(recipeName, 50);
                this.cachedSearchResults[recipeName] = searchResult;
                videos = searchResult;
            }
            if (this.currentYouTubeWindow != null)
            {
                currentYouTubeWindow.Close();
            }
            this.currentYouTubeWindow = new YouTubeWindow();
            this.currentYouTubeWindow.ReceiveVideos(videos);
            this.currentYouTubeWindow.Show();
        }
        
        private async void SavedVideosButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> videoIds = new List<string> { "qcXRx-Umcsg", "Yc5SGUIAhtM", "sWblpsLZ-O8" };
            var videos = await this.youTubeHandle.GetVideosFromIds(videoIds);
            this.cachedSavedVideos = videos;
            if (this.currentYouTubeWindow != null)
            {
                currentYouTubeWindow.Close();
            }
            this.currentYouTubeWindow = new YouTubeWindow();
            this.currentYouTubeWindow.ReceiveVideos(videos);
            this.currentYouTubeWindow.Show();
        }

        private void TimerButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentTimerWindow != null)
            {
                this.currentTimerWindow.Close();
            }
            this.currentTimerWindow = new TimerWindow();
            this.currentTimerWindow.Show();
        }

        public void EmbedRecipesWindow(int selectedRecipeId)
        {
            if (this.currentRecipesWindow != null)
            {
                this.currentRecipesWindow.Close();
            }
            currentRecipesWindow = new RecipesWindow(selectedRecipeId);
            recipesFrame.Content = this.currentRecipesWindow.Content;

        }

        private void RecipeSelectIdDummy_Click(object sender, RoutedEventArgs e)
        {
            this.selectedRecipeId = 1;
            EmbedRecipesWindow(selectedRecipeId);
        }
    }
}
