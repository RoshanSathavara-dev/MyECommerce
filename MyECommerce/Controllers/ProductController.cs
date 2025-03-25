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
        public async Task<IActionResult> Create(Product product, IFormFile? MainImage, List<IFormFile>? ImageFiles)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            product.ShowInHeroSection = Request.Form["ShowInHeroSection"] == "true";
            product.IsFeatured = Request.Form["IsFeatured"] == "true";

            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // ✅ Process Main Image Upload
            if (MainImage != null && MainImage.Length > 0)
            {
                string mainImageFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(MainImage.FileName);
                string mainImagePath = Path.Combine(uploadsFolder, mainImageFileName);

                using (var fileStream = new FileStream(mainImagePath, FileMode.Create))
                {
                    await MainImage.CopyToAsync(fileStream);
                }

                product.ImageUrl = "/images/" + mainImageFileName;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // ✅ Save Product First

            // ✅ Process Multiple Images (Remove Duplicates)
            if (ImageFiles != null && ImageFiles.Count > 0)
            {
                // ✅ Remove duplicate images based on file names
                var uniqueImages = ImageFiles
                    .GroupBy(img => img.FileName.ToLower()) // Group by filename (case-insensitive)
                    .Select(group => group.First()) // Take only unique images
                    .ToList();

                foreach (var image in uniqueImages)
                {
                    if (image.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        string savedImagePath = "/images/" + uniqueFileName;

                        // ✅ Ensure image is not already in the database
                        bool exists = _context.ProductImages.Any(img => img.ProductId == product.Id && img.ImageUrl == savedImagePath);
                        if (!exists)
                        {
                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = savedImagePath
                            });
                        }
                    }
                }
            }

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
            ViewBag.Brands = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile? MainImage, List<IFormFile>? ImageFiles)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var existingProduct = await _context.Products.Include(p => p.ProductImages)
                                                         .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct == null)
                return Json(new { success = false, message = "Product not found." });

            // ✅ Update basic product details
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BrandId = product.BrandId;
            existingProduct.Color = product.Color;
            existingProduct.Size = product.Size;
            existingProduct.ShowInHeroSection = product.ShowInHeroSection;
            existingProduct.IsFeatured = product.IsFeatured;

            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");

            // ✅ Update Main Image if a new one is uploaded
            if (MainImage != null && MainImage.Length > 0)
            {
                string mainImageFileName = Guid.NewGuid() + "_" + Path.GetFileName(MainImage.FileName);
                string mainImagePath = Path.Combine(uploadsFolder, mainImageFileName);

                using (var fileStream = new FileStream(mainImagePath, FileMode.Create))
                {
                    await MainImage.CopyToAsync(fileStream);
                }

                existingProduct.ImageUrl = "/images/" + mainImageFileName;
            }

            // ✅ Update Additional Images (if new ones are uploaded)
            if (ImageFiles != null && ImageFiles.Count > 0)
            {
                foreach (var image in ImageFiles)
                {
                    if (image.Length > 0)
                    {
                        string fileName = Guid.NewGuid() + "_" + Path.GetFileName(image.FileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = existingProduct.Id,
                            ImageUrl = "/images/" + fileName
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Product updated successfully!" });
        }



        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.Stock,
                    p.CategoryId,
                    p.BrandId,
                    p.Color,
                    p.Size,
                    p.ImageUrl,
                    p.ShowInHeroSection, // ✅ Ensure this is returned
                    p.IsFeatured // ✅ Ensure this is returned
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return Json(new { success = false, message = "Product not found." });

            return Json(product);
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
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetProductsByCategory(int? categoryId)
        //{
        //    var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

        //    if (categoryId.HasValue && categoryId > 0)
        //    {
        //        productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
        //    }

        //    var products = await productsQuery.Select(p => new
        //    {
        //        p.Id,
        //        p.Name,
        //        p.Price,
        //        p.ImageUrl
        //    }).ToListAsync();

        //    return Json(products);
        //}
        [HttpGet]
        public IActionResult FilterProducts(int? categoryId)
        {
            var products = _context.Products
                .Include(p => p.Category) // ✅ Ensure category data is included
                .Where(p => categoryId == 0 || p.CategoryId == categoryId) // ✅ Show all if categoryId is 0
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl
                }).ToList();

            return Json(products);
        }


    }
}
