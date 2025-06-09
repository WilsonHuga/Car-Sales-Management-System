using Car_Sales_Management_System.DataModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Car_Sales_Management_System.Views.Components
{
    public partial class SignupControl : UserControl
    {
        private readonly UserService _userService;
        public event EventHandler SignupSuccess;

        public SignupControl()
        {
            InitializeComponent();
            _userService = UserService.Instance;
            //_userService = new UserService();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string phone = PhoneTextBox.Text.Trim(); // Optional field

            // Required field check
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowStatus("All fields are required.", Brushes.Red);
                return;
            }

            // Name length check
            if (firstName.Length < 2 || lastName.Length < 2)
            {
                ShowStatus("First and Last names must be at least 2 characters long.", Brushes.Red);
                return;
            }

            // Email format validation
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowStatus("Invalid email format.", Brushes.Red);
                return;
            }

            // Password strength validation
            if (password.Length < 6 ||
                !password.Any(char.IsUpper) ||
                !password.Any(char.IsLower) ||
                !password.Any(char.IsDigit))
            {
                ShowStatus("Password must be at least 6 characters and include uppercase, lowercase, and a digit.", Brushes.Red);
                return;
            }

            // Optional: Phone number validation
            //if (!string.IsNullOrWhiteSpace(phone) && !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+?\d{7,15}$"))
            //{
            //    ShowStatus("Invalid phone number format.", Brushes.Red);
            //    return;
            //}

            // Check for existing user
            var existingUser = _userService.GetUserByEmail(email);
            if (existingUser != null)
            {
                ShowStatus("Email already registered.", Brushes.Red);
                return;
            }

            var user = new User
            {
                Fullname = $"{firstName} {lastName}",
                Email = email,
                Password = password,
                Phone = phone,
                Role = "User" // Ensure enforced even if class default is changed
            };

            try
            {
                _userService.AddUser(user);

                // Set the current user after successful registration
                var registeredUser = _userService.GetUserByEmail(email);
                _userService.SetCurrentUser(registeredUser);

                ShowStatus("Registration successful!", Brushes.Green);
                SignupSuccess?.Invoke(this, EventArgs.Empty);
                ClearFields();
            }
            catch
            {
                ShowStatus("Registration failed. Try again.", Brushes.Red);
            }
        }

        // Utility to show status
        private void ShowStatus(string message, Brush color)
        {
            StatusTextBlock.Foreground = color;
            StatusTextBlock.Text = message;
        }


        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            StatusTextBlock.Text = "";
        }

        private void ClearFields()
        {
            FirstNameTextBox.Text = "";
            LastNameTextBox.Text = "";
            EmailTextBox.Text = "";
            PhoneTextBox.Text = "";
            PasswordBox.Clear();
        }

        private void SwitchToLogin_Click(object sender, RoutedEventArgs e)
        {
            // Find the parent LoginSignupWrapper and show LoginControl
            var wrapper = FindParent<LoginSignupWrapper>(this);
            wrapper?.ShowLogin();
        }

        // Helper method to find parent of a specific type
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }
    }
}