using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Car_Sales_Management_System.DataModels;

namespace Car_Sales_Management_System
{
    public partial class MainWindow : Window
    {
        private CarService? _carService;
        public ObservableCollection<Car> Cars { get; set; }
        private List<Car> _allCars = new List<Car>();


        public MainWindow()
        {
            InitializeComponent();
            _carService = new CarService();
            Cars = new ObservableCollection<Car>();
            DataContext = this; // Set DataContext for binding
            LoadCars();
        }

        private void LoadCars()
        {
            try
            {
                List<Car> cars = _carService?.GetAllCars() ?? new List<Car>();

                _allCars = cars; // Save for filtering
                Cars = new ObservableCollection<Car>();
                CarGrid.ItemsSource = Cars;

                MessageBox.Show($"Loaded {cars.Count} cars from DB");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading cars: " + ex.Message);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allCars == null || Cars == null) return;

            string query = SearchTextBox.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(query))
            {
                Cars.Clear();
                foreach (var car in _allCars)
                    Cars.Add(car);
                return;
            }

            var filtered = _allCars.Where(car =>
                (!string.IsNullOrEmpty(car.Make) && car.Make.ToLower().Contains(query)) ||
                (!string.IsNullOrEmpty(car.Model) && car.Model.ToLower().Contains(query)) ||
                (!string.IsNullOrEmpty(car.Color) && car.Color.ToLower().Contains(query)) ||
                (!string.IsNullOrEmpty(car.VIN) && car.VIN.ToLower().Contains(query)) ||
                (!string.IsNullOrEmpty(car.Condition) && car.Condition.ToLower().Contains(query)) ||
                (!string.IsNullOrEmpty(car.Status) && car.Status.ToLower().Contains(query))
            ).ToList();

            Cars.Clear();
            foreach (var car in filtered)
                Cars.Add(car);
        }

    }
}
