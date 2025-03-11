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
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            Console.WriteLine($"AddToCart called for Product ID: {productId}");
            string cartId = GetCartId();
            var userId = GetUserId();

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
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

            // ✅ Fetch updated cart items
            var cartItems = await _context.ShoppingCartItems
                .Where(item => item.CartId == cartId || item.UserId == userId)
                .Select(item => new
                {
                    id = item.ProductId,
                    name = item.Name,
                    price = item.Price,
                    quantity = item.Quantity,
                    imageUrl = item.ImageUrl
                })
                .ToListAsync();

            var totalCartItems = cartItems.Sum(i => i.quantity);
            var totalCartPrice = cartItems.Sum(i => i.price * i.quantity);

            return Json(new
            {
                success = true,
                message = "Added to cart!",
                count = totalCartItems,
                totalPrice = totalCartPrice,
                cartItems = cartItems // ✅ Send updated cart items
            });
        }



        // Remove Item from Cart
       [HttpPost]
public async Task<IActionResult> RemoveFromCart(int productId)
{
    string cartId = GetCartId();
    var userId = GetUserId();

    var cartItem = await _context.ShoppingCartItems
        .FirstOrDefaultAsync(item => (item.CartId == cartId || item.UserId == userId) && item.ProductId == productId);

    if (cartItem == null)
    {
        return Json(new { success = false, message = "Item not found in cart." });
    }

    _context.ShoppingCartItems.Remove(cartItem);
    await _context.SaveChangesAsync();

    // ✅ Fetch updated cart items after removal
    var cartItems = await _context.ShoppingCartItems
        .Where(item => item.CartId == cartId || item.UserId == userId)
        .Select(item => new
        {
            id = item.ProductId,
            name = item.Name,
            price = item.Price,
            quantity = item.Quantity,
            imageUrl = item.ImageUrl
        })
        .ToListAsync();

    var totalCartItems = cartItems.Sum(i => i.quantity);
    var totalCartPrice = cartItems.Sum(i => i.price * i.quantity);

    return Json(new
    {
        success = true,
        message = "Item removed from cart!",
        count = totalCartItems,
        totalPrice = totalCartPrice,
        cartItems = cartItems // ✅ Send updated cart items
    });
}



        // Update Quantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            string cartId = GetCartId();
            var userId = GetUserId();

            var cartItem = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(item => (item.CartId == cartId || item.UserId == userId) && item.ProductId == id);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Cart item not found." });
            }

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            // ✅ Fetch updated cart items
            var cartItems = await _context.ShoppingCartItems
                .Where(item => item.CartId == cartId || item.UserId == userId)
                .Select(item => new
                {
                    id = item.ProductId,
                    name = item.Name,
                    price = item.Price,
                    quantity = item.Quantity,
                    imageUrl = item.ImageUrl
                })
                .ToListAsync();

            var totalCartItems = cartItems.Sum(i => i.quantity);
            var totalCartPrice = cartItems.Sum(i => i.price * i.quantity);

            return Json(new
            {
                success = true,
                message = "Quantity updated successfully!",
                count = totalCartItems,
                totalPrice = totalCartPrice,
                cartItems = cartItems // ✅ Send updated cart items
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartSummary()
        {
            string cartId = GetCartId();
            var userId = GetUserId();

            // ✅ Fetch cart items
            var cartItems = await _context.ShoppingCartItems
                .Where(item => item.CartId == cartId || item.UserId == userId)
                .ToListAsync();

            int totalCount = cartItems.Sum(i => i.Quantity);
            decimal totalPrice = cartItems.Sum(i => i.Price * i.Quantity);

            return Json(new { success = true, count = totalCount, totalPrice });
        }




        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            string cartId = GetCartId();
            var userId = GetUserId();

            // ✅ Fetch cart items from the database
            var cartItems = await _context.ShoppingCartItems
                .Where(item => item.CartId == cartId || item.UserId == userId)
                .Select(item => new
                {
                    id = item.ProductId,
                    name = item.Name,
                    price = item.Price,
                    quantity = item.Quantity,
                    imageUrl = item.ImageUrl
                })
                .ToListAsync();

            var totalCartItems = cartItems.Sum(i => i.quantity);
            var totalCartPrice = cartItems.Sum(i => i.price * i.quantity);

            return Json(new
            {
                success = true,
                count = totalCartItems,
                totalPrice = totalCartPrice,
                cartItems = cartItems // ✅ Send updated cart items
            });
        }



        // Helper method to get CartId
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




        // Helper method to get UserId if logged in
        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier); // ✅ Returns a string now
        }



    }
}
