namespace CpWorld.ViewModel
{
    using CpWorld.Enums;
    using CpWorld.Models;

    public class OrderViewModel
    {
        public ICollection<Order> Response;

        public OrderViewModel()
        {
            this.Response = new List<Order>();
        }

        public string SearchTerm { get; set; } = string.Empty;

        public int CurrentPage { get; set; }

        public string Message { get; set; } = string.Empty;

        public OrderStatus OrderStatus { get; set; }

        public List<int> Pages { get; set; } = new List<int>();
    }
}