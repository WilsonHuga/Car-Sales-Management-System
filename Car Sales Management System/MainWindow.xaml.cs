using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

using Car_Sales_Management_System.DataModels;

namespace Car_Sales_Management_System
{
    public partial class MainWindow : Window
    {
        private CarService? _carService;
        public ObservableCollection<Car> Cars { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _carService = new CarService();
            Cars = new ObservableCollection<Car>();

            LoadCars();
        }

        private void LoadCars()
        {
            try
            {
                List<Car> cars;

                if (_carService != null)
                {
                    cars = _carService.GetAllCars();
                    if (cars == null)
                    {
                        cars = new List<Car>();
                    }
                }
                else
                {
                    cars = new List<Car>();
                }

                MessageBox.Show($"Loaded {cars.Count} cars from DB");
                Cars = new ObservableCollection<Car>(cars);
                CarGrid.ItemsSource = Cars;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading cars: " + ex.Message);
            }
        }

    }
}
