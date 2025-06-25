using Car_Sales_Management_System.DataModels;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Car_Sales_Management_System.Views
{
    public partial class ManageUsersWindow : Window
    {
        private readonly UserService _userService;
        public ObservableCollection<User> Users { get; set; }
        public ObservableCollection<string> AvailableRoles { get; } = new ObservableCollection<string> { "User", "Admin" };

        public ManageUsersWindow(UserService userService)
        {
            InitializeComponent();
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            Users = new ObservableCollection<User>();
            DataContext = new { Users = _userService.GetAllUsers() };
            //UsersDataGrid.ItemsSource = _userService.GetAllUsers();
            //DataContext = this;
            UsersDataGrid.ItemsSource = Users;
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                Users.Clear();
                var users = _userService.GetAllUsers();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddUserWindow(_userService);
            addWindow.Owner = this;
            if (addWindow.ShowDialog() == true && addWindow.CreatedUser != null)
            {
                // Ensure the user is added to the database (already done in AddUserWindow)
                Users.Add(addWindow.CreatedUser); // Instantly update the ObservableCollection
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                if (selectedUser.Email == "admin@gmail.com")
                {
                    MessageBox.Show("The default admin account cannot be modified.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var editWindow = new EditUserWindow(selectedUser, _userService);
                editWindow.Owner = this;
                var result = editWindow.ShowDialog();
                if (result == true)
                {
                    LoadUsers(); // Refresh the list
                }
            }
            else
            {
                MessageBox.Show("Please select a user to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                if (selectedUser.Email == "admin@gmail.com")
                {
                    MessageBox.Show("The default admin account cannot be deleted.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Are you sure you want to delete user {selectedUser.Fullname} ({selectedUser.Email})?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool deleted = _userService.DeleteUser(selectedUser.Email);
                        if (deleted)
                        {
                            Users.Remove(selectedUser); // Update ObservableCollection directly
                            MessageBox.Show("User deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete user. The user may not exist or there was an issue with the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a user to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}