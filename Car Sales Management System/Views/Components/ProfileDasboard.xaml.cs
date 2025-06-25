using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Car_Sales_Management_System.DataModels;
using Microsoft.Win32;

namespace Car_Sales_Management_System.Views.Components
{
    public partial class ProfileDasboard : UserControl
    {
        private readonly UserService _userService;
        public ProfileDasboard()
        {
            InitializeComponent();
            _userService = UserService.Instance;
            DataContext = _userService.CurrentUser;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window?.Close();
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            var editProfileWindow = new Window
            {
                Title = "Edit Profile",
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            // Full Name
            stackPanel.Children.Add(new TextBlock { Text = "Full Name:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) });
            var fullNameTextBox = new TextBox { Text = _userService.CurrentUser.Fullname, Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(fullNameTextBox);

            // Email
            stackPanel.Children.Add(new TextBlock { Text = "Email:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) });
            var emailTextBox = new TextBox { Text = _userService.CurrentUser.Email, IsReadOnly = true, Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(emailTextBox);

            // Phone
            stackPanel.Children.Add(new TextBlock { Text = "Phone:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) });
            var phoneTextBox = new TextBox { Text = _userService.CurrentUser.Phone, Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(phoneTextBox);

            // Profile Picture
            stackPanel.Children.Add(new TextBlock { Text = "Profile Picture:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) });
            var uploadButton = new Button { Content = "Upload New Picture", Width = 150, Margin = new Thickness(0, 0, 0, 10) };
            byte[] newProfilePicture = null;
            uploadButton.Click += (s, args) =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        newProfilePicture = File.ReadAllBytes(openFileDialog.FileName);
                        MessageBox.Show("Profile picture selected.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            };
            stackPanel.Children.Add(uploadButton);

            // Save and Cancel Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };
            var saveButton = new Button { Content = "Save", Width = 80, Margin = new Thickness(0, 0, 10, 0) };
            saveButton.Click += (s, args) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(fullNameTextBox.Text) || string.IsNullOrWhiteSpace(phoneTextBox.Text))
                    {
                        MessageBox.Show("Full Name and Phone cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    bool updated = _userService.UpdateUserDetails(_userService.CurrentUser.Email, fullNameTextBox.Text, phoneTextBox.Text);
                    if (newProfilePicture != null)
                    {
                        updated |= _userService.UpdateProfilePicture(_userService.CurrentUser.Email, newProfilePicture);
                    }

                    if (updated)
                    {
                        MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        DataContext = _userService.CurrentUser;
                        editProfileWindow.Close();
                    }
                    else
                    {
                        MessageBox.Show("No changes were made.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            var cancelButton = new Button { Content = "Cancel", Width = 80 };
            cancelButton.Click += (s, args) => editProfileWindow.Close();
            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            editProfileWindow.Content = stackPanel;
            editProfileWindow.ShowDialog();
        }

        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = _userService.CurrentUser;
                string data = $"Profile Data\n\n" +
                              $"Full Name: {user.Fullname}\n" +
                              $"Email: {user.Email}\n" +
                              $"Role: {user.Role}\n" +
                              $"Phone: {user.Phone}\n" +
                              $"Cars Viewed: {user.CarsViewed}\n" +
                              $"Inquiries Made: {user.InquiriesMade}\n";

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt",
                    FileName = $"{user.Fullname}_Profile.txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, data);
                    MessageBox.Show("Profile data exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to delete your account? This action cannot be undone.",
                                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool deleted = _userService.DeleteUser(_userService.CurrentUser.Email);
                    if (deleted)
                    {
                        MessageBox.Show("Account deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        var window = Window.GetWindow(this);
                        window?.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete account.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting account: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string theme = selectedItem.Content.ToString();
                // Placeholder for theme application logic
                MessageBox.Show($"Theme changed to {theme}. (Theme application not fully implemented)",
                                "Theme Changed", MessageBoxButton.OK, MessageBoxImage.Information);
                // In a real application, you would apply the theme by updating Application.Current.Resources
                // or using a theme manager. For simplicity, we show a message.
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string language = selectedItem.Content.ToString();
                // Placeholder for language application logic
                MessageBox.Show($"Language changed to {language}. (Language application not fully implemented)",
                                "Language Changed", MessageBoxButton.OK, MessageBoxImage.Information);
                // In a real application, you would load the appropriate resource dictionary
                // or update culture settings (e.g., Thread.CurrentThread.CurrentUICulture).
            }
        }
    }

    // Converter for byte array to BitmapImage
    public class ByteArrayToImageConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                try
                {
                    using var stream = new MemoryStream(bytes);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = stream;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error converting profile picture: {ex.Message}");
                }
            }

            try
            {
                var defaultImage = new BitmapImage();
                defaultImage.BeginInit();
                defaultImage.UriSource = new Uri("pack://application:,,,/Images/default_profile.png", UriKind.Absolute);
                defaultImage.EndInit();
                defaultImage.Freeze();
                return defaultImage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading default image: {ex.Message}");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported.");
        }
    }
}
