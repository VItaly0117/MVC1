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
    
    [HttpGet]
    public async Task<IActionResult> Count()
    {
        var cart = await GetCartAsync();
        var count = cart.Items.Sum(i => i.Quantity);
        return Json(new { count });
    }

    private async Task<Cart> GetCartAsync()
    {
        Cart? cart = null;
        var user = await _userManager.GetUserAsync(User);

        if (user != null)
        {
            cart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
        }

        if (cart == null && Request.Cookies.TryGetValue(CartCookieName, out var cartIdStr) && Guid.TryParse(cartIdStr, out var cartId))
        {
            cart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.UniqueId == cartId);
        }

        if (cart == null)
        {
            cart = new Cart();
            _context.Carts.Add(cart);
        }

        if (user != null && cart.UserId == null)
        {
            cart.UserId = user.Id;
            await _context.SaveChangesAsync();
        }

        Response.Cookies.Append(CartCookieName, cart.UniqueId.ToString(), new CookieOptions
        {
            Expires = DateTime.Now.AddDays(30),
            HttpOnly = true,
            IsEssential = true
        });

        return cart;
    }
}