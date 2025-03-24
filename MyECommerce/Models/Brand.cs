using System.ComponentModel.DataAnnotations;

namespace MyECommerce.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Brand Name is required.")]
        [StringLength(100, ErrorMessage = "Brand Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
