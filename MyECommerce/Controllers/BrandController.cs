using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Data;
using MyECommerce.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyECommerce.Controllers
{
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BrandController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ✅ GET: Retrieve all brands (for AJAX rendering)
        public async Task<IActionResult> GetAllBrands()
        {
            var brands = await _context.Brands.ToListAsync();
            return Json(brands);
        }

        // ✅ POST: Create a new brand with an image
        [HttpPost]
        public async Task<IActionResult> CreateBrand(string name, IFormFile image)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Brand name is required." });
            }

            if (image == null || image.Length == 0)
            {
                return Json(new { success = false, message = "Brand image is required." });
            }

            // ✅ Save the uploaded image
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/brands");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // ✅ Save Brand with Image URL
            var newBrand = new Brand { Name = name, ImageUrl = "/uploads/brands/" + fileName };
            _context.Brands.Add(newBrand);
            await _context.SaveChangesAsync();

            return Json(new { success = true, brand = newBrand });
        }

        // ✅ DELETE: Remove a brand by ID
        [HttpPost]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return Json(new { success = false, message = "Brand not found." });
            }

            // ✅ Delete the image file
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot" + brand.ImageUrl);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Brand deleted successfully." });
        }
    }
}
