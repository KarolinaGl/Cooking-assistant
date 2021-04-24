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
    /// Interaction logic for RecipeCRUDWindow.xaml
    /// </summary>
    public partial class RecipeCRUDWindow : Window
    {
        private CookingAssistantDBEntities db = new CookingAssistantDBEntities();
        private int selectedRecipeId = 0;
        private int selectedRecipeIngredientId = 0;
        private string selectedMeasurementDescription = "";

        public RecipeCRUDWindow()
        {
            InitializeComponent();
            // volumeRadioButton checked by default
            this.volumeRadioButton.IsChecked = true;

            UpdateIngredientsDataGrid();
            UpdateRecipesDataGrid();
        }

        /// <summary>
        /// Fills recipesDataGrid with recipes from the database 
        /// </summary>
        private void UpdateRecipesDataGrid()
        {
            var recipes = from recipe in db.Recipes
                          select recipe;

            this.recipesDataGrid.ItemsSource = recipes.ToList();
        }

        /// <summary>
        /// Fills ingredientsDataGrid with ingredients from the database 
        /// </summary>
        private void UpdateIngredientsDataGrid()
        {
            if (selectedRecipeId > 0)
            {
                this.ingredientsDataGrid.Visibility = Visibility.Visible;

                var ingredients = from ingredient in db.RecipeIngredients
                                  where ingredient.recipeId == selectedRecipeId
                                  select new
                                  {
                                      ingredient.recipeIngredientId,
                                      ingredient.ingredientId,
                                      ingredient.measurementQuantity,
                                      ingredient.MeasurementUnit.measurementDescription,
                                      ingredient.Ingredient.ingredientName
                                  };

                var recipes = from recipe in db.Recipes
                              where recipe.recipeId == selectedRecipeId
                              select recipe;

                Recipe recipeObject = recipes.SingleOrDefault();

                if (recipeObject != null)
                {
                    this.recipeNameLabel.Content = recipeObject.recipeName;
                    this.preparationTimeLabel.Content = "Preparation time: " + recipeObject.preparationTime + " minutes";
                    this.descriptionTextBlock.Text = recipeObject.description;
                    this.ingredientsDataGrid.ItemsSource = ingredients.ToList();
                }
            }
            else
            {
                this.recipeNameLabel.Content = "";
                this.preparationTimeLabel.Content = "";
                this.descriptionTextBlock.Text = "";
                this.ingredientsDataGrid.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Updates ingredients list with items from the selected recipe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecipesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.recipesDataGrid.SelectedIndex >= 0 && this.recipesDataGrid.SelectedItems.Count >= 0)
            {
                selectedRecipeId = Convert.ToInt32(recipesDataGrid.SelectedItems[0].GetType().GetProperty("recipeId").GetValue(recipesDataGrid.SelectedItems[0]));
                UpdateIngredientsDataGrid();
            }
        }

        /// <summary>
        /// Updates selected recipeIngredientId
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ingredientsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ingredientsDataGrid.SelectedIndex >= 0 && this.ingredientsDataGrid.SelectedItems.Count >= 0)
            {
                selectedRecipeIngredientId = Convert.ToInt32(ingredientsDataGrid.SelectedItems[0].GetType().GetProperty("recipeIngredientId").GetValue(ingredientsDataGrid.SelectedItems[0]));
            }
        }

        /// <summary>
        /// Makes selected recipesDataGrid columns visible during auto column generation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecipesDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.Column.Header.ToString())
            {
                case "recipeName":
                    e.Column.Visibility = Visibility.Visible;
                    break;
                default:
                    e.Column.Visibility = Visibility.Hidden;
                    break;
            }
        }

        /// <summary>
        /// Makes selected ingredientsDataGrid columns visible during auto column generation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ingredientsDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.Column.Header.ToString())
            {
                case "measurementQuantity":
                    e.Column.Visibility = Visibility.Visible;
                    break;
                case "measurementDescription":
                    e.Column.Visibility = Visibility.Visible;
                    break;
                case "ingredientName":
                    e.Column.Visibility = Visibility.Visible;
                    break;
                default:
                    e.Column.Visibility = Visibility.Hidden;
                    break;
            }
        }

        /// <summary>
        /// Adds user-specified recipe to database Recipes table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            if (addRecipeNameTextBox.Text == "")
            {
                GenerateMessageBox("Make sure to fill in the recipe name before you add a new recipe", "Add new recipe");
                return;
            }
            else if (addPreparationTimeTextBox.Text == "")
            {
                GenerateMessageBox("Make sure to fill in the recipe preparation time before you add a new recipe", "Add new recipe");
                return;
            }
            else if (!double.TryParse(addPreparationTimeTextBox.Text, out _))
            {
                GenerateMessageBox("Preparation time must be a number of minutes", "Add new recipe");
                return;
            }

            Recipe recipeObject = new Recipe()
            {
                recipeName = addRecipeNameTextBox.Text,
                preparationTime = Convert.ToInt32(addPreparationTimeTextBox.Text),
                description = addDescriptionTextBox.Text
            };

            db.Recipes.Add(recipeObject);
            db.SaveChanges();
            UpdateIngredientsDataGrid();
            UpdateRecipesDataGrid();
        }

        /// <summary>
        /// Shows a MessageBox with specified content and title
        /// </summary>
        /// <param name="messageBoxText">Content of the generated MessageBox</param>
        /// <param name="messageBoxTitle">Title of the generated MessageBox</param>
        private void GenerateMessageBox(string messageBoxText, string messageBoxTitle)
        {
            MessageBox.Show(messageBoxText,
            messageBoxTitle,
            MessageBoxButton.OK,
            MessageBoxImage.Warning,
            MessageBoxResult.No
            );
        }

        /// <summary>
        /// Validates textbox content 
        /// </summary>
        /// <param name="buttonType">Name of the performed CRUD operation</param>
        /// <returns></returns>
        private bool ValidateAddUpdateIngredientButton(string buttonType)
        {
            bool isValidationPassed = true;
            string ingredientName = "";
            string ingredientAmount = "";
            string windowTitle = "";
            string emptyTextboxMessage = "";
            string amountMessage = "Amount must be a number";

            if (buttonType == "add")
            {
                ingredientName = addIngredientTextBox.Text;
                ingredientAmount = amountTextBox.Text;
                windowTitle = "Add ingredient";
                emptyTextboxMessage = "Make sure to fill in all of the textboxes before you add an ingredient";
            }

            // Checks if texbox content is empty
            if (ingredientName == "" || ingredientAmount == "")
            {
                GenerateMessageBox(emptyTextboxMessage, windowTitle);
                isValidationPassed = false;
            }
            // Checks if textbox content is a number
            else if (!double.TryParse(ingredientAmount, out _))
            {
                GenerateMessageBox(amountMessage, windowTitle);
                isValidationPassed = false;
            }

            return isValidationPassed;
        }

        /// <summary>
        /// Retrieves an existing ingredient from the database or creates a new one
        /// </summary>
        /// <param name="operationType">Name of the performed CRUD operation</param>
        /// <returns></returns>
        private Ingredient GetOrCreateIngredient(string operationType)
        {
            string ingredientName = "";
            if (operationType == "add")
            {
                ingredientName = addIngredientTextBox.Text;
            }

            var ingredients = from ingredient in db.Ingredients
                              where ingredient.ingredientName == ingredientName
                              select ingredient;

            Ingredient ingredientObject;

            // If ingredient doesn't exist yet, create a new ingredient
            if (ingredients.Count() == 0)
            {
                ingredientObject = new Ingredient()
                {
                    ingredientName = ingredientName
                };
                db.Ingredients.Add(ingredientObject);
            }
            // If ingredient already exists, get the existing ingredient
            else
            {
                ingredientObject = ingredients.FirstOrDefault();
            }

            return ingredientObject;
        }

        /// <summary>
        /// Adds user-specified ingredient to database Ingredients table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRecipeId != 0)
            {
                if (!ValidateAddUpdateIngredientButton("add"))
                {
                    return;
                }

                Ingredient ingredientObject = GetOrCreateIngredient("add");

                var measurementTypes = from measurement in db.MeasurementUnits
                                       where measurement.measurementDescription == selectedMeasurementDescription
                                       select measurement;

                MeasurementUnit measurementObject = measurementTypes.SingleOrDefault();

                RecipeIngredient recipeIngredientObject = new RecipeIngredient()
                {
                    recipeId = selectedRecipeId,
                    measurementId = measurementObject.measurementId,
                    ingredientId = ingredientObject.ingredientId,
                    measurementQuantity = Convert.ToDouble(this.amountTextBox.Text)
                };

                db.RecipeIngredients.Add(recipeIngredientObject);
                db.SaveChanges();
                UpdateIngredientsDataGrid();
            }
            else
            {
                GenerateMessageBox("Choose a recipe before you add an ingredient", "Add ingredient");
            }
        }

        /// <summary>
        /// Updates selected measurement description from the measurementComboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeasurementComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.measurementComboBox.SelectedItem != null)
            {
                selectedMeasurementDescription = this.measurementComboBox.SelectedItem.ToString();
            }
        }

        /// <summary>
        /// Populates specified ComboBox with measurement units from the database
        /// </summary>
        /// <param name="measurementType">Selected measurement type</param>
        /// <param name="comboBox">Specifies the CRUD operation ComboBox type</param>
        private void RadioButtonChecked(string measurementType, string comboBox)
        {
            var units = from unit in db.MeasurementUnits
                        where unit.type == measurementType
                        orderby unit.measurementDescription
                        select new
                        {
                            unit.measurementDescription
                        };

            List<string> unitDescriptions = new List<string>();
            foreach (var unit in units)
            {
                unitDescriptions.Add(unit.measurementDescription);
            }

            if (comboBox == "measurementComboBox")
            {
                this.measurementComboBox.ItemsSource = unitDescriptions;

                if (this.measurementComboBox.Items.Count > 0)
                {
                    this.measurementComboBox.SelectedItem = this.measurementComboBox.Items[0];
                }
            }
        }

        /// <summary>
        /// Event handler for checking UpdateVolumeRadioButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VolumeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonChecked("volume", "measurementComboBox");
        }

        /// <summary>
        /// Event handler for checking UpdateMassRadioButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MassRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonChecked("mass", "measurementComboBox");
        }

        /// <summary>
        /// Event handler for checking UpdateQuantityRadioButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuantityRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonChecked("quantity", "measurementComboBox");
        }

        /// <summary>
        /// Deletes user-specified recipe from the database Recipes table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRecipeId != 0)
            {
                var recipes = from recipe in db.Recipes
                              where recipe.recipeId == selectedRecipeId
                              select recipe;

                Recipe recipeObject = recipes.SingleOrDefault();

                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to delete " + recipeObject.recipeName + " from your recipes?",
                    "Delete recipe",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No
                    );

                var recipeIngredients = from recipeIngredient in db.RecipeIngredients
                                        where recipeIngredient.recipeId == selectedRecipeId
                                        select recipeIngredient;

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (recipeObject != null)
                    {
                        foreach (var element in recipeIngredients)
                        {
                            db.RecipeIngredients.Remove(element);
                        }
                        db.Recipes.Remove(recipeObject);
                        db.SaveChanges();
                        selectedRecipeId = 0;
                        UpdateIngredientsDataGrid();
                        UpdateRecipesDataGrid();
                    }
                }
            }
        }

        /// <summary>
        /// Deletes user-specified ingredient from the database RecipeINgredients table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRecipeIngredientId != 0)
            {
                var recipeIngredients = from recipeIngredient in db.RecipeIngredients
                                        where recipeIngredient.recipeIngredientId == selectedRecipeIngredientId
                                        select recipeIngredient;

                RecipeIngredient recipeIngredientObject = recipeIngredients.SingleOrDefault();

                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to delete " + recipeIngredientObject.Ingredient.ingredientName + " from your recipe?",
                    "Delete ingredient",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No
                    );

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (recipeIngredientObject != null)
                    {
                        db.RecipeIngredients.Remove(recipeIngredientObject);
                        db.SaveChanges();
                        selectedRecipeIngredientId = 0;
                        UpdateIngredientsDataGrid();
                    }
                }
            }
            else
            {
                GenerateMessageBox("Choose an ingredient you want to delete", "Delete ingredient");
            }
        }
    }
}
