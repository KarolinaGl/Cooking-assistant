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
        private CookingAssistantDBEntities db = new CookingAssistantDBEntities();
        public YouTubeHandle youTubeHandle;
        YouTubeWindow currentYouTubeWindow;
        RecipesWindow currentRecipesWindow;
        ShoppingListWindow currentShoppingListWindow;
        SupplyWindow currentSupplyWindow;
        RecipeCRUDWindow currentRecipeCRUDWindow;
        RecipePrepareWindow currentRecipePrepareWindow;
        public MainWindow()
        {
            InitializeComponent();
            BindRecipes();
            BindShoppingList();
            BindSupplies();
            youTubeHandle = new YouTubeHandle("AIzaSyDvi23J4hoKVVtjVC - 1XzW - s_PPjHGe_cA");
        }

        public void BindRecipes()
        {
            recipesDataGrid.ItemsSource = db.Recipes.Select((Recipe r) => new RecipeGridCell() { RecipeName = r.recipeName}).ToArray();
        }

        public void BindShoppingList()
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

        public void BindSupplies()
        {
            var supplies = from supply in db.Supplies
                           select new
                           {
                               supply.Ingredient.ingredientName,
                               supply.measurementQuantity,
                               supply.MeasurementUnit.measurementDescription
                           };
            suppliesDataGrid.ItemsSource = supplies.ToArray();
        }


        public void BindAll()
        {
            BindRecipes();
            BindShoppingList();
            BindSupplies();
        }

        private void YouTubeTabButton_Click(object sender, RoutedEventArgs e)
        { 
            if (currentlyChosenRecipe != null)
            {
                youTubeTabButton.IsEnabled = false;
                string recipeName = this.currentlyChosenRecipe.recipeName;
                if (this.currentYouTubeWindow != null)
                {
                    currentYouTubeWindow.Close();
                    rightFrame.Content = null;
                }
                this.currentYouTubeWindow = new YouTubeWindow
                {
                    Owner = this
                };
                this.currentYouTubeWindow.UpdateRecommendedBinding();
                this.currentYouTubeWindow.UpdateFavouriteBinding();
                rightFrame.Content = this.currentYouTubeWindow.Content;
            }
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
                    youTubeTabButton.IsEnabled = true;
                    startupComment.Visibility = Visibility.Collapsed;
                    if (this.currentlyChosenRecipe != null)
                    {
                        if (this.currentYouTubeWindow != null)
                        {
                            this.currentYouTubeWindow.Close();
                            rightFrame.Content = null;
                        }
                        if (this.currentRecipesWindow != null)
                        {
                            this.currentRecipesWindow.Close();
                            recipesFrame.Content = null;
                        }
                        if (this.currentRecipePrepareWindow != null)
                        {
                            this.currentRecipesWindow.Close();
                            recipePrepareFrame.Content = null;
                        }
                        this.currentRecipesWindow = new RecipesWindow(this.currentlyChosenRecipe.recipeId)
                        {
                            Owner = this
                        };
                        recipesFrame.Content = this.currentRecipesWindow.Content;
                        if (this.currentRecipePrepareWindow != null)
                        {
                            this.currentRecipePrepareWindow.Close();
                        }
                        this.currentRecipePrepareWindow = new RecipePrepareWindow(this.currentlyChosenRecipe)
                        {
                            Owner = this
                        };
                        recipePrepareFrame.Content = this.currentRecipePrepareWindow.Content;
                    }
                }
            }
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
            this.currentShoppingListWindow.ShowDialog();
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
            this.currentRecipeCRUDWindow.ShowDialog();
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
            this.currentSupplyWindow.ShowDialog();
        }
    }
    /// <summary>
    /// Auxiliary class used instead of anonymous objects to bind recipes to a DataGrid
    /// </summary>
    class RecipeGridCell
    {
        public string RecipeName { get; set; }
        public Recipe recipe { get; set; }
    }
}
