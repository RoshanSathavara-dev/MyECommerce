using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Data;
using MyECommerce.Models;
using MyECommerce.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MyECommerce.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Order") });
            }


            string cartId = GetCartId();
            var userId = GetUserId();

            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.Email })
                .FirstOrDefaultAsync();

            var cartItems = await _context.ShoppingCartItems
                .Where(item => item.CartId == cartId || item.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Shop");
            }

            var viewModel = new CheckoutViewModel
            {
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(item => item.Price * item.Quantity),
                Email = user?.Email ?? ""
            };

            return View(viewModel);
        }


        // ✅ Enforce Login Before Placing Order
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            string cartId = GetCartId();
            var userId = GetUserId();

            var cartItems = await _context.ShoppingCartItems
                .Where(item => item.CartId == cartId || item.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Shop");
            }

            var order = new Order
            {
                UserId = userId ?? "Guest",
                OrderDate = DateTime.UtcNow,
                TotalAmount = cartItems.Sum(item => item.Price * item.Quantity),
                FullName = $"{model.FirstName} {model.LastName}".Trim(),
                Email = model.Email,
                Phone = model.Phone,
                Address = model.StreetAddress,
                Country = model.Country,
                City = model.City,
                State = model.State,
                PinCode = model.PinCode,
                PaymentMethod = model.PaymentMethod,
                Status = "Pending",
                OrderItems = cartItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.ShoppingCartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your order has been placed successfully!";
            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }
        // ✅ Order Confirmation Page
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // ✅ Helper Methods
        private string GetCartId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return GetUserId() ?? Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("CartId")))
            {
                HttpContext.Session.SetString("CartId", Guid.NewGuid().ToString());
            }

            return HttpContext.Session.GetString("CartId") ?? string.Empty;
        }

        [HttpGet]
        public IActionResult GetCities(string state)
        {
            try
            {
                var stateCityMapping = new Dictionary<string, List<string>>
        {
            { "Gujarat", new List<string> { "Ahmedabad", "Surat", "Vadodara", "Rajkot" } }
        };

                if (!string.IsNullOrEmpty(state) && stateCityMapping.ContainsKey(state))
                {
                    return Json(stateCityMapping[state]);
                }

                return BadRequest(new { error = "Invalid State" }); // 🔴 Returns proper error response
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Server Error", message = ex.Message });
            }
        }



        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
