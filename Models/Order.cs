using Microsoft.AspNetCore.Http.Features;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CpWorld.Models
{
    public class Order
    {
        public Order()
        {
            Items = new List<OrderItem>();
            ItemCount = Items.Count;
        }
        public int OrderId { get; set; }
        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public ICollection<OrderItem> Items { get; set; }
        [NotMapped]
        public int ItemCount { get; set; }
    }

    public class OrderItem
    {
        public int OrderItemId { get; set; }
        [Required]
        [MaxLength(100)]
        //public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        
        public Item? Item { get; set; }
    }

    public class Item
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantityAvailable { get; set; }
        public int ItemId { get; set; }
        public decimal Price { get; set; }
    }

    public class OrderViewModel
    {
        public OrderViewModel()
        {
            Response = new List<Order>();
        }
        public ICollection<Order> Response;
    }

    public class DetailsViewModel
    {
        public DetailsViewModel()
        {
            Order = new Order();
        }
        public Order Order { get; set; }
    }

    public class CreateViewModel : Order
    {
        public List<OrderedItem>? OrderedItems { get; set; }
        public string Response { get; set; } = String.Empty;
    }
    public class OrderedItem
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
