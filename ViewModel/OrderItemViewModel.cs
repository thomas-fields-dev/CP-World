using System.ComponentModel.DataAnnotations;

namespace CpWorld.ViewModel
{
    public class OrderItemViewModel
    {
        public int ItemId { get; set; }
        [Range(0,100)]
        public int? Quantity { get; set; }
    }
}

