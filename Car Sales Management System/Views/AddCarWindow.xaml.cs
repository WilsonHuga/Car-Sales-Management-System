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
using System.Windows.Shapes;
using Car_Sales_Management_System.Views.Components;

namespace Car_Sales_Management_System.Views
{
    /// <summary>
    /// Interaction logic for AddCarWindow.xaml
    /// </summary>
    public partial class AddCarWindow : Window
    {
        public AddCarWindow()
        {
            InitializeComponent();
            var addCarControl = this.Content as Addcar;
            if (addCarControl != null)
            {
                addCarControl.CarAdded += AddCarControl_CarAdded;
            }
        }

        private void AddCarControl_CarAdded(object sender, EventArgs e)
        {
            // Close the window when car is added successfully
            this.DialogResult = true; // optional, helps to check dialog result
            this.Close();
        }


    }
}
