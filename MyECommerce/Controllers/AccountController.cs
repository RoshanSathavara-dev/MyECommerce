using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Data;
using MyECommerce.Models;
using MyECommerce.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyECommerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly SendGridService _sendGridService;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
            ApplicationDbContext context, SendGridService sendGridService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _sendGridService = sendGridService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Action("Index", "Home"); // ✅ Default to Home if returnUrl is null
            }

            ViewBag.ReturnUrl = returnUrl; // ✅ Store returnUrl for use in the view
            return View();
        }


        // ✅ Send OTP for Login
        [HttpPost]
        public async Task<IActionResult> SendLoginOtp(string EmailOrContact, string? returnUrl = null)
        
        {
            if (string.IsNullOrEmpty(EmailOrContact))
                return Json(new { success = false, message = "Email or Contact No. is required." });

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == EmailOrContact || u.ContactNo == EmailOrContact);

            if (user == null)
                return Json(new { success = false, message = "User not found. Please register first." });

            // ✅ Generate Secure OTP
            string otp = new Random().Next(100000, 999999).ToString();

            // ✅ Ensure returnUrl is stored in session
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTP_UserId", user.Id);
            HttpContext.Session.SetString("OTP_Expiry", DateTime.UtcNow.AddMinutes(5).ToString());
            // ✅ Ensure returnUrl is always a valid string (never null)
            returnUrl = returnUrl ?? Url.Action("Index", "Home") ?? "/";

            HttpContext.Session.SetString("ReturnUrl", returnUrl);
            // ✅ Store returnUrl in session

            await _sendGridService.SendOtpEmailAsync(user.Email ?? string.Empty, otp);

            return Json(new { success = true, message = "OTP sent successfully." });
        }

        [HttpGet]
        public IActionResult CheckSession()
        {
            bool isAuthenticated = User.Identity != null && User.Identity.IsAuthenticated;
            string? userId = isAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

            return Json(new
            {
                isAuthenticated,
                userId
            });
        }




        // ✅ Verify OTP and Log in
        [HttpPost]
        public async Task<IActionResult> VerifyLoginOtp(string otp, string EmailOrContact)
        {
            if (string.IsNullOrEmpty(otp))
                return Json(new { success = false, message = "OTP is required." });

            string? storedOtp = HttpContext.Session.GetString("OTP");
            string? userId = HttpContext.Session.GetString("OTP_UserId");
            string? expiryStr = HttpContext.Session.GetString("OTP_Expiry");
            string? returnUrl = HttpContext.Session.GetString("ReturnUrl") ?? Url.Action("Index", "Home");

            if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(expiryStr))
                return Json(new { success = false, message = "Session expired. Please try again." });

            if (DateTime.UtcNow > DateTime.Parse(expiryStr))
                return Json(new { success = false, message = "OTP expired. Please request a new OTP." });

            if (otp != storedOtp)
                return Json(new { success = false, message = "Invalid OTP. Please try again." });

            // ✅ Remove OTP from session after successful login
            HttpContext.Session.Remove("OTP");
            HttpContext.Session.Remove("OTP_Expiry");
            HttpContext.Session.Remove("ReturnUrl");

            // ✅ Retrieve user and sign them in
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            await _signInManager.SignInAsync(user, isPersistent: false);

            // ✅ Merge Guest Cart into User Cart
            MergeGuestCartIntoUserCart(user.Id);

            // ✅ Check if the user is an admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Admin") });
            }
            else
            {
                return Json(new { success = true, redirectUrl = returnUrl });
            }
        }


        private void MergeGuestCartIntoUserCart(string userId)
        {
            string? guestCartId = HttpContext.Session.GetString("CartId"); // ✅ Get Guest Cart ID
            if (string.IsNullOrEmpty(guestCartId))
                return; // ✅ No guest cart, nothing to merge

            var guestCartItems = _context.ShoppingCartItems
                .Where(item => item.CartId == guestCartId && item.UserId == null) // ✅ Only guest cart items
                .ToList();

            if (!guestCartItems.Any())
                return; // ✅ No items in guest cart, exit

            var userCartItems = _context.ShoppingCartItems
                .Where(item => item.UserId == userId) // ✅ Get the user's cart items
                .ToList();

            foreach (var guestItem in guestCartItems)
            {
                var existingUserItem = userCartItems
                    .FirstOrDefault(item => item.ProductId == guestItem.ProductId);

                if (existingUserItem != null)
                {
                    // ✅ Product exists in user cart, update quantity
                    existingUserItem.Quantity += guestItem.Quantity;
                }
                else
                {
                    // ✅ Assign guest cart items to logged-in user
                    guestItem.UserId = userId;

                    // ✅ If you want to keep `CartId`, remove this line:
                    // guestItem.CartId = null;

                    _context.ShoppingCartItems.Update(guestItem);
                }
            }

            _context.SaveChanges(); // ✅ Save changes

            // ✅ Clear guest cart session (CartId) since it's no longer needed
            HttpContext.Session.Remove("CartId");
        }




        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // ✅ Properly logs out the user
            HttpContext.Session.Clear(); // ✅ Clears session
            Response.Cookies.Delete(".AspNetCore.Cookies"); // ✅ Deletes authentication cookie

            return RedirectToAction("Login", "Account"); // ✅ Redirects to login page
        }






        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ✅ Send OTP for Registration
        [HttpPost]
        public async Task<IActionResult> SendOtp(string Name, string Email, string ContactNo)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(ContactNo))
                return Json(new { success = false, message = "Email or Contact No. is required." });

            var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == Email || u.ContactNo == ContactNo);
            if (existingUser != null)
                return Json(new { success = false, message = "User already registered. Please log in." });

            string otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("UserName", Name);
            HttpContext.Session.SetString("UserEmail", Email);
            HttpContext.Session.SetString("UserContact", ContactNo);
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTP_Expiry", DateTime.UtcNow.AddMinutes(5).ToString());

            await _sendGridService.SendOtpEmailAsync(Email, otp);

            return Json(new { success = true });
        }

        // ✅ Verify OTP and Register
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string otp)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var storedOtp = HttpContext.Session.GetString("OTP");
            var expiryStr = HttpContext.Session.GetString("OTP_Expiry");
            var name = HttpContext.Session.GetString("UserName");
            var contactNo = HttpContext.Session.GetString("UserContact");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(expiryStr))
                return Json(new { success = false, message = "Session expired. Please try again." });

            if (DateTime.UtcNow > DateTime.Parse(expiryStr))
                return Json(new { success = false, message = "OTP expired. Please request a new OTP." });

            if (otp != storedOtp)
                return Json(new { success = false, message = "Invalid OTP. Please try again." });

            // ✅ Remove OTP After Successful Verification
            HttpContext.Session.Remove("OTP");
            HttpContext.Session.Remove("OTP_Expiry");

            var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email || u.ContactNo == contactNo);
            if (existingUser == null)
            {
                var newUser = new User
                {
                    Name = name ?? "Unknown", // ✅ Provide default value if null
                    Email = email ?? string.Empty, // ✅ Ensure email is not null
                    ContactNo = contactNo ?? "N/A", // ✅ Provide default value
                    UserName = email ?? "defaultuser", // ✅ Identity requires UserName
                    CreatedDate = DateTime.UtcNow
                };


                var result = await _userManager.CreateAsync(newUser);
                if (!result.Succeeded)
                    return Json(new { success = false, message = "Failed to create user." });
            }

            return Json(new { success = true });
        }
    }
}
