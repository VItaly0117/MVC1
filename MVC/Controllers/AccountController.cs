using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using MVC.Models.Forms;

namespace MVC.Controllers;

// Добавляем SignInManager<User> в конструктор
public class AccountController(UserManager<User> userManager, SignInManager<User> signInManager) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Правильный выход через SignInManager очищает куки корректно
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterForm());
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterForm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var existingUser = await userManager.FindByEmailAsync(form.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(form.Email), "Email is already in use.");
            return View(form);
        }

        var user = new User
        {
            UserName = form.Email,
            Email = form.Email,
            // ИСПРАВЛЕНИЕ: Теперь берем имя из формы, а не "New User"
            FullName = form.FullName 
        };

        var result = await userManager.CreateAsync(user, form.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(nameof(form.Password), error.Description);
            }
            return View(form);
        }

        await signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginForm());
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginForm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        // Ищем пользователя
        var user = await userManager.FindByEmailAsync(form.Email);

        if (user == null)
        {
            ModelState.AddModelError(nameof(form.Email), "User not found");
            return View(form);
        }

        // Проверяем пароль и входим
        // PasswordSignInAsync делает все проверки безопасно
        var result = await signInManager.PasswordSignInAsync(user, form.Password, isPersistent: true, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(form.Password), "Wrong password");
            return View(form);
        }

        // Проверяем роль для редиректа
        if (await userManager.IsInRoleAsync(user, RoleConstants.Admin))
        {
            // ОШИБКА БЫЛА ТУТ: Порядок аргументов (Action, Controller)
            return RedirectToAction("Index", "User");
        }

        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}