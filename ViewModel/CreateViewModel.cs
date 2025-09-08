using CpWorld.Models;

namespace CpWorld.ViewModel
{
    public class CreateViewModel : Order
    {
        public List<OrderItemViewModel>? OrderedItems { get; set; }
        public string Response { get; set; } = string.Empty;
    }
}

