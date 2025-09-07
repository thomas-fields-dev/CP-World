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
    }
}

