using System.ComponentModel.DataAnnotations;

namespace MyECommerce.Models
{
    public class LoginViewModel
    {
        [Required]
        public string EmailOrContactNo { get; set; } = string.Empty;

        [Required]
        [StringLength(6, ErrorMessage = "OTP must be 6 digits.", MinimumLength = 6)]
        public string OTP { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
    }
}
