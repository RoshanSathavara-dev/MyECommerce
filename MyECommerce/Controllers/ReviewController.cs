using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Data;
using MyECommerce.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyECommerce.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

            public async Task<IActionResult> GetReviews(int productId)
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ProductId == productId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.Id,
                        UserName = r.User != null ? r.User.UserName : "Anonymous",
                        r.Rating,
                        r.Comment,
                        CreatedAt = r.CreatedAt.ToString("yyyy-MM-dd")
                    })
                    .ToListAsync();

                return Json(reviews);
            }



        [HttpPost]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "❌ You must be logged in to add a review." });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return Json(new { success = false, message = "❌ Unable to identify user. Please log in again." });
            }

            string userId = userIdClaim.Value;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "❌ User not found." });
            }

            // ✅ Check if user has purchased the product before allowing a review
            bool hasPurchased = await _context.Orders
                .AnyAsync(o => o.UserId == userId && o.OrderItems.Any(oi => oi.ProductId == productId));

            if (!hasPurchased)
            {
                return Json(new { success = false, message = "❌ You can only review products you have purchased." });
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "✅ Review added successfully!" });
        }


        [HttpGet]
        public async Task<IActionResult> CanReview(int productId)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "❌ You must be logged in to add a review." });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return Json(new { success = false, message = "❌ Unable to identify user. Please log in again." });
            }

            string userId = userIdClaim.Value;

            bool hasPurchased = await _context.Orders
                .AnyAsync(o => o.UserId == userId && o.OrderItems.Any(oi => oi.ProductId == productId));

            if (!hasPurchased)
            {
                return Json(new { success = false, message = "❌ You can only review products you have purchased." });
            }

            return Json(new { success = true });
        }



    }
}
