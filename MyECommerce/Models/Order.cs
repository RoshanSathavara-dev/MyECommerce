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

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string PinCode { get; set; } = string.Empty;

        // ✅ Payment Details
        [Required]
        public string PaymentMethod { get; set; } = "COD"; // COD, Card, UPI, Net Banking


        // Navigation Property

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
