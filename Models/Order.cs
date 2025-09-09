using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CpWorld.Emums;

namespace CpWorld.Models
{
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
        public OrderStatus OrderStatus { get; set; }

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

    }    
}

