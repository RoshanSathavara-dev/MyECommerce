using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyECommerce.Models
{
    public class ShoppingCartItem
    {
        [Key]
        public int Id { get; set; }


        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // ✅ Add CartId for session-based tracking
        public string? CartId { get; set; } = string.Empty;

        [ForeignKey("User")]
        public int? UserId { get; set; }
        public User? User { get; set; }

    }
}
