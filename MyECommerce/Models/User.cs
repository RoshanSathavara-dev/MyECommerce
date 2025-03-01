using System.ComponentModel.DataAnnotations;

namespace MyECommerce.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string ContactNo { get; set; } = string.Empty;

        public string Otp { get; set; } = string.Empty;  // OTP Field
        public DateTime OtpGeneratedAt { get; set; }     // OTP Timestamp

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
