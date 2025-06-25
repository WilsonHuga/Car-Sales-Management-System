using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Car_Sales_Management_System.DataModels;

namespace Car_Sales_Management_System.Views
{
    // Converter for null values
    public class NullToEmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class InquiryViewModel
    {
        public Inquiry Inquiry { get; }
        public string CarMakeModel { get; }
        public string CarDetails { get; }
        public string UserDetails { get; }

        public InquiryViewModel(Inquiry inquiry, CarService carService, UserService userService)
        {
            Inquiry = inquiry ?? throw new ArgumentNullException(nameof(inquiry));
            Car car = carService.GetCarById(inquiry.CarId.ToString());
            User user = userService.GetUserById(inquiry.UserId.ToString());
            CarMakeModel = car != null ? $"{car.Make} {car.Model}" : "Unknown Car";
            UserDetails = user != null ? user.Fullname : "Unknown User";
        }
    }

    public partial class ManageInquiriesWindow : Window
    {
        private readonly UserService _userService;
        private readonly InquiryService _inquiryService;
        private readonly CarService _carService;
        //public ObservableCollection<Inquiry> Inquiries { get; set; }
        public ObservableCollection<InquiryViewModel> Inquiries { get; set; }
        private InquiryViewModel _selectedInquiry;

        public ManageInquiriesWindow(UserService userService)
        {
            InitializeComponent();
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _inquiryService = new InquiryService();
            _carService = new CarService();

            Inquiries = new ObservableCollection<InquiryViewModel>();
            LoadInquiries();
            InquiriesDataGrid.ItemsSource = Inquiries;
            DataContext = this;
            //LoadInquiries();
        }

        private void LoadInquiries()
        {
            try
            {
                Inquiries.Clear();
                var inquiries = _userService.IsAdmin
                    ? _inquiryService.GetAllInquiries()
                    : _inquiryService.GetInquiriesByUserId(_userService.CurrentUser.Id.ToString());

                if (inquiries == null || inquiries.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No inquiries found or null result.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Loaded {inquiries.Count} inquiries.");
                    foreach (var inquiry in inquiries)
                    {
                        Inquiries.Add(new InquiryViewModel(inquiry, _carService, _userService));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading inquiries: {ex.Message}");
                MessageBox.Show("Error loading inquiries: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInquiry == null)
            {
                MessageBox.Show("Please select an inquiry to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Show the edit panel and populate the ComboBox with the current status
            EditPanel.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            EditButton.Visibility = Visibility.Collapsed;
            StatusComboBox.SelectedItem = new ComboBoxItem { Content = _selectedInquiry.Inquiry.Status };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInquiry != null && StatusComboBox.SelectedItem != null)
            {
                var newStatus = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();
                var updatedInquiry = new Inquiry
                {
                    Id = _selectedInquiry.Inquiry.Id,
                    CarId = _selectedInquiry.Inquiry.CarId,
                    UserId = _selectedInquiry.Inquiry.UserId,
                    Name = _selectedInquiry.Inquiry.Name,
                    Email = _selectedInquiry.Inquiry.Email,
                    Phone = _selectedInquiry.Inquiry.Phone,
                    Message = _selectedInquiry.Inquiry.Message,
                    InquiryDate = _selectedInquiry.Inquiry.InquiryDate,
                    Status = newStatus
                };

                if (_inquiryService.UpdateInquiry(_selectedInquiry.Inquiry.Id.ToString(), updatedInquiry))
                {
                    LoadInquiries(); // Refresh the DataGrid
                    MessageBox.Show("Inquiry status updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to update inquiry status.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Hide the edit panel and restore button states
            EditPanel.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the edit panel and restore button states without saving
            EditPanel.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Visible;
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (InquiriesDataGrid.SelectedItem is InquiryViewModel selectedInquiryViewModel)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the inquiry for {selectedInquiryViewModel.CarDetails} by {selectedInquiryViewModel.UserDetails}?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (_inquiryService.DeleteInquiry(selectedInquiryViewModel.Inquiry.Id.ToString()))
                        {
                            var inquiryToRemove = Inquiries.FirstOrDefault(i => i.Inquiry.Id == selectedInquiryViewModel.Inquiry.Id);
                            if (inquiryToRemove != null)
                            {
                                Inquiries.Remove(inquiryToRemove); // Update UI directly
                            }
                            MessageBox.Show("Inquiry deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete inquiry.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting inquiry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an inquiry to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadInquiries();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InquiriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedInquiry = InquiriesDataGrid.SelectedItem as InquiryViewModel;
        }
    }
}
