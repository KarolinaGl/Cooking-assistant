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

namespace CookingAssistant
{
    /// <summary>
    /// Interaction logic for RecipePrepareWindow.xaml
    /// </summary>
    public partial class RecipePrepareWindow : Window
    {
        private CookingAssistantDBEntities db = new CookingAssistantDBEntities();
        List<ShoppingList> currentlyMissingItems;
        Recipe currentlyChosenRecipe;
        public RecipePrepareWindow()
        {
            InitializeComponent();
        }
        public RecipePrepareWindow(Recipe currentlyChosenRecipe)
        {
            InitializeComponent();
            this.currentlyChosenRecipe = currentlyChosenRecipe;
            EvaluateMissingItems();
        }
        public void EvaluateMissingItems()
        {
            var recipe = this.currentlyChosenRecipe;
            var supplies = db.Supplies.ToList();
            var missingItems = new List<ShoppingList>();

            foreach (RecipeIngredient recipeIngredient in recipe.RecipeIngredients.ToList())
            {
                var relatedSupply = supplies.Find((Supply supply) => supply.ingredientId == recipeIngredient.ingredientId);
                double missingQuantity = 0;
                if (relatedSupply != null)
                {
                    if (relatedSupply.MeasurementUnit.type == recipeIngredient.MeasurementUnit.type)
                    {
                        double balance = relatedSupply.measurementQuantity * relatedSupply.MeasurementUnit.defaultUnit.Value - recipeIngredient.measurementQuantity * recipeIngredient.MeasurementUnit.defaultUnit.Value;
                        if (balance < 0)
                        {
                            missingQuantity = Math.Abs(balance);
                        }
                        missingQuantity = missingQuantity / relatedSupply.MeasurementUnit.defaultUnit.Value;
                        missingQuantity = Math.Round(missingQuantity, 2);
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
            if (this.currentlyMissingItems.Count > 0)
            {
                missingItemsComment.Text = "You're not able to prepare this meal, because you're missing some of the needed ingredients.";
                missingItemsComment.Visibility = Visibility.Visible;
                missingItemsDataGrid.Visibility = Visibility.Visible;
                addToShoppingListButton.IsEnabled = true;
                prepareRecipeButton.IsEnabled = false;
                var missingItemsToDisplay = from item in this.currentlyMissingItems
                                            select new
                                            {
                                                item.measurementQuantity,
                                                item.MeasurementUnit.measurementDescription,
                                                item.Ingredient.ingredientName
                                            };
                missingItemsDataGrid.ItemsSource = missingItemsToDisplay;
            }
            else
            {
                missingItemsDataGrid.Visibility = Visibility.Collapsed;
                missingItemsComment.Visibility = Visibility.Collapsed;
                prepareRecipeButton.IsEnabled = true;
                addToShoppingListButton.IsEnabled = false;
            }
        }

        public void AddMissingItemsToShoppingList()
        {
            if (this.currentlyMissingItems.Count() > 0)
            {
                foreach (var missingItem in this.currentlyMissingItems)
                {
                    var relatedRecord = (from shoppingList in db.ShoppingLists
                                         where shoppingList.ingredientId == missingItem.ingredientId
                                         select shoppingList).FirstOrDefault();
                    if (relatedRecord != null)
                    {
                        if (relatedRecord.MeasurementUnit != null)
                        {
                            if (relatedRecord.MeasurementUnit.type == missingItem.MeasurementUnit.type)
                            {
                                relatedRecord.measurementQuantity += (missingItem.measurementQuantity * missingItem.MeasurementUnit.defaultUnit.Value) / relatedRecord.MeasurementUnit.defaultUnit.Value;
                                relatedRecord.measurementQuantity = Math.Round(relatedRecord.measurementQuantity, 2);
                            }
                        }
                    }
                    else
                    {
                        ShoppingList item = new ShoppingList
                        {
                            ingredientId = missingItem.ingredientId,
                            measurementId = missingItem.measurementId,
                            measurementQuantity = missingItem.measurementQuantity
                        };
                        db.ShoppingLists.Add(item);
                    }
                    db.SaveChanges();
                }
            }
        }

        public void PrepareRecipeAndUpdateSupplies()
        {
            var usedIngredients = this.currentlyChosenRecipe.RecipeIngredients;
            foreach (var ingredient in usedIngredients)
            {
                var relatedSupply = db.Supplies.ToList().Find((Supply supply) => supply.ingredientId == ingredient.ingredientId);
                if (relatedSupply != null)
                {
                    if (relatedSupply.MeasurementUnit.type == ingredient.MeasurementUnit.type)
                    {
                        relatedSupply.measurementQuantity -= (ingredient.measurementQuantity * ingredient.MeasurementUnit.defaultUnit.Value) / relatedSupply.MeasurementUnit.defaultUnit.Value;
                        relatedSupply.measurementQuantity = Math.Round(relatedSupply.measurementQuantity, 2);
                        if (relatedSupply.measurementQuantity <= 0)
                        {
                            db.Supplies.Remove(relatedSupply);
                            db.SaveChanges();
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        private void AddToShoppingListButton_Click(object sender, RoutedEventArgs e)
        {
            AddMissingItemsToShoppingList();
            (this.Owner as MainWindow).BindShoppingList();
            EvaluateMissingItems();
        }

        private void PrepareRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareRecipeAndUpdateSupplies();
            (this.Owner as MainWindow).BindSupplies();
            EvaluateMissingItems();
        }
    }
}
