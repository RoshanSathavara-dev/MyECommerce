using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Data;
using MyECommerce.Models;
using System.Threading.Tasks;
using System.Linq;

namespace MyECommerce.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: Fetch all categories (Returns JSON)
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Json(new { success = true, data = categories });
        }

        // ✅ POST: Create a new category (Returns JSON)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.Name))
            {
                return Json(new { success = false, message = "Category name is required." });
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Category added successfully!", data = category });
        }

        // ✅ POST: Update a category (Returns JSON)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] Category category)
        {
            if (id != category.Id)
            {
                return Json(new { success = false, message = "Invalid category ID." });
            }

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                return Json(new { success = false, message = "Category not found." });
            }

            existingCategory.Name = category.Name;
            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Category updated successfully!", data = existingCategory });
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Category not found." });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Category deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


    }
}
