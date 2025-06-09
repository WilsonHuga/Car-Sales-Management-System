using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Car_Sales_Management_System.DataModels;

namespace Car_Sales_Management_System.Views.Components
{
    public partial class LoginControl : UserControl
    {
        public event EventHandler LoginSuccess; // Custom event for login success
        private readonly UserService _userService;

        public LoginControl()
        {
            InitializeComponent();
            _userService = UserService.Instance; // Singleton instance of UserService
            //_userService = new UserService();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;



            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                StatusTextBlock.Text = "Email and password are required.";
                return;
            }

            if (_userService.Login(email, password)) // Use the new Login method
            {
                var user = _userService.CurrentUser;
                StatusTextBlock.Foreground = Brushes.Green;
                StatusTextBlock.Text = $"Login successful! Welcome {user.Role}";

                // Clear fields after successful login
                EmailTextBox.Text = "";
                PasswordBox.Clear();
                StatusTextBlock.Text = "";

                LoginSuccess?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StatusTextBlock.Foreground = Brushes.Red;
                StatusTextBlock.Text = "Invalid email or password.";
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            EmailTextBox.Text = "";
            PasswordBox.Clear();
            StatusTextBlock.Text = "";
        }

        private void SwitchToSignup_Click(object sender, RoutedEventArgs e)
        {
            var wrapper = FindParent<LoginSignupWrapper>(this);
            wrapper?.ShowSignup();
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                StatusTextBlock.Foreground = Brushes.Red;
                StatusTextBlock.Text = "Please enter your email address.";
                return;
            }

            var user = _userService.GetUserByEmail(email);
            if (user == null)
            {
                StatusTextBlock.Foreground = Brushes.Red;
                StatusTextBlock.Text = "Email not found.";
                return;
            }

            string resetToken = _userService.GeneratePasswordResetToken(email);
            // In a real app, send email with reset link (e.g., http://yourapp.com/reset?token={resetToken})  
            // For demo, log the token  
            StatusTextBlock.Foreground = Brushes.Green;
            StatusTextBlock.Text = $"Password reset link sent to {email}. Check console for token.";
            Console.WriteLine($"Password reset token for {email}: {resetToken}");
        }


    }
}