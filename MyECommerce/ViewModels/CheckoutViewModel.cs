using MyECommerce.Models;
using System.ComponentModel.DataAnnotations;

namespace MyECommerce.ViewModels
{
    public class CheckoutViewModel
    {
        // ✅ Shipping Details
        [Required]
        public string FirstName { get; set; } = string.Empty; // ✅ First Name

        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string Phone { get; set; } = string.Empty;
        [Required] public string StreetAddress { get; set; } = string.Empty;
        [Required]
        public string Country { get; set; } = "India";
        [Required] public string State { get; set; } = string.Empty;
        [Required] public string City { get; set; } = string.Empty;


        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Pin Code must be 6 digits")]
        public string PinCode { get; set; } = string.Empty;
        public string? OrderNotes { get; set; }


        // ✅ Payment Details
        [Required] public string PaymentMethod { get; set; } = "COD"; // Cash on Delivery, UPI, Card

        // ✅ Order Summary
        public List<ShoppingCartItem> CartItems { get; set; } = new List<ShoppingCartItem>();
        public decimal TotalAmount { get; set; }

        public List<string> AvailableStates { get; set; } = new List<string> { "Gujarat" };
        public Dictionary<string, List<string>> StateCityMapping { get; set; } = new Dictionary<string, List<string>>
    {
        { "Gujarat", new List<string> { "Ahmedabad", "Surat", "Vadodara", "Rajkot" } }
    };
    }
}
