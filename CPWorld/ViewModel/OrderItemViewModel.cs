namespace CpWorld.ViewModel
{
    using System.ComponentModel.DataAnnotations;

    public class OrderItemViewModel
    {
        public int ItemId { get; set; }

        [Range(0, 500)]
        public int? Quantity { get; set; }
    }
}