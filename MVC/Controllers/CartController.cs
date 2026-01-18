using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Models.Dto;

namespace MVC.Controllers;

public class CartController : Controller
{
    private readonly StoreContext _context;
    private readonly UserManager<User> _userManager;
    private const string CartCookieName = "cart_uuid";

    public CartController(StoreContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var cart = await GetCartAsync();
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddProductDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _context.Products.FindAsync(model.ProductId);
        if (product == null)
        {
            return NotFound("Product not found");
        }

        var cart = await GetCartAsync();
        var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == model.ProductId);

        if (cartItem != null)
        {
            cartItem.Quantity += model.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = model.ProductId,
                Quantity = model.Quantity
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrderForItem(int id)
    {
        var cart = await GetCartAsync();
        var cartItem = cart.Items.FirstOrDefault(i => i.Id == id);

        if (cartItem == null)
        {
            return NotFound();
        }

        // "Оплачиваем" товар, помечая его как заказанный
        cartItem.IsOrdered = true;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Order placed for {cartItem.Product.Name}!";

        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> Count()
    {
        var cart = await GetCartAsync();
        var count = cart.Items.Sum(i => i.Quantity);
        return Json(new { count });
    }

    private async Task<Cart> GetCartAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        // --- Logic for Logged-in Users ---
        if (user != null)
        {
            // Find a cart specifically for this user.
            var userCart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // If the user doesn't have a cart, create one for them.
            if (userCart == null)
            {
                userCart = new Cart { UserId = user.Id };
                _context.Carts.Add(userCart);
                await _context.SaveChangesAsync();
            }
            return userCart;
        }

        // --- Logic for Guests ---
        Cart? guestCart = null;
        if (Request.Cookies.TryGetValue(CartCookieName, out var cartIdStr) && Guid.TryParse(cartIdStr, out var cartId))
        {
            // Find the guest cart by its unique ID from the cookie.
            // Important: Also check that it's not associated with any user.
            guestCart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.UniqueId == cartId && c.UserId == null);
        }

        // If no valid guest cart is found, create a new one.
        if (guestCart == null)
        {
            guestCart = new Cart(); // This cart has no UserId.
            _context.Carts.Add(guestCart);
            await _context.SaveChangesAsync();
        }

        // Set/update the cookie for the guest.
        Response.Cookies.Append(CartCookieName, guestCart.UniqueId.ToString(), new CookieOptions
        {
            Expires = DateTime.Now.AddDays(30),
            HttpOnly = true,
            IsEssential = true
        });

        return guestCart;
    }
}