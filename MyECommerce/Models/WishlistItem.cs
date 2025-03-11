using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyECommerce.Models
{
    public class WishlistItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public string UserId { get; set; } = string.Empty; // For user-based wishlist
    }
}
