using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyECommerce.Models
{
    public class GalleryImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
