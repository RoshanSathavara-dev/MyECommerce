using System.ComponentModel.DataAnnotations;

namespace MyECommerce.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
