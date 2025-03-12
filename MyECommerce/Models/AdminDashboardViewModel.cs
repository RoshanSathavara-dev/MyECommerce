namespace MyECommerce.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
    }
}
