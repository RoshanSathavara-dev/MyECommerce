using MyECommerce.Models;
using System.ComponentModel.DataAnnotations;

namespace MyECommerce.ViewModels
{
    public class CheckoutViewModel
    {
        // ✅ Shipping Details
        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, Phone] public string Phone { get; set; } = string.Empty;
        [Required] public string StreetAddress { get; set; } = string.Empty;
        [Required] public string City { get; set; } = string.Empty;
        [Required] public string State { get; set; } = string.Empty;
        [Required] public string PinCode { get; set; } = string.Empty;
        public string? OrderNotes { get; set; }


        // ✅ Payment Details
        [Required] public string PaymentMethod { get; set; } = "COD"; // Cash on Delivery, UPI, Card

        // ✅ Order Summary
        public List<ShoppingCartItem> CartItems { get; set; } = new List<ShoppingCartItem>();
        public decimal TotalAmount { get; set; }
    }
}
