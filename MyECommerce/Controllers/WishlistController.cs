using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Data;
using MyECommerce.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace MyECommerce.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Get the Wishlist View
        public async Task<IActionResult> Index()
        {
            string userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account"); // ✅ Redirect if user is not logged in
            }

            var wishlist = await _context.WishlistItems
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return View(wishlist);
        }

        // ✅ Add Item to Wishlist
        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            string userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Please log in to add items to wishlist." });
            }

            var existingItem = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.ProductId == productId && w.UserId == userId);

            if (existingItem != null)
            {
                return Json(new { success = false, message = "Product is already in your wishlist!" });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found!" });
            }

            _context.WishlistItems.Add(new WishlistItem
            {
                ProductId = productId,
                UserId = userId
            });
            await _context.SaveChangesAsync();

            int wishlistCount = await _context.WishlistItems.CountAsync(w => w.UserId == userId);

            return Json(new { success = true, message = "Added to wishlist!", count = wishlistCount });
        }

        // ✅ Remove Item from Wishlist
        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            string userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Please log in to remove items from wishlist." });
            }

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.ProductId == productId && w.UserId == userId);

            if (item == null)
            {
                return Json(new { success = false, message = "Item not found in wishlist!" });
            }

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();

            int wishlistCount = await _context.WishlistItems.CountAsync(w => w.UserId == userId);

            return Json(new { success = true, message = "Removed from wishlist!", count = wishlistCount });
        }

        // ✅ Get Wishlist Count
        [HttpGet]
        public async Task<IActionResult> GetWishlistCount()
        {
            string userId = GetUserId();
            int count = await _context.WishlistItems.CountAsync(w => w.UserId == userId);
            return Json(new { success = true, count = count });
        }

        // ✅ Helper method to get the logged-in user's ID
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }
    }
}
