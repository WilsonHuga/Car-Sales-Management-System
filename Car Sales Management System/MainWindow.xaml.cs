using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Car_Sales_Management_System.DataModels;
using Car_Sales_Management_System.Views;
using Car_Sales_Management_System.Views.Components; // Adjust namespace as needed

namespace Car_Sales_Management_System
{
    // Simple RelayCommand<T> implementation for command binding
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || (parameter is T t && _canExecute(t));
        }

        public void Execute(object? parameter)
        {
            if (parameter is T t)
                _execute(t);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value!;
            remove => CommandManager.RequerySuggested -= value!;
        }
    }

    public partial class MainWindow : Window
    {
        private CarService? _carService;
        public ObservableCollection<Car> Cars { get; set; }
        private List<Car> _allCars = new List<Car>();

        public ICommand RequestViewingCommand { get; }
        public ICommand GeneralEnquiryCommand { get; }

        public MainWindow()
        {
            InitializeComponent();
            _carService = new CarService();
            Cars = new ObservableCollection<Car>();
            DataContext = this; // Set DataContext for binding

            // Initialize commands
            RequestViewingCommand = new RelayCommand<Car>(RequestViewing);
            GeneralEnquiryCommand = new RelayCommand<Car>(GeneralEnquiry);

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

                Cars.Clear();
                foreach (var car in cars)
                {
                    Cars.Add(car);
                }

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

        private void ToggleViewButton_Checked(object sender, RoutedEventArgs e)
        {
            CarGrid.Visibility = Visibility.Collapsed;
            CardViewScrollViewer.Visibility = Visibility.Visible;
            ToggleViewButton.Content = "Table View";
        }

        private void ToggleViewButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CarGrid.Visibility = Visibility.Visible;
            CardViewScrollViewer.Visibility = Visibility.Collapsed;
            ToggleViewButton.Content = "Card View";
        }

        private void RequestViewing(Car car)
        {
            MessageBox.Show($"Viewing request for {car.Make} {car.Model} submitted!");
        }

        private void GeneralEnquiry(Car car)
        {
            MessageBox.Show($"General enquiry for {car.Make} {car.Model} submitted!");
        }

        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            var addCarWindow = new AddCarWindow();
            addCarWindow.Owner = this;  // Set owner to center modal properly
            var result = addCarWindow.ShowDialog();

            if (result == true)
            {
                // Refresh your car list here
                LoadCars();
            }

        }
    }
}
