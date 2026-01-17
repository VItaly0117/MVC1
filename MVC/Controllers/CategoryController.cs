using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Models.Forms;
using MVC.Models.Services;

namespace MVC.Controllers;

[Authorize(Roles = RoleConstants.Manager)]
public class CategoryController : Controller
{
    private readonly StoreContext _context;
    private readonly IFileStorage _fileStorage;

    public CategoryController(StoreContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Categories.Include(c => c.Image).ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryForm form)
    {
        if (!ModelState.IsValid) return View(form);

        var category = new Category { Name = form.Name };

        if (form.Image != null)
        {
            var fileName = await _fileStorage.SaveFileAsync(form.Image);
            category.Image = new ImageUploaded { FileName = fileName };
        }

        _context.Add(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var category = await _context.Categories.Include(c => c.Image).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();

        var form = new CategoryForm { Name = category.Name };
        ViewBag.ImageSrc = category.Image?.Src;
        
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryForm form)
    {
        if (!ModelState.IsValid)
        {
            var cat = await _context.Categories.AsNoTracking().Include(c => c.Image).FirstOrDefaultAsync(c => c.Id == id);
            ViewBag.ImageSrc = cat?.Image?.Src;
            return View(form);
        }

        var categoryToUpdate = await _context.Categories.Include(c => c.Image).FirstOrDefaultAsync(c => c.Id == id);
        if (categoryToUpdate == null) return NotFound();

        categoryToUpdate.Name = form.Name;

        if (form.Image != null)
        {
            if (categoryToUpdate.Image != null)
            {
                await _fileStorage.DeleteFileAsync(categoryToUpdate.Image.FileName);
                _context.Images.Remove(categoryToUpdate.Image);
            }
            var fileName = await _fileStorage.SaveFileAsync(form.Image);
            categoryToUpdate.Image = new ImageUploaded { FileName = fileName };
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Categories.Any(e => e.Id == id)) return NotFound();
            else throw;
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.Include(c => c.Image).FirstOrDefaultAsync(c => c.Id == id);
        if (category?.Image != null)
        {
            await _fileStorage.DeleteFileAsync(category.Image.FileName);
            _context.Images.Remove(category.Image);
        }
        if (category != null) _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}