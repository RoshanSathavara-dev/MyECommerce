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
        public async Task<IActionResult> Index(string search, string categoryIds, string brandIds, decimal? minPrice, decimal? maxPrice, string sort, int page = 1)
        {
            // Debugger to check the search parameter
     
            Console.WriteLine("Search term:", search);
            int pageSize = 12; // ✅ Show 12 products per page

            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            ViewBag.Brands = await _context.Brands.ToListAsync();

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

            // ✅ Apply brand filter (THIS WAS MISSING)
            if (!string.IsNullOrEmpty(brandIds))
            {
                var brandIdList = brandIds.Split(',').Select(int.Parse).ToList();
                productsQuery = productsQuery.Where(p => brandIdList.Contains(p.BrandId)); // Assuming `BrandId` exists in `Products`
            }

            // ✅ Apply price filter
            if (minPrice.HasValue && maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value && p.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "price_asc":
                        productsQuery = productsQuery.OrderBy(p => p.Price); // ✅ Sort by Price (Low to High)
                        break;
                    case "price_desc":
                        productsQuery = productsQuery.OrderByDescending(p => p.Price); // ✅ Sort by Price (High to Low)
                        break;
                    default:
                        productsQuery = productsQuery.OrderByDescending(p => p.Id); // ✅ Default Sorting (Latest First)
                        break;
                }
            }
            else
            {
                productsQuery = productsQuery.OrderByDescending(p => p.Id); // ✅ Default Sorting (Latest First)
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
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ Store pagination details for the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalProducts;
            ViewBag.PageSize = pageSize; // ✅ Add PageSize for UI calculations

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
                    totalPages = totalPages,
                    totalRecords = totalProducts, // ✅ Include total records in AJAX response
                    currentPage = page,
                     pageSize = pageSize

                });
            }

            // ✅ Send pagination details to the view (For normal page load)
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
