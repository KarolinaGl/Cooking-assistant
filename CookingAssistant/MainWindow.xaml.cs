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
    class RecipeGridCell
    {
        public string RecipeName { get; set; }
        Recipe recipe { get; set; }
    }
    public partial class MainWindow : Window
    {
        public Recipe currentlyChosenRecipe;
        private CookingAssistantDBEntities db = new CookingAssistantDBEntities();
        YouTubeHandle youTubeHandle;
        Dictionary<string, List<YouTubeUtils.Video>> cachedSearchResults = new Dictionary<string, List<YouTubeUtils.Video>>();
        YouTubeWindow currentYouTubeWindow;
        TimerWindow currentTimerWindow;
        RecipesWindow currentRecipesWindow;
        public MainWindow()
        {
            InitializeComponent();
            youTubeHandle = new YouTubeHandle("AIzaSyDvi23J4hoKVVtjVC - 1XzW - s_PPjHGe_cA");
            /*
            var recipes = (from r in db.Recipes
                           select new RecipeGridCell()
                           {
                               RecipeName = r.recipeName
                           }).ToArray();
            */
            var recipes = db.Recipes.Select((Recipe r) => new RecipeGridCell() { RecipeName = r.recipeName }).ToArray() ;
            recipesGrid.ItemsSource = recipes;
        }
        /*
        public void LoadRecipes()
        {
            var recipes = db.Recipes.ToList();
            recipesGrid.ItemsSource = recipes;
        }
        */
        private async void RecommendedVideosButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentlyChosenRecipe != null)
            {
                string recipeName = this.currentlyChosenRecipe.recipeName;
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
                this.currentYouTubeWindow.Owner = this;
                this.currentYouTubeWindow.ReceiveVideos(videos);
                rightFrame.Content = this.currentYouTubeWindow.Content;
            }
        } 
        private async void SavedVideosButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentlyChosenRecipe != null)
            {
                var records = (from r in db.Recipes.Include("FavouriteVideo")
                            where this.currentlyChosenRecipe.recipeName == r.recipeName
                            select r.FavouriteVideo).ToList();
                var videoIds = new List<string>();
                foreach (var record in records)
                {
                    if (record != null)
                    {
                        videoIds.Add(record.youtubeId);
                    }
                }
                if (videoIds.Count() > 0)
                {
                    var videos = await this.youTubeHandle.GetVideosFromIds(videoIds);
                    if (this.currentYouTubeWindow != null)
                    {
                        currentYouTubeWindow.Close();
                    }
                    this.currentYouTubeWindow = new YouTubeWindow();
                    this.currentYouTubeWindow.Owner = this;
                    this.currentYouTubeWindow.ReceiveVideos(videos);
                    rightFrame.Content = this.currentYouTubeWindow.Content;
                } 
            }
        }
        private void TimerButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentTimerWindow != null)
            {
                this.currentTimerWindow.Close();
            }
            this.currentTimerWindow = new TimerWindow();
            this.currentTimerWindow.Owner = this;
            this.currentTimerWindow.Show();
        }
        public void EmbedRecipesWindow(int chosenRecipeId)
        {
            if (this.currentRecipesWindow != null)
            {
                this.currentRecipesWindow.Close();
            }
            this.currentRecipesWindow = new RecipesWindow(chosenRecipeId);
            this.currentRecipesWindow.Owner = this;
            recipesFrame.Content = this.currentRecipesWindow.Content;
        }
        private void RemoveFavouriteHandle(object sender, RoutedEventArgs e)
        {
            
        }

        private void recipesGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (recipesGrid.CurrentCell.IsValid)
            {
                RecipeGridCell recipeGridCell = recipesGrid.CurrentCell.Item as RecipeGridCell;
                if (recipeGridCell != null)
                {
                    string recipeName = recipeGridCell.RecipeName;
                    var recipe = from r in db.Recipes where r.recipeName == recipeName select r;
                    this.currentlyChosenRecipe = recipe.SingleOrDefault();
                    if (this.currentlyChosenRecipe != null)
                    {
                        if (this.currentYouTubeWindow != null)
                        {
                            this.currentYouTubeWindow.Close();
                        }
                        EmbedRecipesWindow(this.currentlyChosenRecipe.recipeId);
                    }
                }
            }
        }
    }
}
