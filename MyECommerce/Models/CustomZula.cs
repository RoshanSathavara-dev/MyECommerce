using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyECommerce.Models
{
    public class CustomZula
    {
        [Key]
        public int Id { get; set; }

        public string? ImageUrl { get; set; } // ✅ Renamed from 'Image' to 'ImageUrl'


        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number must be exactly 10 digits.")]
        public long ContactNo { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }


        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // ✅ Track the user who made the request
        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; } // ✅ Navigation Property (Optional, if using Identity)

        // ✅ Add Status Field (Pending, Approved, Rejected)
        [Required]
        public string Status { get; set; } = "Pending"; // ✅ Default value
    }
}
