using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Data;
using System.Linq;

namespace MyECommerce.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard view
        public IActionResult Dashboard()
        {
            var categoryCount = _context.Categories.Count();
            var productCount = _context.Products.Count();
            var orderCount = _context.Orders.Count();
            var paymentCount = _context.Payments.Count();

            var dashboardData = new
            {
                CategoryCount = categoryCount,
                ProductCount = productCount,
                OrderCount = orderCount,
                PaymentCount = paymentCount
            };

            return View(dashboardData);
        }

        // Orders page
        public IActionResult Orders()
        {
            var orders = _context.Orders.ToList();
            return View(orders);
        }

        // Staff page
        public IActionResult Staff()
        {
            // Fetch staff or admin data if needed
            return View();
        }

        // Products page
        public IActionResult ProductIndex()
        {
            var products = _context.Products.Include(p => p.Category).ToList();
            return View("Product/Index", products);
        }

        // Admin Categories
        public IActionResult CategoryIndex()
        {
            var categories = _context.Categories.ToList();
            return View("Category/Index", categories);
        }
    }
}
