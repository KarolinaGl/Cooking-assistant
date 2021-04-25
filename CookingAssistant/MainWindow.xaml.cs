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
using System.ComponentModel;
using YouTubeLib;


namespace CookingAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml. It's the main module of the application, acting as a hub for other windows and controls.
    /// </summary>
    public partial class MainWindow : Window
    {
        public Recipe currentlyChosenRecipe;
        public List<ShoppingList> currentlyMissingItems;
        private CookingAssistantDBEntities db = new CookingAssistantDBEntities();
        public YouTubeHandle youTubeHandle;
        Dictionary<string, List<YouTubeUtils.Video>> cachedSearchResults = new Dictionary<string, List<YouTubeUtils.Video>>();
        YouTubeWindow currentYouTubeWindow;
        TimerWindow currentTimerWindow;
        RecipesWindow currentRecipesWindow;
        ShoppingListWindow currentShoppingListWindow;
        SupplyWindow currentSupplyWindow;
        RecipeCRUDWindow currentRecipeCRUDWindow;
        public MainWindow()
        {
            InitializeComponent();
            DisplayRecipes();
            DisplayShoppingList();
            youTubeHandle = new YouTubeHandle("AIzaSyDvi23J4hoKVVtjVC - 1XzW - s_PPjHGe_cA");
        }

        public void DisplayRecipes()
        {
            recipesDataGrid.ItemsSource = db.Recipes.Select((Recipe r) => new RecipeGridCell() { RecipeName = r.recipeName }).ToArray();
        }

        public void DisplayShoppingList()
        {
            var shoppingLists = from shoppingList in db.ShoppingLists
                                select new
                                {
                                    shoppingList.measurementQuantity,
                                    shoppingList.MeasurementUnit.measurementDescription,
                                    shoppingList.Ingredient.ingredientName
                                };
            shoppingListDataGrid.ItemsSource = shoppingLists.ToArray();
        }

        public void DisplaySupplies()
        {
            var supplies = from supply in db.Supplies
                           select new
                           {
                               supply.Ingredient.ingredientName,
                               supply.measurementQuantity,
                               supply.MeasurementUnit

                           };
            suppliesDataGrid.ItemsSource = supplies.ToArray();
        }

        public void DisplayMissingItems()
        {
            var toDisplay = from item in this.currentlyMissingItems
                            select new
                            {
                                item.Ingredient.ingredientName,
                                item.measurementQuantity,
                                item.MeasurementUnit.measurementDescription
                            };
            missingItemsDataGrid.ItemsSource = toDisplay.ToList();
            AddMissingItemsToShoppingList(this.currentlyMissingItems);
        }

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
                    var searchResult = await youTubeHandle.SearchVideos(recipeName + " recipe", 50);
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

        private void recipesGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (recipesDataGrid.CurrentCell.IsValid)
            {
                RecipeGridCell recipeGridCell = recipesDataGrid.CurrentCell.Item as RecipeGridCell;
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
                            this.rightFrame.Content = null;
                        }
                        EmbedRecipesWindow(this.currentlyChosenRecipe.recipeId);
                        EvaluateMissingItems();
                        DisplayMissingItems();
                    }
                }
            }
        }

        public void EvaluateMissingItems()
        {
            var recipe = this.currentlyChosenRecipe;
            var supplies = db.Supplies.ToList();
            var missingItems = new List<ShoppingList>();

            foreach(RecipeIngredient recipeIngredient in recipe.RecipeIngredients.ToList())
            {
                var relatedSupply = supplies.Find((Supply supply) => supply.ingredientId == recipeIngredient.ingredientId);
                double missingQuantity = 0;
                if (relatedSupply != null)
                {
                    var balance = relatedSupply.measurementQuantity - recipeIngredient.measurementQuantity;
                    if (balance < 0)
                    {
                        missingQuantity = Math.Abs(balance);
                    }
                }
                else
                {
                    missingQuantity = recipeIngredient.measurementQuantity;
                }
                if (missingQuantity > 0)
                {
                    var missingItem = new ShoppingList()
                    {
                        Ingredient = recipeIngredient.Ingredient,
                        MeasurementUnit = recipeIngredient.MeasurementUnit,
                        measurementQuantity = missingQuantity,
                        ingredientId = recipeIngredient.ingredientId,
                        measurementId = recipeIngredient.measurementId
                    };
                    missingItems.Add(missingItem);
                }
            }

            this.currentlyMissingItems = missingItems;
        }

        public void AddMissingItemsToShoppingList(List<ShoppingList> missingItems)
        {
            if (missingItems.Count() > 0)
            {
                foreach (var missingItem in missingItems)
                {
                    var relatedRecord = (from shoppingList in db.ShoppingLists 
                                        where shoppingList.ingredientId == missingItem.ingredientId 
                                        select shoppingList).FirstOrDefault();
                    if (relatedRecord != null)
                    {
                        relatedRecord.measurementQuantity += missingItem.measurementQuantity;
                    }
                    else
                    {
                        db.ShoppingLists.Add(missingItem);
                    }
                }
                db.SaveChanges();
            }
        }

        public void FulfillRecipeAndUpdateSupplies()
        {
            var usedIngredients = this.currentlyChosenRecipe.RecipeIngredients;
            foreach (var ingredient in usedIngredients)
            {
                var relatedSupply = db.Supplies.Find(ingredient);
                if (relatedSupply != null)
                {
                    relatedSupply.measurementQuantity -= ingredient.measurementQuantity;
                }
            }
            db.SaveChanges();
        }

        private void ShoppingListButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentShoppingListWindow != null)
            {
                this.currentShoppingListWindow.Close();
            }
            this.currentShoppingListWindow = new ShoppingListWindow
            {
                Owner = this
            };
            this.currentShoppingListWindow.Show();
        }

        private void RecipeCRUDWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentRecipeCRUDWindow != null)
            {
                this.currentRecipeCRUDWindow.Close();
            }
            this.currentRecipeCRUDWindow = new RecipeCRUDWindow
            {
                Owner = this
            };
            this.currentRecipeCRUDWindow.Show();
        }

        private void recipesDataGrid_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            Console.WriteLine("test");
        }

        private void SuppliesButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentSupplyWindow != null)
            {
                this.currentSupplyWindow.Close();
            }
            this.currentSupplyWindow = new SupplyWindow
            {
                Owner = this
            };
            this.currentSupplyWindow.Show();
        }
    }
    /// <summary>
    /// Auxiliary class used instead of anonymous objects to bind recipes to a DataGrid
    /// </summary>
    class RecipeGridCell
    {
        public string RecipeName { get; set; }
        Recipe recipe { get; set; }
    }
}
