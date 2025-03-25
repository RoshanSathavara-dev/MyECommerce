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

        public IActionResult ManageGallery()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetImages()
        {
            var images = await _context.GalleryImages.ToListAsync();
            return Json(images);
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
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/gallery");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

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

                return Json(new { success = true, message = "Image uploaded successfully!", image = newImage });
            }

            return Json(new { success = false, message = "Please select an image." });
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
                return Json(new { success = false, message = "Image not found!" });
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                // Delete the old image
                string oldFilePath = Path.Combine(_environment.WebRootPath, galleryImage.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Save new image
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/gallery");
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                galleryImage.ImageUrl = "/uploads/gallery/" + uniqueFileName;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Image updated successfully!", image = galleryImage });
            }

            return Json(new { success = false, message = "No image selected." });
        }


        // ✅ 5. Delete Image
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var image = await _context.GalleryImages.FindAsync(id);
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found!" });
            }

            // Delete file from server
            string filePath = Path.Combine(_environment.WebRootPath, image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.GalleryImages.Remove(image);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Image deleted successfully!" });
        }
    }
}
