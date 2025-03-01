using Microsoft.AspNetCore.Mvc;
using MyECommerce.Data;
using MyECommerce.Models;

namespace MyECommerce.Controllers
{
    public class CustomZulaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomZulaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CustomZula
        public IActionResult Index()
        {
            var customZulas = _context.CustomZulas.ToList();
            return View(customZulas);
        }

        // GET: CustomZula/Details/5
        public IActionResult Details(int id)
        {
            var customZula = _context.CustomZulas.Find(id);
            if (customZula == null)
            {
                return NotFound();
            }
            return View(customZula);
        }

        // GET: CustomZula/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CustomZula/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomZula model, IFormFile? imageFile) // ✅ Allow imageFile to be nullable
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CustomZulaImages"); // ✅ Store inside wwwroot
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(folderPath, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    model.ImageUrl = "/CustomZulaImages/" + uniqueFileName; // ✅ Ensure relative path is correct
                }
                else
                {
                    model.ImageUrl = "/CustomZulaImages/default.jpg"; // ✅ Set default image if no file uploaded
                }

                _context.CustomZulas.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }




        // GET: CustomZula/Edit/5
        public IActionResult Edit(int id)
        {
            var customZula = _context.CustomZulas.Find(id);
            if (customZula == null)
            {
                return NotFound();
            }
            return View(customZula);
        }

        // POST: CustomZula/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomZula model, IFormFile? imageFile)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var customZula = _context.CustomZulas.Find(id);
                if (customZula == null)
                {
                    return NotFound();
                }

                customZula.Description = model.Description;
                customZula.ContactNo = model.ContactNo;

                if (imageFile != null && imageFile.Length > 0)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/CustomZulaImages");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(folderPath, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    customZula.ImageUrl = "/CustomZulaImages/" + uniqueFileName;
                }

                _context.CustomZulas.Update(customZula);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: CustomZula/Delete/5


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customZula = await _context.CustomZulas.FindAsync(id);
            if (customZula == null)
            {
                return Json(new { success = false, message = "Item not found." });
            }

            _context.CustomZulas.Remove(customZula);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
