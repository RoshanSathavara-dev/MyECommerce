using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MyECommerce.Services;
using System;
using System.Threading.Tasks;
using MyECommerce.Models;
using MyECommerce.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace MyECommerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SendGridService _sendGridService;
        private static readonly Random _random = new();
     

        public AccountController(ApplicationDbContext context, SendGridService sendGridService)
        {
            _context = context;
            _sendGridService = sendGridService;
            
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        public async Task<IActionResult> Login(UserLoginModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                TempData["Error"] = "Invalid email.";
                return RedirectToAction("Login");
            }

            // ✅ Retrieve guest CartId from session BEFORE login
            string? guestCartId = HttpContext.Session.GetString("CartId");

            // ✅ Store UserId in session (for session-based operations)
            HttpContext.Session.SetString("UserId", user.Id.ToString());

            // ✅ Store authentication claims for the logged-in user
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email)
    };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("Cookies", claimsPrincipal);

            // ✅ Merge Guest Cart into User Cart (if applicable)
            if (!string.IsNullOrEmpty(guestCartId))
            {
                await MergeGuestCartIntoUserCart(user.Id, guestCartId);
            }

            return RedirectToAction("Index", "Home");
        }



        public IActionResult Register()
        {
            return View();
        }

        // ✅ Send OTP Only Once Using This Method
        [HttpPost]
        public async Task<IActionResult> SendOtp(string Name, string Email, string ContactNo)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(ContactNo))
                return Json(new { success = false, message = "Email or Contact No. is required." });

            // ✅ Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email || u.ContactNo == ContactNo);
            if (existingUser != null)
            {
                return Json(new { success = false, message = "User already registered. Please log in." });
            }

            // ✅ Generate OTP
            string otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("UserName", Name);
            HttpContext.Session.SetString("UserEmail", Email);
            HttpContext.Session.SetString("UserContact", ContactNo);
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTP_Expiry", DateTime.UtcNow.AddMinutes(5).ToString());

            // ✅ Send OTP via Email
            bool isSent = await _sendGridService.SendOtpEmailAsync(Email, otp);

            if (isSent)
                return Json(new { success = true });
            else
                return Json(new { success = false, message = "Failed to send OTP. Please try again." });
        }






        // ✅ Register User (No OTP Sending Here)
        [HttpPost]
        public IActionResult Register(string Name, string Email)
        {
            // ✅ Call SendOtp Instead of Duplicating Code
            return RedirectToAction("VerifyOtp");
        }

        // ✅ Verify OTP
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string otp)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var storedOtp = HttpContext.Session.GetString("OTP");
            var expiryStr = HttpContext.Session.GetString("OTP_Expiry");
            var name = HttpContext.Session.GetString("UserName");
            var contactNo = HttpContext.Session.GetString("UserContact");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(expiryStr))
            {
                return Json(new { success = false, message = "Session expired. Please try again." });
            }

            if (DateTime.UtcNow > DateTime.Parse(expiryStr))
            {
                return Json(new { success = false, message = "OTP expired. Please request a new OTP." });
            }

            if (otp != storedOtp)
            {
                return Json(new { success = false, message = "Invalid OTP. Please try again." });
            }

            // ✅ Remove OTP After Successful Verification
            HttpContext.Session.Remove("OTP");
            HttpContext.Session.Remove("OTP_Expiry");

            // ✅ Ensure session data exists before saving user
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(contactNo))
            {
                return Json(new { success = false, message = "Session data is missing. Please try again." });
            }

            // ✅ Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email || u.ContactNo == contactNo);
            if (existingUser == null)
            {
                var newUser = new User
                {
                    Name = name,
                    Email = email,
                    ContactNo = contactNo,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
            }

            // ✅ Set session for logged-in user
            HttpContext.Session.SetString("User", email);

            return Json(new { success = true });
        }


        [HttpPost]
        public async Task<IActionResult> VerifyLoginOtp(string otp)
        {
            var emailOrPhone = HttpContext.Session.GetString("UserEmailOrPhone");
            var storedOtp = HttpContext.Session.GetString("OTP");
            var expiryStr = HttpContext.Session.GetString("OTP_Expiry");

            if (string.IsNullOrEmpty(emailOrPhone) || string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(expiryStr))
            {
                return Json(new { success = false, message = "Session expired. Please try again." });
            }

            if (DateTime.UtcNow > DateTime.Parse(expiryStr))
            {
                return Json(new { success = false, message = "OTP expired. Please request a new OTP." });
            }

            if (otp != storedOtp)
            {
                return Json(new { success = false, message = "Invalid OTP. Please try again." });
            }

            // ✅ Remove OTP After Successful Verification
            HttpContext.Session.Remove("OTP");
            HttpContext.Session.Remove("OTP_Expiry");

            // ✅ Check if user exists (Login should NOT create a new user)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailOrPhone || u.ContactNo == emailOrPhone);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found. Please register first." });
            }

            // ✅ Retrieve guestCartId from session BEFORE merging carts
            string? guestCartId = HttpContext.Session.GetString("CartId");

            // ✅ Merge Guest Cart into User Cart manually
            await MergeGuestCartIntoUserCart(user.Id, guestCartId);

            // ✅ Set session for logged-in user
            HttpContext.Session.SetString("User", emailOrPhone);

            return Json(new { success = true });
        }



        [HttpPost]
        public async Task<IActionResult> SendLoginOtp(string EmailOrContact)
        {
            if (string.IsNullOrEmpty(EmailOrContact))
                return Json(new { success = false, message = "Email or Contact No. is required." });

            // ✅ Find user by Email or Contact No.
            var user = await _context.Users
                .Where(u => u.Email == EmailOrContact || u.ContactNo == EmailOrContact)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Json(new { success = false, message = "User not found. Please register first." });
            }

            // ✅ Generate OTP
            string otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("UserEmailOrPhone", EmailOrContact);
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTP_Expiry", DateTime.UtcNow.AddMinutes(5).ToString());

            // ✅ Send OTP via Email
            bool isSent = await _sendGridService.SendOtpEmailAsync(user.Email, otp);

            if (isSent)
                return Json(new { success = true });
            else
                return Json(new { success = false, message = "Failed to send OTP. Please try again." });
        }



        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // ✅ Store current userId before logout
            var userIdStr = HttpContext.Session.GetString("UserId");

            // ✅ Sign out the authenticated user (if using authentication)
            await HttpContext.SignOutAsync("Cookies");

            // ✅ Clear all session data EXCEPT CartId
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("User");

            // ✅ Restore UserId session to retain cart after logout
            if (!string.IsNullOrEmpty(userIdStr))
            {
                Console.WriteLine($"🔹 Restoring CartId as UserId {userIdStr} after logout.");
                HttpContext.Session.SetString("CartId", userIdStr);
            }

            return RedirectToAction("Index", "Home");
        }



        private async Task MergeGuestCartIntoUserCart(int userId, string? guestCartId)
        {
            if (string.IsNullOrEmpty(guestCartId)) return;

            var guestCartItems = await _context.ShoppingCartItems
                .Where(item => item.CartId == guestCartId)
                .ToListAsync();

            foreach (var item in guestCartItems)
            {
                var existingCartItem = await _context.ShoppingCartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == item.ProductId);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += item.Quantity;
                    _context.ShoppingCartItems.Remove(item); // Remove duplicate guest cart item
                }
                else
                {
                    item.UserId = userId;
                    item.CartId = userId.ToString(); // ✅ Assign the logged-in user's CartId
                }
            }

            await _context.SaveChangesAsync();

            // ✅ Clear session-based CartId after merging
            HttpContext.Session.Remove("CartId");
        }



        private async Task ClearCart(int userId)
        {
            var cartItems = await _context.ShoppingCartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Any())
            {
                _context.ShoppingCartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
        }






    }
}
