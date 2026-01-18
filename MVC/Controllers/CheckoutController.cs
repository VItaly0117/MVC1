using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using MVC.Models.Forms;

namespace MVC.Controllers;

[Authorize] // Оформлять заказ могут только залогиненные пользователи
public class CheckoutController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly StoreContext _context;

    public CheckoutController(UserManager<User> userManager, StoreContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var form = new CheckoutForm
        {
            // Предзаполняем имя из профиля пользователя
            FullName = user?.FullName ?? string.Empty
        };
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutForm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        // Здесь могла бы быть логика создания заказа и очистки корзины
        // Но мы просто перенаправляем на страницу благодарности
        return RedirectToAction(nameof(ThankYou));
    }

    public IActionResult ThankYou()
    {
        return View();
    }
}