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
        public RecipePrepareWindow(Recipe currentlyChosenRecipe)
        {
            InitializeComponent();
            this.currentlyChosenRecipe = currentlyChosenRecipe;
            EvaluateMissingItems();
        }

        /// <summary>
        /// Evaluates which items (ingredients) are missing and in what quantity. The evaluation compares the list of ingredients of currently chosen recipe with the list of supplies. If the items types are mismatched the items are ignored in the evaluation. 
        /// </summary>
        public void EvaluateMissingItems()
        {
            var recipe = this.currentlyChosenRecipe;
            var supplies = db.Supplies.ToList();
            var missingItems = new List<ShoppingList>();
            var mismatchedItems = new List<string>();

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
                    else
                    {
                        mismatchedItems.Add(recipeIngredient.Ingredient.ingredientName);
                    }
                }
                else
                {
                    missingQuantity = Math.Round(recipeIngredient.measurementQuantity, 2);
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
            if (mismatchedItems.Count > 0)
            {
                mismatchedItemsList.ItemsSource = mismatchedItems;
                mismatchedItemsList.Visibility = Visibility.Visible;
                mismatchedItemsComment.Visibility = Visibility.Visible;
            }
            else
            {
                mismatchedItemsList.Visibility = Visibility.Collapsed;
                mismatchedItemsComment.Visibility = Visibility.Collapsed;
            }
            if (this.currentlyMissingItems.Count > 0)
            {
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

        /// <summary>
        /// Adds evaluated missing items to the shopping list, changing quantity of present ones and creating new records if the ingredient were not in the shopping list.
        /// </summary>
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
                        var relatedRecordMeasurementUnit = (from measurementUnit in db.MeasurementUnits
                                                            where measurementUnit.measurementId == missingItem.measurementId
                                                            select measurementUnit).FirstOrDefault();
                        relatedRecord.measurementQuantity += (missingItem.measurementQuantity * missingItem.MeasurementUnit.defaultUnit.Value) / relatedRecordMeasurementUnit.defaultUnit.Value;
                        relatedRecord.measurementQuantity = Math.Round(relatedRecord.measurementQuantity, 2);
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
                        db.SaveChanges();
                    }
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Subtracts quantity of ingredients used in currently chosen recipe from quantity of this ingredient in supplies.
        /// </summary>
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

        /// <summary>
        /// Executes function which adds previously evaluated items to shopping list and binds the list in Main Window to refresh the data in datagrid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddToShoppingListButton_Click(object sender, RoutedEventArgs e)
        {
            AddMissingItemsToShoppingList();
            (this.Owner as MainWindow).BindShoppingList();
        }

        /// <summary>
        /// Executes function which subtracts needed ingredients from supplies, then refreshes the data in datagrid inside Main Window and evaluates missing items again to check if the recipe can prepared again. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PrepareRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareRecipeAndUpdateSupplies();
            (this.Owner as MainWindow).BindSupplies();
            EvaluateMissingItems();
        }
    }
}
