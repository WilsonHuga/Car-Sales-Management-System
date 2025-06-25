using Car_Sales_Management_System.DataModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Car_Sales_Management_System.Views
{
    public partial class AddUserWindow : Window
    {
        private readonly UserService _userService;
        public User CreatedUser { get; private set; }

        public AddUserWindow(UserService userService)
        {
            InitializeComponent();
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string firstName = FirstNameTextBox.Text.Trim();
                string lastName = LastNameTextBox.Text.Trim();
                string email = EmailTextBox.Text.Trim();
                string phone = PhoneTextBox.Text.Trim();
                string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
                {
                    MessageBox.Show("Please fill in all required fields.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!_userService.IsValidEmail(email))
                {
                    MessageBox.Show("Invalid email format.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_userService.GetUserByEmail(email) != null)
                {
                    MessageBox.Show("A user with this email already exists.", "Duplicate Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newUser = new User
                {
                    Fullname = $"{firstName} {lastName}",
                    Email = email,
                    Phone = phone,
                    Role = role,
                    Password = "default123"
                };

                _userService.AddUser(newUser);
                CreatedUser = new User // Assign a new instance to CreatedUser to avoid reusing the same object
                {
                    Fullname = newUser.Fullname,
                    Email = newUser.Email,
                    Phone = newUser.Phone,
                    Role = newUser.Role,
                    Password = newUser.Password
                };

                //_userService.AddUser(newUser);
                CreatedUser = newUser; // Store the created user to return to ManageUsersWindow
                MessageBox.Show("User added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding user: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}