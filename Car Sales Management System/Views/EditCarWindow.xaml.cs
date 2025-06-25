using System;
using System.Windows;
using System.Windows.Controls;
using Car_Sales_Management_System.DataModels;

namespace Car_Sales_Management_System.Views
{
    public partial class EditCarWindow : Window
    {
        private readonly Car _car;
        private readonly CarService _carService;

        public EditCarWindow(Car car, CarService carService)
        {
            InitializeComponent();
            _car = car ?? throw new ArgumentNullException(nameof(car));
            _carService = carService ?? throw new ArgumentNullException(nameof(carService));
            PopulateFields();
        }

        private void PopulateFields()
        {
            try
            {
                // Populate the fields with the current car data
                MakeTextBox.Text = _car.Make;
                ModelTextBox.Text = _car.Model;
                YearTextBox.Text = _car.Year.ToString();
                MileageTextBox.Text = _car.Mileage.ToString();
                ColorTextBox.Text = _car.Color;
                PriceTextBox.Text = _car.Price.ToString();
                ConditionComboBox.SelectedItem = ConditionComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == _car.Condition);
                StatusComboBox.SelectedItem = StatusComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == _car.Status);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading car data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(MakeTextBox.Text))
                {
                    MessageBox.Show("Make is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(ModelTextBox.Text))
                {
                    MessageBox.Show("Model is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(YearTextBox.Text, out int year) || year < 1900 || year > DateTime.Now.Year + 1)
                {
                    MessageBox.Show("Please enter a valid year.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(MileageTextBox.Text, out int mileage) || mileage < 0)
                {
                    MessageBox.Show("Please enter a valid mileage (non-negative number).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(PriceTextBox.Text, out int price) || price < 0)
                {
                    MessageBox.Show("Please enter a valid price (non-negative number).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ConditionComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a condition.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (StatusComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a status.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update car properties
                _car.Make = MakeTextBox.Text.Trim();
                _car.Model = ModelTextBox.Text.Trim();
                _car.Year = year;
                _car.Mileage = mileage;
                _car.Color = ColorTextBox.Text.Trim();
                _car.Price = price;
                _car.Condition = (ConditionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                _car.Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                // Update the car in the database
                bool success = _carService.UpdateCar(_car.Id.ToString(), _car);

                if (success)
                {
                    MessageBox.Show("Car updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true; // Indicate success
                    Close();
                }
                else
                {
                    MessageBox.Show("Failed to update car. It may no longer exist or there was a database issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving car: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Indicate cancellation
            Close();
        }
    }
}