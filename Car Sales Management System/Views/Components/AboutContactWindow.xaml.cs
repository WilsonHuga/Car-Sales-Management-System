using System.Windows;

namespace Car_Sales_Management_System.Views.Components
{
    public partial class AboutContactWindow : Window
    {
        public AboutContactWindow()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
