namespace CpWorld.Models
{
    public class Item
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantityAvailable { get; set; }
        public int ItemId { get; set; }
        public decimal Price { get; set; }
    }
}

