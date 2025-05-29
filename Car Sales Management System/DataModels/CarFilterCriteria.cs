using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Car_Sales_Management_System.DataModels
{
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
}
