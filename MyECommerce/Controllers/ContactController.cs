using Microsoft.AspNetCore.Mvc;
using MyECommerce.Models;  // Import your models
using System.Threading.Tasks;
using MyECommerce.Data;  // Import your database context

namespace MyECommerce.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Show Contact Page
        public IActionResult Index()
        {
            return View();
        }

        // Handle Form Submission
        [HttpPost]
        public async Task<IActionResult> SubmitContactForm(Contact model)
        {
            if (ModelState.IsValid)
            {
                _context.Contacts.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your message has been sent successfully!";
                return RedirectToAction("Index");
            }

            return View("Index", model);
        }
    }
}
