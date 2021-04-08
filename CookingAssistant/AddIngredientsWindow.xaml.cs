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
    /// Interaction logic for AddIngredientsWindow.xaml
    /// </summary>
    public partial class AddIngredientsWindow : Window
    {
        private CookingAssistantDBEntities db = new CookingAssistantDBEntities();
        private int updatedIngredientId = 0;
        public AddIngredientsWindow()
        {
            InitializeComponent();

            CookingAssistantDBEntities db = new CookingAssistantDBEntities();
            var ingredients = from ingredient in db.Ingredients
                              select ingredient;

            this.ingredientsDataGrid.ItemsSource = ingredients.ToList();
        }

        private void IngredientsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ingredientsDataGrid.SelectedIndex >= 0 && this.ingredientsDataGrid.SelectedItems.Count >= 0)
            {
                Ingredient i = (Ingredient)this.ingredientsDataGrid.SelectedItems[0];
                this.updateIngredientNameTextBox.Text = i.ingredientName;
                updatedIngredientId = i.ingredientId;
            }
        }

        private void AddIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            Ingredient ingredientObject = new Ingredient()
            {
                ingredientName = addIngredientNameTextBox.Text
            };

            db.Ingredients.Add(ingredientObject);
            db.SaveChanges();
            this.ingredientsDataGrid.ItemsSource = db.Ingredients.ToList();
        }

        private void UpdateIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            var ingredients = from i in db.Ingredients
                             where i.ingredientId == updatedIngredientId
                             select i;

            Ingredient obj = ingredients.SingleOrDefault();

            if (obj != null)
            {
                obj.ingredientName = updateIngredientNameTextBox.Text;
            }

            db.SaveChanges();
            this.ingredientsDataGrid.ItemsSource = db.Ingredients.ToList();
        }

        private void DeleteIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            var ingredients = from i in db.Ingredients
                              where i.ingredientId == updatedIngredientId
                              select i;

            Ingredient obj = ingredients.SingleOrDefault();

            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to delete " + obj.ingredientName + " from your ingredient list?",
                "Delete ingredient",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No
                );

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                if (obj != null)
                {
                    db.Ingredients.Remove(obj);
                    db.SaveChanges();
                }
            }
            this.ingredientsDataGrid.ItemsSource = db.Ingredients.ToList();
        }
    }
}
