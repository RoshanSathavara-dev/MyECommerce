using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MyECommerce.Data;
using MyECommerce.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyECommerce.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWebHostEnvironment _environment;

        public GalleryController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IWebHostEnvironment environment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _environment = environment;
        }

        // ✅ Display Uploaded Images
        public async Task<IActionResult> Index()
        {
            var galleryImages = await _context.GalleryImages.ToListAsync();
            return View(galleryImages);
        }

        // ✅ Upload Image (GET)
        public IActionResult Upload()
        {
            return View();
        }

        // ✅ Upload Image (POST)
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/gallery");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                var newImage = new GalleryImage
                {
                    ImageUrl = "/uploads/gallery/" + uniqueFileName
                };

                _context.GalleryImages.Add(newImage);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.Message = "Please select an image.";
            return View();
        }

        // ✅ Delete Image
        public async Task<IActionResult> Edit(int id)
        {
            var galleryImage = await _context.GalleryImages.FindAsync(id);
            if (galleryImage == null)
            {
                return NotFound();
            }
            return View(galleryImage);
        }

        // ✅ 4. Edit Image (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, IFormFile imageFile)
        {
            var galleryImage = await _context.GalleryImages.FindAsync(id);
            if (galleryImage == null)
            {
                return NotFound();
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                // Delete the old image
                if (!string.IsNullOrEmpty(galleryImage.ImageUrl))
                {
                    string oldFilePath = Path.Combine(_environment.WebRootPath, galleryImage.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Save new image
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/gallery");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                galleryImage.ImageUrl = "/uploads/gallery/" + uniqueFileName;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Image updated successfully!";
            }

            return RedirectToAction("Index");
        }

        // ✅ 5. Delete Image
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var galleryImage = await _context.GalleryImages.FindAsync(id);
            if (galleryImage == null)
            {
                return NotFound();
            }

            // Delete image from server
            if (!string.IsNullOrEmpty(galleryImage.ImageUrl))
            {
                string filePath = Path.Combine(_environment.WebRootPath, galleryImage.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.GalleryImages.Remove(galleryImage);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Image deleted successfully!";

            return RedirectToAction("Index");
        }
    }
}
