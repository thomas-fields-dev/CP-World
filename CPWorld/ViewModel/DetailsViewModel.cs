using CpWorld.Models;

namespace CpWorld.ViewModel
{
    public class DetailsViewModel
    {
        public DetailsViewModel()
        {
            Order = new Order();
        }
        public Order Order { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Disabled { get; set; } = string.Empty;
    }
}

