using System;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows;
using Car_Sales_Management_System.DataModels;

namespace Car_Sales_Management_System.Views
{
    public partial class InquiryWindow : Window
    {
        private readonly Car _car;
        private readonly User _user;
        private readonly InquiryService _inquiryService;

        public InquiryWindow(Car car, User user)
        {
            InitializeComponent();
            _car = car ?? throw new ArgumentNullException(nameof(car));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _inquiryService = new InquiryService();
            DataContext = this;
        }

        public Car Car => _car;
        public User User => _user;

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var inquiry = new Inquiry
                {
                    CarId = _car.Id,
                    UserId = _user.Id,
                    Name = _user.Fullname,
                    Email = _user.Email,
                    Phone = PhoneTextBox.Text,
                    Message = MessageTextBox.Text,
                    InquiryDate = DateTime.UtcNow,
                    Status = "Pending"
                };

                _inquiryService.AddInquiry(inquiry);
                UserService.Instance.IncrementInquiriesMade(_user.Email);
                MessageBox.Show("Inquiry submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting inquiry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}