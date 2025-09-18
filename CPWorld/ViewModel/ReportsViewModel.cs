using CpWorld.Models;

namespace CpWorld.ViewModel
{
    public class ReportsViewModel
    {
        public ReportsViewModel()
        {
            Reports = new Dictionary<int, string>()
            {
                { 1, "Top 5 customers by spending" },
                { 2, "Most sold products" },
                { 3, "Customers with no orders" }
            };
        }
        public string TotalResults { get; set; } = string.Empty;
        public int SelectedReport { get; set; }
        public Dictionary<int, string> Reports { get; set; } = new Dictionary<int, string>();
        public object GeneratedReport { get; set; }
    }
}
