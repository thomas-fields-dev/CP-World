namespace CpWorld.ViewModel
{
    using CpWorld.Models;

    public class CreateViewModel : Order
    {
        public List<OrderItemViewModel>? OrderedItems { get; set; }

        public string Response { get; set; } = string.Empty;
    }
}