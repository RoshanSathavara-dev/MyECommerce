using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Models;
using System.Linq;
using MyECommerce.Data;
using System.Security.Claims;

namespace MyECommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        //public async Task<IActionResult> Index()
        //{
        //    if (User.Identity != null && User.Identity.IsAuthenticated)
        //    {
        //        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        //        Console.WriteLine($"✅ User is logged in. ID: {userId}, Email: {userEmail}");
        //    }
        //    else
        //    {
        //        Console.WriteLine("❌ User is NOT logged in.");
        //    }

        //    var heroProducts = await _context.Products
        //        .Where(p => p.ShowInHeroSection)
        //        .ToListAsync();

        //    var featuredProducts = await _context.Products
        //        .Where(p => p.IsFeatured)
        //        .ToListAsync();

        //    var allProducts = await _context.Products
        //        .Include(p => p.Category)
        //        .ToListAsync();

        //    var categories = await _context.Categories.ToListAsync();
        //    var galleryImages = await _context.GalleryImages.ToListAsync();

        //    var viewModel = new HomeViewModel
        //    {
        //        HeroProducts = heroProducts,
        //        FeaturedProducts = featuredProducts,
        //        AllProducts = allProducts,
        //        Categories = categories,
        //        GalleryImages = galleryImages
        //    };

        //    return View(viewModel);
        //}

        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var user = await _context.Users.FindAsync(userId);

                Console.WriteLine($"✅ User is logged in. ID: {userId}, Email: {userEmail}");

                // ✅ Fetch the Admin role first and check for null before accessing its ID
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole != null && await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.Id))
                {
                    Console.WriteLine("🔹 Redirecting admin to Admin Panel.");
                    return RedirectToAction("Index", "Admin"); // ✅ Redirect Admin
                }
            }
            else
            {
                Console.WriteLine("❌ User is NOT logged in.");
            }

            var heroProducts = await _context.Products.Where(p => p.ShowInHeroSection).ToListAsync();
            var featuredProducts = await _context.Products.Where(p => p.IsFeatured).ToListAsync();
            var allProducts = await _context.Products.Include(p => p.Category).ToListAsync();
            var categories = await _context.Categories.ToListAsync();
            var galleryImages = await _context.GalleryImages.ToListAsync();
            var brands = await _context.Brands.ToListAsync();

            var viewModel = new HomeViewModel
            {
                HeroProducts = heroProducts,
                FeaturedProducts = featuredProducts,
                AllProducts = allProducts,
                Categories = categories,
                GalleryImages = galleryImages,
                Brands = brands
            };

            return View(viewModel);
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
