using System.ComponentModel.DataAnnotations;

namespace MyECommerce.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public required string UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? EstimatedDeliveryDate { get; set; }

        // Navigation Property

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
