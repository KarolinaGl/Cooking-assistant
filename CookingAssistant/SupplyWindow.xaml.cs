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
    /// Interaction logic for SupplyWindow.xaml
    /// </summary>
    public partial class SupplyWindow : Window
    {
        private CookingAssistantDBEntities db = new CookingAssistantDBEntities();
        private int selectedSupplyId = 0;
        private string selectedMeasurementDescription = "";
        private string selectedUpdateMeasurementDescription = "";
        public SupplyWindow()
        {
            InitializeComponent();
            // volumeRadioButton checked by default
            this.volumeRadioButton.IsChecked = true;
            this.updateVolumeRadioButton.IsChecked = true;

            UpdateIngredientsDataGrid();
        }

        /// <summary>
        /// Fills ingredientsDataGrid with supplies from the database 
        /// </summary>
        public void UpdateIngredientsDataGrid ()
        {
            var supplies = from supply in db.Supplies
                           select new
                           {
                               supply.supplyId,
                               supply.measurementQuantity,
                               supply.MeasurementUnit.measurementDescription,
                               supply.MeasurementUnit.type,
                               supply.Ingredient.ingredientName
                           };

            this.ingredientsDataGrid.ItemsSource = supplies.ToList();
        }

        /// <summary>
        /// Populates the ingredient update controls every time the user clicks on a datagrid row
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IngredientsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ingredientsDataGrid.SelectedIndex >= 0 && this.ingredientsDataGrid.SelectedItems.Count >= 0)
            {
                selectedSupplyId = Convert.ToInt32(ingredientsDataGrid.SelectedItems[0].GetType().GetProperty("supplyId").GetValue(ingredientsDataGrid.SelectedItems[0]));
                this.updateAmountTextBox.Text = Convert.ToString(ingredientsDataGrid.SelectedItems[0].GetType().GetProperty("measurementQuantity").GetValue(ingredientsDataGrid.SelectedItems[0]));
                this.updateIngredientNameTextBox.Text = Convert.ToString(ingredientsDataGrid.SelectedItems[0].GetType().GetProperty("ingredientName").GetValue(ingredientsDataGrid.SelectedItems[0]));
                string selectedMeasurementType = Convert.ToString(ingredientsDataGrid.SelectedItems[0].GetType().GetProperty("type").GetValue(ingredientsDataGrid.SelectedItems[0])).Trim();

                switch(selectedMeasurementType)
                {
                    case "volume":
                        this.updateVolumeRadioButton.IsChecked = true;
                        break;
                    case "mass":
                        this.updateMassRadioButton.IsChecked = true;
                        break;
                    case "quantity":
                        this.updateQuantityRadioButton.IsChecked = true;
                        break;
                }

                this.updateMeasurementComboBox.SelectedItem = Convert.ToString(ingredientsDataGrid.SelectedItems[0].GetType().GetProperty("measurementDescription").GetValue(ingredientsDataGrid.SelectedItems[0]));
            }
        }

        /// <summary>
        /// Shows a MessageBox with specified content and title
        /// </summary>
        /// <param name="messageBoxText">Content of the generated MessageBox</param>
        /// <param name="messageBoxTitle">Title of the generated MessageBox</param>
        public void GenerateMessageBox(string messageBoxText, string messageBoxTitle)
        {
            MessageBox.Show(messageBoxText,
                            messageBoxTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning,
                            MessageBoxResult.No);
        }

        /// <summary>
        /// Retrieves an existing ingredient from the database or creates a new one
        /// </summary>
        /// <param name="operationType">Name of the performed CRUD operation</param>
        /// <returns>Ingredient object</returns>
        public Ingredient GetOrCreateIngredient(string operationType)
        {
            string ingredientName = "";
            if (operationType == "add")
            {
                ingredientName = addIngredientTextBox.Text;
            }
            else if (operationType == "update")
            {
                ingredientName = updateIngredientNameTextBox.Text;
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
        /// Validates received addIngredientTextBox and amountTextBox content
        /// </summary>
        /// <param name="ingredientName">addIngredientTextBox content</param>
        /// <param name="ingredientAmount">amountTextBox content</param>
        /// <returns></returns>
        public static string[] ValidateText(string buttonType, string ingredientName, string ingredientAmount)
        {
            string[] outputArray = new string[2];
            double parsedNumber;

            if (buttonType == "update")
            {
                outputArray[1] = "Update ingredient";
                outputArray[0] = "Make sure to fill in all of the textboxes before you update an ingredient";
            }
            else if (buttonType == "add")
            {
                outputArray[1] = "Add ingredient";
                outputArray[0] = "Make sure to fill in all of the textboxes before you add an ingredient";
            }

            if (ingredientName == "" || ingredientAmount == "")
            {
                return outputArray;
            }
            // Checks if textbox content is a number
            if (!double.TryParse(ingredientAmount, out parsedNumber))
            {
                outputArray[0] = "Amount must be a number";
                return outputArray;
            }
            else
            {
                if (parsedNumber <= 0)
                {
                    outputArray[0] = "Amount must be a positive number";
                    return outputArray;
                }
            }
            
            outputArray = new string[] { null, null };
            return outputArray;
        }

        /// <summary>
        /// Validates textbox content 
        /// </summary>
        /// <param name="buttonType">Name of the performed CRUD operation</param>
        /// <returns>True if textbox input was validated</returns>
        public bool ValidateAddUpdateIngredientButton(string buttonType)
        {
            string ingredientName = "";
            string ingredientAmount = "";

            if (buttonType == "update")
            {
                ingredientName = updateIngredientNameTextBox.Text;
                ingredientAmount = updateAmountTextBox.Text;
            }
            else if (buttonType == "add")
            {
                ingredientName = addIngredientTextBox.Text;
                ingredientAmount = amountTextBox.Text;
            }

            string[] output = ValidateText(buttonType, ingredientName, ingredientAmount);

            if (output[0] != null && output[1] != null)
            {
                GenerateMessageBox(output[0], output[1]);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds user-specified ingredient to database Supplies table 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddIngredientButton_Click(object sender, RoutedEventArgs e)
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

            Supply supplyObject = new Supply()
            {
                measurementId = measurementObject.measurementId,
                ingredientId = ingredientObject.ingredientId,
                measurementQuantity = Convert.ToDouble(this.amountTextBox.Text)
            };

            db.Supplies.Add(supplyObject);
            db.SaveChanges();
            UpdateIngredientsDataGrid();
        }

        /// <summary>
        /// Updates user-specified ingredient in the database Supplies table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            if(!ValidateAddUpdateIngredientButton("update"))
            {
                return;
            }

            Ingredient ingredientObject = GetOrCreateIngredient("update");

            var supplies = from supply in db.Supplies
                           where supply.supplyId == selectedSupplyId
                           select supply;

            Supply supplyObject = supplies.SingleOrDefault();

            var measurementUnits = from unit in db.MeasurementUnits
                                   where unit.measurementDescription == selectedUpdateMeasurementDescription
                                   select unit;

            MeasurementUnit measurementUnit = measurementUnits.SingleOrDefault();

            if (supplyObject != null)
            {
                supplyObject.measurementQuantity = Convert.ToDouble(updateAmountTextBox.Text);
                supplyObject.ingredientId = ingredientObject.ingredientId;
                supplyObject.measurementId = measurementUnit.measurementId;
            }

            db.SaveChanges();
            UpdateIngredientsDataGrid();
        }

        /// <summary>
        /// Deletes user-specified supply from the database Supplies table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DeleteIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSupplyId != 0)
            {
                var supplies = from supply in db.Supplies
                               where supply.supplyId == selectedSupplyId
                               select supply;

                Supply obj = supplies.SingleOrDefault();

                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to delete " + obj.Ingredient.ingredientName + " from your supplies?",
                    "Delete ingredient",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No
                    );

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (obj != null)
                    {
                        db.Supplies.Remove(obj);
                        db.SaveChanges();
                        UpdateIngredientsDataGrid();
                    }
                }
                selectedSupplyId = 0;
            }
            else
            {
                GenerateMessageBox("Choose an ingredient you want to delete", "Delete ingredient");
            }
        }

        /// <summary>
        /// Updates selected measurement description from the measurementComboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MeasurementComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.measurementComboBox.SelectedItem != null)
            {
                selectedMeasurementDescription = this.measurementComboBox.SelectedItem.ToString();
            }
        }

        /// <summary>
        /// Updates selected measurement description from the updateMeasurementComboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateMeasurementComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.updateMeasurementComboBox.SelectedItem != null)
            {
                selectedUpdateMeasurementDescription = this.updateMeasurementComboBox.SelectedItem.ToString();
            }
        }

        /// <summary>
        /// Makes selected DataGrid columns visible during auto column generation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IngredientsDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.Column.Header.ToString())
            {
                case "measurementQuantity":
                    e.Column.Visibility = Visibility.Visible;
                    e.Column.Header = "Quantity";
                    break;
                case "measurementDescription":
                    e.Column.Visibility = Visibility.Visible;
                    e.Column.Header = "Measurement unit";
                    break;
                case "ingredientName":
                    e.Column.Visibility = Visibility.Visible;
                    e.Column.Header = "Ingredient";
                    break;
                default:
                    e.Column.Visibility = Visibility.Hidden;
                    break;
            }
        }

        /// <summary>
        /// Populates specified ComboBox with measurement units from the database
        /// </summary>
        /// <param name="measurementType">Selected measurement type</param>
        /// <param name="comboBox">Specifies the CRUD operation ComboBox type</param>
        public void RadioButtonChecked(string measurementType, string comboBox)
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
            else if (comboBox == "updateMeasurementComboBox")
            {
                this.updateMeasurementComboBox.ItemsSource = unitDescriptions;

                if (this.updateMeasurementComboBox.Items.Count > 0)
                {
                    this.updateMeasurementComboBox.SelectedItem = this.updateMeasurementComboBox.Items[0];
                }
            }
        }

        /// <summary>
        /// Event handler for checking VolumeRadioButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void VolumeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonChecked("volume", "measurementComboBox");
        }

        /// <summary>
        /// Event handler for checking MassRadioButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MassRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonChecked("mass", "measurementComboBox");
        }

        /// <summary>
        /// Event handler for checking QuantityRadioButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void QuantityRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonChecked("quantity", "measurementComboBox");
        }

        /// <summary>
        /// Event handler for checking UpdateVolumeRadioButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateVolumeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonChecked("volume", "updateMeasurementComboBox");
        }

        /// <summary>
        /// Event handler for checking UpdateMassRadioButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateMassRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonChecked("mass", "updateMeasurementComboBox");
        }

        /// <summary>
        /// Event handler for checking UpdateQuantityRadioButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateQuantityRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButtonChecked("quantity", "updateMeasurementComboBox");
        }

        private void SupplyWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (this.Owner as MainWindow).BindSupplies();
        }
    }
}
