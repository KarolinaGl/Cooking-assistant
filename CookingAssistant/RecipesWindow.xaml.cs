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
using System.Data.SqlClient;

namespace CookingAssistant
{
    /// <summary>
    /// Interaction logic for RecipesWindow.xaml
    /// </summary>
    public partial class RecipesWindow : Window
    {
        private CookingAssistantDBEntities db = new CookingAssistantDBEntities();
        private int selectedRecipeId = 1;
        public RecipesWindow()
        {
            InitializeComponent();

            var recipes = from recipe in db.Recipes
                          where recipe.recipeId == selectedRecipeId
                          select recipe;

            Recipe obj = recipes.SingleOrDefault();

            this.recipeNameLabel.Content = obj.recipeName;
            this.preparationTimeLabel.Content = "Preparation time: " + obj.preparationTime + " minutes";
            this.descriptionTextBlock.Text = obj.description;

            var ingredients = from r in db.RecipeIngredients
                              where r.recipeId == selectedRecipeId
                              select new
                              {
                                  r.measurementQuantity,
                                  r.MeasurementUnit.measurementDescription,
                                  r.Ingredient.ingredientName
                              };

            this.recipesDataGrid.ItemsSource = ingredients.ToList();
        }
    }
}
