namespace MyECommerce.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public required string Message { get; set; }  // ✅ Required property
        public required string Type { get; set; }  // ✅ Required property
        public bool IsRead { get; set; } = false;  // ✅ Default to false
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
