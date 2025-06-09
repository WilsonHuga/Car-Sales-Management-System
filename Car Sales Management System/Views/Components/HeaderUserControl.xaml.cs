using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Car_Sales_Management_System.DataModels;

namespace Car_Sales_Management_System.Views.Components
{
    public partial class HeaderUserControl : UserControl
    {
        private readonly UserService _userService;

        public HeaderUserControl()
        {
            InitializeComponent();
            _userService = UserService.Instance;
            UpdateMenuItemsForRole(_userService.IsLoggedIn, _userService.IsAdmin);
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsUserLoggedIn())
            {
                MessageBox.Show("User is already logged in.", "Profile", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var loginWindow = new Window
            {
                Title = "Login or Sign Up",
                Content = new LoginSignupWrapper(),
                Width = 450,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            loginWindow.ContentRendered += (s, args) =>
            {
                if (loginWindow.Content is LoginSignupWrapper wrapper)
                {
                    // Subscribe to the LoginSuccess event of the LoginControl
                    wrapper.Login.LoginSuccess += (s2, e2) =>
                    {
                        if (IsUserLoggedIn())
                        {
                            loginWindow.Close();
                        }
                    };
                }
            };

            loginWindow.ShowDialog();
        }

        private bool IsUserLoggedIn()
        {
            return _userService.CurrentUser != null;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (!IsUserLoggedIn())
            {
                MessageBox.Show("No user is currently logged in.", "Logout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ShowLogoutConfirmation();
        }

        private void ShowLogoutConfirmation()
        {
            var result = MessageBox.Show("Are you sure you want to log out?", "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _userService.Logout();
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.ShowLoginSignup();
            }
        }

        // important ****** 
        private void UpdateMenuItemsForRole(bool isLoggedIn, bool isAdmin)
        {
            if (isLoggedIn && isAdmin)
            {
                EditCarMenuItem.IsEnabled = true;
                DeleteCarMenuItem.IsEnabled = true;
            }
            else
            {
                EditCarMenuItem.IsEnabled = false;
                DeleteCarMenuItem.IsEnabled = false;
            }
        }

        private void AddCar_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;

            var addCarWindow = new AddCarWindow();
            addCarWindow.Owner = mainWindow;
            var result = addCarWindow.ShowDialog();

            if (result == true)
            {
                mainWindow.LoadCars();
            }
        }

        //private void EditCar_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!_userService.IsAdmin)
        //    {
        //        MessageBox.Show("Only administrators can edit cars.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    var mainWindow = Window.GetWindow(this) as MainWindow;
        //    if (mainWindow == null) return;

        //    var selectedCar = mainWindow.CarGrid.SelectedItem as Car;
        //    if (selectedCar == null)
        //    {
        //        MessageBox.Show("Please select a car to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    var editCarWindow = new EditCarWindow(selectedCar);
        //    editCarWindow.Owner = mainWindow;
        //    var result = editCarWindow.ShowDialog();

        //    if (result == true)
        //    {
        //        mainWindow.LoadCars();
        //    }
        //}

        private void DeleteCar_Click(object sender, RoutedEventArgs e)
        {
            if (!_userService.IsAdmin)
            {
                MessageBox.Show("Only administrators can delete cars.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;

            var selectedCar = mainWindow.CarGrid.SelectedItem as Car;
            if (selectedCar == null)
            {
                MessageBox.Show("Please select a car to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete {selectedCar.Make} {selectedCar.Model}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var carService = new CarService();
                bool deleted = carService.DeleteCar(selectedCar.Id.ToString());
                if (deleted)
                {
                    mainWindow.LoadCars();
                    MessageBox.Show("Car deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to delete car.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ToggleViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;

            var menuItem = sender as MenuItem;
            if (menuItem?.Header.ToString() == "Card View")
            {
                mainWindow.CarGrid.Visibility = Visibility.Collapsed;
                mainWindow.CardViewScrollViewer.Visibility = Visibility.Visible;
                menuItem.Header = "Table View";
            }
            else
            {
                mainWindow.CarGrid.Visibility = Visibility.Visible;
                mainWindow.CardViewScrollViewer.Visibility = Visibility.Collapsed;
                menuItem.Header = "Card View";
            }
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.LoadCars();
        }
    }
}
