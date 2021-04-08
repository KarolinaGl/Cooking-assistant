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
    /// Interaction logic for AddMeasurementsWindow.xaml
    /// </summary>
    public partial class AddMeasurementsWindow : Window
    {
        private CookingAssistantDBEntities db = new CookingAssistantDBEntities();
        private int updatedMeasurementId = 0;
        public AddMeasurementsWindow()
        {
            InitializeComponent();
            CookingAssistantDBEntities db = new CookingAssistantDBEntities();
            var measurements = from m in db.MeasurementUnits
                              select m;

            this.measurementsDataGrid.ItemsSource = measurements.ToList();
        }

        private void measurementsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.measurementsDataGrid.SelectedIndex >= 0 && this.measurementsDataGrid.SelectedItems.Count >= 0)
            {
                MeasurementUnit m = (MeasurementUnit)this.measurementsDataGrid.SelectedItems[0];
                this.UpdateMeasurementNameTextBox.Text = m.measurementDescription;
                updatedMeasurementId = m.measurementId;
            }
        }

        private void AddMeasurementButton_Click(object sender, RoutedEventArgs e)
        {
            MeasurementUnit measurementObject = new MeasurementUnit()
            {
                measurementDescription = AddMeasurementNameTextBox.Text
            };

            db.MeasurementUnits.Add(measurementObject);
            db.SaveChanges();
            this.measurementsDataGrid.ItemsSource = db.MeasurementUnits.ToList();
        }

        private void UpdateMeasurementButton_Click(object sender, RoutedEventArgs e)
        {
            var measurements = from m in db.MeasurementUnits
                              where m.measurementId == updatedMeasurementId
                              select m;

            MeasurementUnit obj = measurements.SingleOrDefault();

            if (obj != null)
            {
                obj.measurementDescription = UpdateMeasurementNameTextBox.Text;
            }

            db.SaveChanges();
            this.measurementsDataGrid.ItemsSource = db.MeasurementUnits.ToList();
        }

        private void DeleteMeasurementButton_Click(object sender, RoutedEventArgs e)
        {
            var measurements = from m in db.MeasurementUnits
                              where m.measurementId == updatedMeasurementId
                              select m;

            MeasurementUnit obj = measurements.SingleOrDefault();

            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to delete " + obj.measurementDescription + " from your measurement units list?",
                "Delete measurement unit",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No
                );

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                if (obj != null)
                {
                    db.MeasurementUnits.Remove(obj);
                    db.SaveChanges();
                }
            }
            this.measurementsDataGrid.ItemsSource = db.MeasurementUnits.ToList();
        }
    }
}
