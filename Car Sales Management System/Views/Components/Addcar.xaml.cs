using Car_Sales_Management_System.DataModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Car_Sales_Management_System.Views.Components
{
    public partial class Addcar : UserControl
    {
        public event EventHandler CarAdded;
        private readonly CarService _carService;

        public Addcar()
        {
            InitializeComponent();
            _carService = new CarService();
            MakeComboBox.SelectionChanged += MakeComboBox_SelectionChanged;
            ListedDatePicker.SelectedDate = DateTime.Now;
        }

        private void MakeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMakeItem = MakeComboBox.SelectedItem as ComboBoxItem;
            var selectedMake = selectedMakeItem?.Content?.ToString();

            if (string.IsNullOrEmpty(selectedMake) || !_makeModelMap.ContainsKey(selectedMake))
            {
                ModelComboBox.Items.Clear();
                ModelComboBox.IsEnabled = false;
            }
            else
            {
                ModelComboBox.Items.Clear();
                foreach (var model in _makeModelMap[selectedMake])
                {
                    ModelComboBox.Items.Add(new ComboBoxItem { Content = model });
                }
                ModelComboBox.IsEnabled = true;
            }
        }



        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Basic validation
                if (!ValidateInputs(out int year, out int mileage, out int price))
                {
                    ShowError("Please fill all fields correctly.");
                    return;
                }

                // Retrieve ComboBox values safely
                var condition = (ConditionComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
                var status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

                if (string.IsNullOrEmpty(condition) || string.IsNullOrEmpty(status))
                {
                    MessageBox.Show("Please select both condition and status.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var car = new Car
                {
                    Make = MakeComboBox.Text.Trim(),
                    Model = ModelComboBox.Text.Trim(),
                    Year = year,
                    Mileage = mileage,
                    Color = ColorTextBox.Text.Trim(),
                    Price = price,
                    Condition = condition,
                    Status = status,
                    ListedDate = ListedDatePicker.SelectedDate.Value.ToUniversalTime(),
                    VIN = "" // Optional: you can add a VINTextBox and use VINTextBox.Text.Trim()
                };

                _carService.AddCar(car);

                MessageBox.Show("Car added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CarAdded?.Invoke(this, EventArgs.Empty);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding car: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs(out int year, out int mileage, out int price)
        {
            year = mileage = price = 0;
            string make = MakeComboBox.Text.Trim();
            string model = ModelComboBox.Text.Trim();

            bool isValidModel = _makeModelMap.TryGetValue(make, out var validModels) &&
                                validModels.Contains(model);

            if (!isValidModel)
            {
                ShowError("The selected model does not match the selected make.");
                return false;
            }

            return !string.IsNullOrWhiteSpace(make) &&
                   isValidModel &&
                   int.TryParse(YearTextBox.Text, out year) &&
                   int.TryParse(MileageTextBox.Text, out mileage) &&
                   !string.IsNullOrWhiteSpace(ColorTextBox.Text) &&
                   int.TryParse(PriceTextBox.Text, out price) &&
                   ConditionComboBox.SelectedItem != null &&
                   StatusComboBox.SelectedItem != null &&
                   ListedDatePicker.SelectedDate.HasValue;


        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }
        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private void ClearForm()
        {
            MakeComboBox.Text = "";
            MakeComboBox.SelectedIndex = -1;
            ModelComboBox.Text = "";
            YearTextBox.Text = "";
            MileageTextBox.Text = "";
            ColorTextBox.Text = "";
            PriceTextBox.Text = "";
            ConditionComboBox.SelectedIndex = -1;
            StatusComboBox.SelectedIndex = -1;
            ListedDatePicker.SelectedDate = DateTime.Now;
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }


        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }




        private readonly Dictionary<string, List<string>> _makeModelMap = new Dictionary<string, List<string>>
        {
            { "Acura", new List<string> { "ILX", "MDX", "RDX", "TLX" } },
            { "Alfa Romeo", new List<string> { "Giulia", "Stelvio", "Tonale" } },
            { "Audi", new List<string> { "A3", "A4", "A6", "Q5", "Q7" } },
            { "BMW", new List<string> { "X5", "3 Series", "5 Series", "X3" } },
            { "Buick", new List<string> { "Enclave", "Encore", "Regal" } },
            { "Cadillac", new List<string> { "Escalade", "XT4", "CT5" } },
            { "Chevrolet", new List<string> { "Silverado", "Equinox", "Malibu", "Tahoe" } },
            { "Chrysler", new List<string> { "300", "Pacifica" } },
            { "Citroën", new List<string> { "C3", "C4", "C5 Aircross" } },
            { "Dodge", new List<string> { "Charger", "Durango", "Challenger" } },
            { "Ferrari", new List<string> { "488", "F8 Tributo", "Roma" } },
            { "Fiat", new List<string> { "500", "Panda", "Tipo" } },
            { "Ford", new List<string> { "F-150", "Escape", "Mustang", "Explorer" } },
            { "Genesis", new List<string> { "G70", "G80", "GV80" } },
            { "GMC", new List<string> { "Sierra", "Terrain", "Acadia" } },
            { "Honda", new List<string> { "Civic", "Accord", "CR-V", "Pilot" } },
            { "Hyundai", new List<string> { "Elantra", "Santa Fe", "Tucson" } },
            { "Infiniti", new List<string> { "Q50", "QX60", "QX80" } },
            { "Jaguar", new List<string> { "XE", "XF", "F-Pace" } },
            { "Jeep", new List<string> { "Wrangler", "Grand Cherokee", "Compass" } },
            { "Kia", new List<string> { "Soul", "Sorento", "Sportage" } },
            { "Lamborghini", new List<string> { "Huracan", "Aventador", "Urus" } },
            { "Land Rover", new List<string> { "Range Rover", "Discovery", "Defender" } },
            { "Lexus", new List<string> { "RX", "ES", "NX" } },
            { "Lincoln", new List<string> { "Nautilus", "Corsair", "Aviator" } },
            { "Maserati", new List<string> { "Ghibli", "Levante", "Quattroporte" } },
            { "Mazda", new List<string> { "Mazda3", "CX-5", "CX-9" } },
            { "Mercedes-Benz", new List<string> { "C-Class", "E-Class", "GLC", "GLE" } },
            { "Mini", new List<string> { "Cooper", "Countryman", "Clubman" } },
            { "Mitsubishi", new List<string> { "Outlander", "Eclipse Cross", "Mirage" } },
            { "Nissan", new List<string> { "Altima", "Rogue", "Sentra" } },
            { "Peugeot", new List<string> { "208", "308", "3008" } },
            { "Porsche", new List<string> { "911", "Cayenne", "Macan" } },
            { "Ram", new List<string> { "1500", "2500", "3500" } },
            { "Renault", new List<string> { "Clio", "Megane", "Captur" } },
            { "Saab", new List<string> { "9-3", "9-5" } },
            { "Subaru", new List<string> { "Impreza", "Outback", "Forester" } },
            { "Suzuki", new List<string> { "Swift", "Vitara", "Jimny" } },
            { "Tesla", new List<string> { "Model S", "Model 3", "Model X", "Model Y" } },
            { "Toyota", new List<string> { "Corolla", "Camry", "RAV4", "Highlander" } },
            { "Volkswagen", new List<string> { "Golf", "Passat", "Tiguan" } },
            { "Volvo", new List<string> { "XC40", "XC60", "XC90" } }
        };

        private void OnBlah(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
