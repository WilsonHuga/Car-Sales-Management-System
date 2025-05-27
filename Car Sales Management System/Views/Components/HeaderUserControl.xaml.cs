using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace Car_Sales_Management_System.Views.Components
{
    public partial class HeaderUserControl : UserControl
    {
        public HeaderUserControl()
        {
            InitializeComponent();
            DataContext = this; // Set DataContext for binding
        }


        //private string? _searchText;
        //public string SearchText
        //{
        //    get => _searchText;
        //    set { _searchText = value; OnPropertyChanged(); }
        //}

        //public string ApplicationTitle { get; set; } = "AutoMart";
        //public ICommand? NewInventoryCommand { get; set; }
        //public ICommand? OpenDatabaseCommand { get; set; }
        //public ICommand? ExitCommand { get; set; }
        //public ICommand? AddNewCarCommand { get; set; }
        //public ICommand? UpdatePricingCommand { get; set; }
        //public ICommand? UserManualCommand { get; set; }
        //public ICommand? ContactSupportCommand { get; set; }

        //public event PropertyChangedEventHandler? PropertyChanged;
        //protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

    }
}
