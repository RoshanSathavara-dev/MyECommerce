using System.ComponentModel.DataAnnotations;

namespace MyECommerce.Models
{
    public class Contact
    {
        public int Id { get; set; } // Primary Key

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
