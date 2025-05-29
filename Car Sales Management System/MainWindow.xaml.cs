using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Car_Sales_Management_System.DataModels;

using Car_Sales_Management_System.Views.Components; // Ensure this namespace is correct

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

            CarGrid.ItemsSource = Cars; // Bind the DataGrid to the Cars collection
            CarFilterControl.FilterChanged += ApplyFilters;

            LoadCars();

        }

        private void LoadCars()
        {
            try
            {
                List<Car> cars = _carService?.GetAllCars() ?? new List<Car>();

                _allCars = cars; // Save for filtering

                foreach (var car in cars)
                {
                    Cars.Add(car);
                }

                //CarGrid.ItemsSource = Cars;

                CarFilterControl.LoadFilterOptions(cars);

                MessageBox.Show($"Loaded {cars.Count} cars from DB");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading cars: " + ex.Message);
            }
        }

        private void ApplyFilters(CarFilter.CarFilterCriteria criteria)
        {
            var filtered = _allCars.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(criteria.Make))
                filtered = filtered.Where(c => c.Make?.Equals(criteria.Make, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(criteria.Model))
                filtered = filtered.Where(c => c.Model?.Equals(criteria.Model, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(criteria.Color))
                filtered = filtered.Where(c => c.Color?.Equals(criteria.Color, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(criteria.Condition))
                filtered = filtered.Where(c => c.Condition?.Equals(criteria.Condition, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(criteria.Year) && int.TryParse(criteria.Year, out int year))
                filtered = filtered.Where(c => c.Year == year);

            if (!string.IsNullOrWhiteSpace(criteria.Mileage) && int.TryParse(criteria.Mileage, out int mileage))
                filtered = filtered.Where(c => c.Mileage == mileage);

            if (!string.IsNullOrWhiteSpace(criteria.PriceRange))
            {
                filtered = criteria.PriceRange switch
                {
                    "0 - 10,000" => filtered.Where(c => c.Price >= 0 && c.Price <= 10000),
                    "10,000 - 20,000" => filtered.Where(c => c.Price > 10000 && c.Price <= 20000),
                    "20,000 - 30,000" => filtered.Where(c => c.Price > 20000 && c.Price <= 30000),
                    "30,000 - 50,000" => filtered.Where(c => c.Price > 30000 && c.Price <= 50000),
                    "50,000+" => filtered.Where(c => c.Price > 50000),
                    _ => filtered
                };
            }

            if (!string.IsNullOrWhiteSpace(criteria.SearchText))
            {
                string search = criteria.SearchText.ToLower();
                filtered = filtered.Where(car =>
                    (car.Make != null && car.Make.ToLower().Contains(search)) ||
                    (car.Model != null && car.Model.ToLower().Contains(search)) ||
                    (car.Color != null && car.Color.ToLower().Contains(search)) ||
                    (car.VIN != null && car.VIN.ToLower().Contains(search)) ||
                    (car.Condition != null && car.Condition.ToLower().Contains(search)) ||
                    (car.Status != null && car.Status.ToLower().Contains(search))
                );
            }

            Cars.Clear();
            foreach (var car in filtered)
                Cars.Add(car);
        }


        //private void FilterCars(string query)
        //{
        //    if (_allCars == null || Cars == null) return;

        //    var filtered = string.IsNullOrWhiteSpace(query)
        //        ? _allCars
        //        : _allCars.Where(car =>
        //            (!string.IsNullOrEmpty(car.Make) && car.Make.ToLower().Contains(query)) ||
        //            (!string.IsNullOrEmpty(car.Model) && car.Model.ToLower().Contains(query)) ||
        //            (!string.IsNullOrEmpty(car.Color) && car.Color.ToLower().Contains(query)) ||
        //            (!string.IsNullOrEmpty(car.VIN) && car.VIN.ToLower().Contains(query)) ||
        //            (!string.IsNullOrEmpty(car.Condition) && car.Condition.ToLower().Contains(query))
        //        ).ToList();

        //    Cars.Clear();
        //    foreach (var car in filtered)
        //        Cars.Add(car);
        //}


    }
}
