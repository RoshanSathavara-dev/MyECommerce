namespace MyECommerce.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;  // e.g., PayPal, Stripe
        public string Status { get; set; } = "Pending";  // Success, Failed, Pending
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Order? Order { get; set; }
    }
}
