using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyECommerce.Data;
using Microsoft.EntityFrameworkCore;

namespace MyECommerce.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Shop
        public async Task<IActionResult> Index(string search, string categoryIds, decimal? minPrice, decimal? maxPrice, int page = 1)
        {
            int pageSize = 12; // ✅ Show 12 products per page

            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // ✅ Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            // ✅ Apply category filterzz
            if (!string.IsNullOrEmpty(categoryIds))
            {
                var categoryIdList = categoryIds.Split(',').Select(int.Parse).ToList();
                productsQuery = productsQuery.Where(p => categoryIdList.Contains(p.CategoryId));
            }

            // ✅ Apply price filter
            if (minPrice.HasValue && maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value && p.Price <= maxPrice.Value);
            }

            // ✅ Fetch category & latest products (Only for normal page load, not for AJAX)
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.LatestProducts = await _context.Products.OrderByDescending(p => p.Id).Take(3).ToListAsync();
            }

            // ✅ Total product count for pagination
            int totalProducts = await productsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            // ✅ Fetch paginated products
            var products = await productsQuery
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ Handle AJAX request separately
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    products = products.Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        description = p.Description?.Length > 50 ? p.Description.Substring(0, 50) + "..." : p.Description,
                        price = p.Price,
                        imageUrl = p.ImageUrl,
                        categoryName = p.Category?.Name ?? "Uncategorized"
                    }),
                    totalPages = totalPages
                });
            }

            // ✅ Send pagination details to the view (For normal page load)
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            return View(products);
        }   

        [HttpGet]
        public async Task<IActionResult> GlobalSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { success = false, products = new List<object>() });
            }

            var products = await _context.Products
                .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    imageUrl = p.ImageUrl
                })
                .Take(5) // ✅ Limit results to 5 for better UX
                .ToListAsync();

            return Json(new { success = true, products });
        }

    }
}
