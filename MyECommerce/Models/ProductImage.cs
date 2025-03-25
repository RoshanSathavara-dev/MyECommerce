using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyECommerce.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public Product? Product { get; set; }
    }
}
