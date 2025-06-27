using Car_Sales_Management_System.DataModels;
using MongoDB.Driver;
using System;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Car_Sales_Management_System.Views
{
    public partial class EditUserWindow : Window
    {
        private readonly User _user;
        private readonly UserService _userService;

        public EditUserWindow(User user, UserService userService)
        {
            InitializeComponent();
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            DataContext = _user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                // Prevent current user from changing their own role
                if (_user.Email == _userService.CurrentUser?.Email)
                {
                    MessageBox.Show("You cannot change your own role.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Update role if changed
                string newRole = RoleComboBox.SelectedValue?.ToString();
                bool roleUpdated = false;
                if (!string.IsNullOrEmpty(newRole) && newRole != (_user.Role ?? "User"))
                {
                    roleUpdated = _userService.UpdateUserRole(_user.Email, newRole);
                    if (!roleUpdated)
                    {
                        MessageBox.Show("Failed to update user role.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    _user.Role = newRole;
                }

                // Update other fields (Fullname and Phone)
                bool detailsUpdated = _userService.UpdateUserDetails(_user.Email, FullnameTextBox.Text, PhoneTextBox.Text);
                if (detailsUpdated)
                {
                    _user.Fullname = FullnameTextBox.Text;
                    _user.Phone = PhoneTextBox.Text;
                }

                // Update password if a new one is provided
                string newPassword = NewPasswordBox.Password;
                bool passwordUpdated = false;
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    passwordUpdated = _userService.UpdateUserPassword(_user.Email, newPassword);
                    if (!passwordUpdated)
                    {
                        MessageBox.Show("Failed to update user password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (detailsUpdated || roleUpdated || passwordUpdated)
                {
                    MessageBox.Show("User updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("No changes detected or update failed.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating user: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}