using System.ComponentModel.DataAnnotations;

namespace BurgerJoint.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        public string DeliveryOption { get; set; } = "Collect"; // Collect or Deliver

        public string DeliveryAddress { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

        //  JSON blob: List<OrderItem>
        public string ItemsJson { get; set; } = "[]";
    }

    public class OrderItem
    {
        public int BurgerId { get; set; }
        public string BurgerName { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public bool AddBeer { get; set; }
    }
}