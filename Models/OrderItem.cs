using System.ComponentModel.DataAnnotations;

namespace CpWorld.Models
{
    //OrderItem
    //Validation: [Range(1, 100)] for Quantity.
    //Remove raw ProductName + Price duplication, instead add:
    //ItemId(FK to Item table).
    //Navigation: Item(so OrderItem links to catalog items).
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        [Range(0, 100)]
        public int Quantity { get; set; }

        public Item? Item { get; set; }
    }
}

