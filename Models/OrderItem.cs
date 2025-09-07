namespace CpWorld.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int Quantity { get; set; }

        public Item? Item { get; set; }
    }
}

