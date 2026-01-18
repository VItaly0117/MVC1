using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Models.Dto;
using MVC.Models.Forms;

namespace MVC.Controllers;

[Authorize(Roles = RoleConstants.Admin)]
public class UserController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public UserController(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userViewModels = new List<UserViewModel>();
        foreach (var user in users)
        {
            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName,
                Roles = await _userManager.GetRolesAsync(user)
            });
        }
        return View(userViewModels);
    }

    public async Task<IActionResult> EditRoles(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = await _roleManager.Roles.ToListAsync();

        var model = new UserRolesForm
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Roles = allRoles.Select(role => new UserRoleItem
            {
                RoleName = role.Name!,
                IsSelected = userRoles.Contains(role.Name!)
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoles(UserRolesForm form)
    {
        var user = await _userManager.FindByIdAsync(form.UserId.ToString());
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var selectedRoles = form.Roles.Where(r => r.IsSelected).Select(r => r.RoleName);

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (!result.Succeeded)
        {
            // Handle error
            return View(form);
        }

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!result.Succeeded)
        {
            // Handle error
            return View(form);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ResetPassword(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var model = new ResetPasswordForm
        {
            UserId = user.Id.ToString(),
            UserName = user.UserName!
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordForm form)
    {
        if (!ModelState.IsValid) return View(form);

        var user = await _userManager.FindByIdAsync(form.UserId);
        if (user == null) return NotFound();

        // To reset password without knowing the old one, we need a token
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, form.NewPassword);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"Password for {user.UserName} has been reset.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"User {user.UserName} has been deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}