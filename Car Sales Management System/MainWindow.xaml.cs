using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Car_Sales_Management_System.DataModels;
using Car_Sales_Management_System.Views;
using Car_Sales_Management_System.Views.Components;

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
        private readonly UserService _userService;
        private Window? _loginWindow;

        public ICommand RequestViewingCommand { get; }
        public ICommand GeneralEnquiryCommand { get; }
        public ICommand LogoutCommand { get; }


        public MainWindow()
        {
            InitializeComponent();
            _carService = new CarService();
            Cars = new ObservableCollection<Car>();
            DataContext = this; // Set DataContext for binding

            //Singleton instance
            _userService = UserService.Instance;

            // Subscribe to user changes
            _userService.UserChanged += OnUserChanged;

            // Initialize commands
            RequestViewingCommand = new RelayCommand<Car>(RequestViewing);
            GeneralEnquiryCommand = new RelayCommand<Car>(GeneralEnquiry);
            LogoutCommand = new RelayCommand<object>(_ => Logout());

            CarGrid.ItemsSource = Cars; // Bind the DataGrid to the Cars collection
            CarFilterControl.FilterChanged += ApplyFilters;

            this.Loaded += MainWindow_Loaded;
            //_carService.InitializePhotosForAllCars();
            //LoadCars();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_userService.IsLoggedIn)
                {
                    ShowMainMenu();
                }
                else
                {
                    ShowLoginSignup();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during initialization: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnUserChanged(object sender, UserService.UserChangedEventArgs e)
        {
            // Handle user login/logout changes
            if (e.IsLoggedIn)
            {
                ShowMainMenu();
            }
            else
            {
                ShowLoginSignup();
            }
        }

        public void LoadCars()
        {
            try
            {
                List<Car> cars = _carService?.GetAllCars() ?? new List<Car>();

                _allCars = cars; // Save for filtering
                //_allCars = cars.Select(c => { c.Photos ??= new List<string>(); return c; }).ToList();
               

                Cars.Clear();
                foreach (var car in cars)
                {
                    Cars.Add(car);
                }

                CarFilterControl.LoadFilterOptions(cars);

                //MessageBox.Show($"Loaded {cars.Count} cars from DB");
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
            if (!_userService.IsLoggedIn)
            {
                MessageBox.Show("Please log in to request a viewing.", "Login Required", MessageBoxButton.OK, MessageBoxImage.Information);
                ShowLoginSignup();
                return;
            }

            try
            {
                // Increment the CarsViewed counter for the current user
                _userService.IncrementCarsViewed(_userService.CurrentUser.Email);

                MessageBox.Show($"Viewing request for {car.Make} {car.Model} submitted by {_userService.CurrentUser.Fullname}! Cars Viewed: {_userService.CurrentUser.CarsViewed}",
                    "Viewing Request", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing viewing request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GeneralEnquiry(Car car)
        {
            if(!_userService.IsLoggedIn)
    {
                MessageBox.Show("Please log in to make an enquiry.", "Login Required", MessageBoxButton.OK, MessageBoxImage.Information);
                ShowLoginSignup();
                return;
            }
            if (car == null)
            {
                MessageBox.Show("No car selected for inquiry.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                var inquiryWindow = new InquiryWindow(car, _userService.CurrentUser);
                inquiryWindow.Owner = this;
                var result = inquiryWindow.ShowDialog();
                if (result == true)
                {
                    MessageBox.Show($"Inquiry for {car.Make} {car.Model} submitted successfully by {_userService.CurrentUser.Fullname}!",
                        "Inquiry Submitted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening inquiry window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_userService.IsAdmin)
            {
                MessageBox.Show("Only administrators can add cars.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addCarWindow = new AddCarWindow();
            addCarWindow.Owner = this;  // Set owner to center modal properly
            var result = addCarWindow.ShowDialog();

            if (result == true)
            {
                LoadCars(); // Refresh car list
            }

        }

        private void EditCarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_userService.IsAdmin)
            {
                MessageBox.Show("Only administrators can edit cars.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CarGrid.SelectedItem is Car selectedCar)
            {
                try
                {
                    // Open the EditCarWindow, passing the selected car and CarService
                    var editCarWindow = new EditCarWindow(selectedCar, _carService);
                    editCarWindow.Owner = this; // Set owner to center modal properly
                    var result = editCarWindow.ShowDialog();

                    // If the edit was successful (dialog result is true), refresh the car list
                    if (result == true)
                    {
                        LoadCars(); // Refresh the car list to reflect changes
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening edit window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a car to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


        }

        private void DeleteCarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_userService.IsAdmin)
            {
                MessageBox.Show("Only administrators can delete cars.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CarGrid.SelectedItem is Car selectedCar)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selectedCar.Make} {selectedCar.Model}?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    bool deleted = _carService.DeleteCar(selectedCar.Id.ToString());
                    if (deleted)
                    {
                        LoadCars();
                        MessageBox.Show("Car deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete car.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a car to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCars();
        }

        private void ManageUsersButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_userService.IsAdmin)
            {
                MessageBox.Show("Only administrators can manage users.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var manageUsersWindow = new ManageUsersWindow(_userService);
                manageUsersWindow.Owner = this;
                manageUsersWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening user management: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageInquiriesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_userService.IsAdmin)
            {
                MessageBox.Show("Only administrators can manage inquiries.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var manageInquiriesWindow = new ManageInquiriesWindow(_userService);
                manageInquiriesWindow.Owner = this;
                manageInquiriesWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening inquiry management: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_userService.IsAdmin)
                {
                    MessageBox.Show("Only administrators can view reports.", "Access Denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get all cars from the service
                var cars = _carService?.GetAllCars() ?? new System.Collections.Generic.List<Car>();

                // Calculate basic statistics
                int totalCars = cars.Count;
                int availableCars = cars.Count(c => c.Status.Equals("Available", StringComparison.OrdinalIgnoreCase));
                int reservedCars = cars.Count(c => c.Status.Equals("Reserved", StringComparison.OrdinalIgnoreCase));
                int soldCars = cars.Count(c => c.Status.Equals("Sold", StringComparison.OrdinalIgnoreCase));
                int newCars = cars.Count(c => c.Condition.Equals("New", StringComparison.OrdinalIgnoreCase));
                int usedCars = cars.Count(c => c.Condition.Equals("Used", StringComparison.OrdinalIgnoreCase));

                // Calculate average price (handle empty list to avoid division by zero)
                double averagePrice = totalCars > 0 ? cars.Average(c => c.Price) : 0;

                // Find oldest and newest listing dates
                string oldestListing = cars.Any() ? cars.Min(c => c.ListedDate).ToString("d MMM yyyy") : "N/A";
                string newestListing = cars.Any() ? cars.Max(c => c.ListedDate).ToString("d MMM yyyy") : "N/A";

                // Build the report message
                string report = "Car Inventory Report\n\n" +
                    $"Total Cars: {totalCars}\n" +
                    $"Available: {availableCars}\n" +
                    $"Reserved: {reservedCars}\n" +
                    $"Sold: {soldCars}\n" +
                    $"New: {newCars}\n" +
                    $"Used: {usedCars}\n" +
                    $"Average Price: ${averagePrice:N2}\n" +
                    $"Oldest Listing: {oldestListing}\n" +
                    $"Newest Listing: {newestListing}";

                // Display the report in a message box
                MessageBox.Show(report, "Car Inventory Report", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating report: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout()
        {
            _userService.Logout();
            // The OnUserChanged event will handle showing the login screen
        }

        public void ShowMainMenu()
        {
            if (_loginWindow != null)
            {
                _loginWindow.Close();
                _loginWindow = null;
            }

            // Ensure MainMenuControl is visible
            MainMenuControl.Visibility = Visibility.Visible;

            // Load cars for the main menu
            LoadCars();

            UpdateUIForUserRole();

            this.Title = $"Car Sales Management System - Welcome {_userService.CurrentUser.Fullname} ({_userService.CurrentUser.Role})";

            //// Optional: Customize UI based on user role
            //if (_userService.CurrentUser?.Role == "Admin")
            //{
            //    // Example: Show admin-specific UI elements
            //    // You can add admin-specific controls in XAML and toggle their visibility here
            //    MessageBox.Show("Logged in as Admin", "Welcome", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
            //else
            //{
            //    // Example: Show user-specific UI elements
            //    MessageBox.Show("Logged in as User", "Welcome", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
        }

        private void UpdateUIForUserRole()
        {
            if (_userService.IsLoggedIn)
            {
                RefreshButton.Visibility = Visibility.Visible;
                ToggleViewButton.Visibility = Visibility.Visible;
            }
            else
            {
                RefreshButton.Visibility = Visibility.Collapsed;
                ToggleViewButton.Visibility = Visibility.Collapsed;
            }

            if (_userService.IsAdmin)
            {
                AddCarButton.Visibility = Visibility.Visible;
                EditCarButton.Visibility = Visibility.Visible;
                DeleteCarButton.Visibility = Visibility.Visible;
                RefreshButton.Visibility = Visibility.Visible;
                ToggleViewButton.Visibility = Visibility.Visible;


                ManageUsersButton.Visibility = Visibility.Visible;
                ManageInquiriesButton.Visibility = Visibility.Visible;
                ReportsButton.Visibility = Visibility.Visible;


            }
            else
            {
                AddCarButton.Visibility = Visibility.Collapsed;
                EditCarButton.Visibility = Visibility.Collapsed;
                DeleteCarButton.Visibility = Visibility.Collapsed;

                RefreshButton.Visibility = Visibility.Visible;
                ToggleViewButton.Visibility = Visibility.Visible;

                ManageUsersButton.Visibility = Visibility.Collapsed;
                ManageInquiriesButton.Visibility = Visibility.Collapsed;
                ReportsButton.Visibility = Visibility.Collapsed;
            }
        }


        public void ShowLoginSignup()
        {
            // Hide main menu and show login/signup
            MainMenuControl.Visibility = Visibility.Collapsed;
            this.Title = "Car Sales Management System - Please Login";


            try
            {
                if (_loginWindow == null)
                {
                    _loginWindow = new Window
                    {
                        Title = "Login or Sign Up",
                        Content = new LoginSignupWrapper(),
                        Width = 450,
                        Height = 500,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        ResizeMode = ResizeMode.NoResize,
                        Owner = this // This will now work because MainWindow is shown
                    };

                    _loginWindow.Closed += (s, e) =>
                    {
                        _loginWindow = null;
                        if (!_userService.IsLoggedIn)
                        {
                            if (MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                Application.Current.Shutdown();
                            }
                            else
                            {
                                // Re-show the login window if they choose not to exit
                                ShowLoginSignup();
                            }
                        }
                    };

                    _loginWindow.ContentRendered += (s, args) =>
                    {
                        if (_loginWindow.Content is LoginSignupWrapper wrapper)
                        {
                            wrapper.Login.LoginSuccess += (s2, e2) => { };
                            wrapper.SignUp.SignupSuccess += (s2, e2) => { };
                        }
                    };

                    _loginWindow.Show();
                }
                else
                {
                    _loginWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening login window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events
            _userService.UserChanged -= OnUserChanged;
            _loginWindow?.Close();

            Application.Current.Shutdown();
            base.OnClosed(e);
        }
    }
}
