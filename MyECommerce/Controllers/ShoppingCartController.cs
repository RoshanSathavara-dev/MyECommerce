using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Data;
using MyECommerce.Models;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyECommerce.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display Cart
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            string cartId = GetCartId();

            var cartItems = await _context.ShoppingCartItems
                .Where(item => item.UserId == userId || (userId == null && item.CartId == cartId))
                .Include(item => item.User)
                .ToListAsync();

            return View(cartItems);
        }


        // Add Item to Cart
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            string cartId = GetCartId();
            var userId = GetUserId();

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cartItem = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(item => (item.CartId == cartId || item.UserId == userId) && item.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new ShoppingCartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl,
                    CartId = cartId,
                    UserId = userId
                };

                _context.ShoppingCartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Remove Item from Cart
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var cartItem = await _context.ShoppingCartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.ShoppingCartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // Update Quantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var cartItem = await _context.ShoppingCartItems.FindAsync(id);
            if (cartItem != null && quantity > 0)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // Helper method to get CartId
        private string GetCartId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return GetUserId()?.ToString() ?? Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("CartId")))
            {
                HttpContext.Session.SetString("CartId", Guid.NewGuid().ToString());
            }

            return HttpContext.Session.GetString("CartId") ?? string.Empty;
        }



        // Helper method to get UserId if logged in
        private int? GetUserId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
            }
            return null;
        }

    }
}
