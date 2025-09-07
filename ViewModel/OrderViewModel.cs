using CpWorld.Models;

namespace CpWorld.ViewModel
{
    public class OrderViewModel
    {
        public OrderViewModel()
        {
            Response = new List<Order>();
        }
        public ICollection<Order> Response;
    }
}

