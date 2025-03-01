using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Data;
using MyECommerce.Models;
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

        // ✅ Submit a review
        [HttpPost]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
        {
            var userId = HttpContext.Session.GetInt32("UserId"); // ✅ Get UserId from session
            if (userId == null)
            {
                return Json(new { success = false, message = "You must be logged in to submit a review." });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Invalid user." });
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = user.Id, // ✅ Save user ID
                Rating = rating,
                Comment = comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Review submitted successfully!" });
        }

        // ✅ Get all reviews for a product
        public async Task<IActionResult> GetReviews(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    Name = r.User != null ? r.User.Name : "Unknown User",
                    r.Rating,
                    r.Comment,
                    CreatedAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Json(reviews);
        }

    }
}
