using Microsoft.AspNetCore.Http.Features;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace CpWorld.Models
{
    //Order
    //Add[Required], [StringLength] validation attributes.
    //Add computed property → TotalAmount (sum of item prices).
    //Add status field: OrderStatus(enum: Pending, Shipped, Cancelled).

    public class Order
    {
        public Order()
        {
            OrderItems = new List<OrderItem>();
        }
        public int OrderId { get; set; }
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }

        [NotMapped]
        public decimal TotalAmount
        {
            get
            {
                decimal totalAmount = 0m;
                foreach (var oi in OrderItems)
                {
                    var price = oi.Item?.Price;

                    if (price != null)
                    {
                        totalAmount += (decimal)price * oi.Quantity;
                    }
                    else
                    {
                        totalAmount = 0m;
                    }
                }
                return totalAmount;
            }
        }

        [NotMapped]
        public OrderStatus orderStatus { get; set; }
    }

    public class OrderItem
    {
        public int OrderItemId { get; set; }

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

    public enum OrderStatus
    {
        Pending, 
        Shipped, 
        Cancelled
    }
}

