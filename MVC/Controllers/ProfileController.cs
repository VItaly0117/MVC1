using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Models.Forms;
using MVC.Models.Services;

namespace MVC.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IFileStorage _fileStorage;
    private readonly StoreContext _context;

    public ProfileController(UserManager<User> userManager, SignInManager<User> signInManager, IFileStorage fileStorage, StoreContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _fileStorage = fileStorage;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        await _context.Entry(user).Reference(u => u.Avatar).LoadAsync();

        var form = new UserProfileForm
        {
            FullName = user.FullName
        };
        
        ViewBag.AvatarSrc = user.Avatar?.Src;
        return View(form);
    }

    [HttpPost]
    public async Task<IActionResult> Index(UserProfileForm form)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await _context.Entry(user).Reference(u => u.Avatar).LoadAsync();
            ViewBag.AvatarSrc = user.Avatar?.Src;
            return View(form);
        }

        user.FullName = form.FullName;

        if (form.Avatar != null)
        {
            await _context.Entry(user).Reference(u => u.Avatar).LoadAsync();
            var oldAvatar = user.Avatar;

            var fileName = await _fileStorage.SaveFileAsync(form.Avatar);
            var newAvatar = new ImageUploaded { FileName = fileName };
            user.Avatar = newAvatar;

            if (oldAvatar != null)
            {
                await _fileStorage.DeleteFileAsync(oldAvatar.FileName);
                _context.Images.Remove(oldAvatar);
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Profile updated successfully.";
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(form);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordForm form)
    {
        if (!ModelState.IsValid) return View(form);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, form.OldPassword, form.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(form);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Your password has been changed.";

        return RedirectToAction(nameof(ChangePassword));
    }
}