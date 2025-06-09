using System;
using System.Collections.Generic;
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

namespace Car_Sales_Management_System.Views.Components
{
    public partial class LoginSignupWrapper : UserControl
    {
        public LoginSignupWrapper()
        {
            InitializeComponent();
            ShowLogin();
        }

        public void ShowLogin()
        {
            Login.Visibility = Visibility.Visible;
            SignUp.Visibility = Visibility.Collapsed;
        }

        public void ShowSignup()
        {
            Login.Visibility = Visibility.Collapsed;
            SignUp.Visibility = Visibility.Visible;
        }
    }
}
