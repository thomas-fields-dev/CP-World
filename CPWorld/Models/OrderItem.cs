namespace CpWorld.Models
{
    using System.ComponentModel.DataAnnotations;

    public class OrderItem
    {
        public int OrderItemId { get; set; }

        [Range(0, 100)]
        public int Quantity { get; set; }

        public Item Item { get; set; } = new Item();
    }
}