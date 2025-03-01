using System.ComponentModel.DataAnnotations;

namespace MyECommerce.Models
{
    public class CustomZula
    {
        [Key]
        public int Id { get; set; }

        public string? ImageUrl { get; set; } // ✅ Renamed from 'Image' to 'ImageUrl'


        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number must be exactly 10 digits.")]
        public long ContactNo { get; set; }


        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
