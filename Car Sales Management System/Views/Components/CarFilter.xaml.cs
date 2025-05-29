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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Car_Sales_Management_System.DataModels;
namespace Car_Sales_Management_System.Views.Components
{
    public partial class CarFilter : UserControl
    {
        public event Action<string>? SearchTextChanged;

        public event Action<CarFilterCriteria>? FilterChanged;



        public ObservableCollection<Car> Cars { get; set; }
        private List<Car> _allCars = new List<Car>();

        public CarFilter()
        {
            InitializeComponent();

            MakeComboBox.SelectionChanged += OnFilterChanged;
            ModelComboBox.SelectionChanged += OnFilterChanged;
            ColorComboBox.SelectionChanged += OnFilterChanged;
            ConditionComboBox.SelectionChanged += OnFilterChanged;
            YearComboBox.SelectionChanged += OnFilterChanged;
            MileageComboBox.SelectionChanged += OnFilterChanged;
            PriceRangeComboBox.SelectionChanged += OnFilterChanged;

            SearchTextBox.TextChanged += OnFilterChanged;
        }
        public void LoadFilterOptions(List<Car> cars)
        {
            _allCars = cars;

            MakeComboBox.ItemsSource = cars.Select(c => c.Make).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().OrderBy(m => m).ToList();
            ModelComboBox.ItemsSource = cars.Select(c => c.Model).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().OrderBy(m => m).ToList();
            ColorComboBox.ItemsSource = cars.Select(c => c.Color).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().OrderBy(m => m).ToList();
            ConditionComboBox.ItemsSource = cars.Select(c => c.Condition).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().OrderBy(m => m).ToList();
            YearComboBox.ItemsSource = cars.Select(c => c.Year.ToString()).Distinct().OrderByDescending(y => y).ToList();
            MileageComboBox.ItemsSource = cars.Select(c => c.Mileage.ToString()).Distinct().OrderBy(m => m).ToList();

            PriceRangeComboBox.ItemsSource = new List<string>
                    {
                        "0 - 10,000", "10,000 - 20,000", "20,000 - 30,000", "30,000 - 50,000", "50,000+"
                    };
        }

        private void OnFilterChanged(object? sender, EventArgs e)
        {
            var criteria = new CarFilterCriteria
            {
                Make = MakeComboBox.SelectedItem as string,
                Model = ModelComboBox.SelectedItem as string,
                Color = ColorComboBox.SelectedItem as string,
                Condition = ConditionComboBox.SelectedItem as string,
                Year = YearComboBox.SelectedItem as string,
                Mileage = MileageComboBox.SelectedItem as string,
                PriceRange = PriceRangeComboBox.SelectedItem as string,
                SearchText = SearchTextBox.Text?.Trim().ToLower() ?? ""
            };

            FilterChanged?.Invoke(criteria);
        }
        public class CarFilterCriteria
        {
            public string? Make { get; set; }
            public string? Model { get; set; }
            public string? Color { get; set; }
            public string? Condition { get; set; }
            public string? Year { get; set; }
            public string? Mileage { get; set; }
            public string? PriceRange { get; set; }
            public string? SearchText { get; set; }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnFilterChanged(sender, e);
        }

        //private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    string query = SearchTextBox.Text?.Trim().ToLower() ?? "";
        //    SearchTextChanged?.Invoke(query);
        //}
    }
}
