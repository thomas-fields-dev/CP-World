using CpWorld.Emums;
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
        public string SearchTerm { get; set; } = string.Empty;
        public OrderStatus OrderStatus { get; set; }

        public int CurrentPage { get; set; }
        public List<int> Pages { get; set; } = new List<int>();
        public string Message { get; set; } = string.Empty;
    }
}

