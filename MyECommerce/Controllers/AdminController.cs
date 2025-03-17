using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Data;
using MyECommerce.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyECommerce.Controllers
{
    [Authorize(Roles = "Admin")] // ✅ Ensure only Admins can access
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalProducts = _context.Products.Count(),
                TotalOrders = _context.Orders.Count(),
                TotalUsers = _userManager.Users.Count(),
                TotalCustomZulaRequests = _context.CustomZulas.Count(),

                // ✅ Fetch the 5 most recent orders
                RecentOrders = _context.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToList(),
                RecentCustomZulas = _context.CustomZulas
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedDate)
            .Take(3)
            .ToList()
            };

            return View(model);
        }


        // ✅ Manage Orders
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // ✅ Manage Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public IActionResult OrderIndex()
        {
            var orders = _context.Orders.OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        // ✅ View Order Details
        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // ✅ Ensure Product is included
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            order.Status = status; // ✅ Ensure the status is updated
            _context.Orders.Update(order);
            await _context.SaveChangesAsync(); // ✅ Ensure changes are saved

            return Json(new { success = true, message = "Order status updated successfully!" });
        }




        // ✅ Delete Order
        [HttpPost]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
            }

            return RedirectToAction("OrderIndex");
        }

        public async Task<IActionResult> ManageProducts(string search, int? categoryId)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");

            return View(await products.ToListAsync());
        }

        public async Task<IActionResult> ManageCustomZulaRequests()
        {
            var requests = await _context.CustomZulas
                .Include(c => c.User) // ✅ Include user details
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomZulaStatus(int id, string status)
        {
            var request = await _context.CustomZulas.FindAsync(id);
            if (request == null)
            {
                return Json(new { success = false, message = "Request not found." });
            }

            request.Status = status; // ✅ Update status
            _context.CustomZulas.Update(request);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Request status updated successfully!" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // ✅ Protect from CSRF attacks
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // ✅ Logs out the user
            HttpContext.Session.Clear(); // ✅ Clears all session data
            Response.Cookies.Delete(".AspNetCore.Cookies"); // ✅ Deletes authentication cookie

            return RedirectToAction("Login", "Account"); // ✅ Redirect to login page
        }


        public async Task<IActionResult> ManageReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // ✅ Delete Review
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return Json(new { success = false, message = "Review not found." });
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public async Task<IActionResult> ManageUsers(string search)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
            }

            var userList = await users.ToListAsync();
            return View(userList);
        }

        // ✅ Assign or Remove Admin Role
        [HttpPost]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.Role = user.Role == "Admin" ? "User" : "Admin"; // Toggle role
            await _userManager.UpdateAsync(user);

            return Json(new { success = true, message = "User role updated successfully!" });
        }



    }
}
