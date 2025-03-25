using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyECommerce.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(100, ErrorMessage = "Product Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty; // Main Image
        [Range(1, int.MaxValue, ErrorMessage = "Stock must be at least 1.")]
        public int Stock { get; set; }
        // Foreign Key for Category
        [Required(ErrorMessage = "Category is required.")]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // ✅ New Foreign Key for Brand


        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;

        public bool IsLimitedEdition { get; set; } = false;

        public bool ShowInHeroSection { get; set; } = false; // ✅ New property for Hero Section

        public bool IsFeatured { get; set; }

        [ForeignKey("Brand")]
        public int BrandId { get; set; }  // If you want it nullable, change it to `int?`
        public Brand? Brand { get; set; }

        public List<ProductImage> ProductImages { get; set; } = new List<ProductImage>();




    }
}
