using CpWorld.Models;

namespace CpWorld.ViewModel
{
    public class CreateViewModel : Order
    {
        public List<OrderedItem>? OrderedItems { get; set; }
        public string Response { get; set; } = string.Empty;
    }
}

