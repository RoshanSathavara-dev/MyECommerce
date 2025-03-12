using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Data;
using MyECommerce.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace MyECommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Product
        public async Task<IActionResult> Index(string search, int? categoryId)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View(await products.ToListAsync());
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid product data." });
            }

            if (ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                product.ImageUrl = "/images/" + uniqueFileName;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Product added successfully!" });
        }


        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? ImageFile)
        {
            if (id != product.Id)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid product data." });
            }

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Color = product.Color;
            existingProduct.Size = product.Size;
            existingProduct.IsFeatured = product.IsFeatured;
            existingProduct.ShowInHeroSection = product.ShowInHeroSection;

            if (ImageFile != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                existingProduct.ImageUrl = "/images/" + uniqueFileName;
            }

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Product updated successfully!" });
        }
        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            return Json(new
            {
                success = true,
                id = product.Id,
                name = product.Name,
                description = product.Description,
                price = product.Price,
                stock = product.Stock,
                categoryId = product.CategoryId,
                color = product.Color,
                size = product.Size,
                isFeatured = product.IsFeatured,
                showInHeroSection = product.ShowInHeroSection,
                imageUrl = product.ImageUrl
            });
        }




        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Delete image from folder
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

    }
}
