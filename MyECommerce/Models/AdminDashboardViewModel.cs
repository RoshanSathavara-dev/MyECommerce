namespace MyECommerce.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCustomZulaRequests { get; set; }

        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<CustomZula> RecentCustomZulas { get; set; } = new List<CustomZula>();
        public List<Review> RecentReviews { get; set; } = new List<Review>();

        public int TotalCategories { get; set; }
    }
}
