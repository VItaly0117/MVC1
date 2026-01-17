using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Models.Forms;
using MVC.Models.Services;

namespace MVC.Controllers;

[Authorize(Roles = RoleConstants.Manager)]
public class ProductController : Controller
{
    private readonly StoreContext _context;
    private readonly IFileStorage _fileStorage;

    public ProductController(StoreContext context, IFileStorage fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _context.Products.Include(p => p.Category).ToListAsync();
        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        var form = new ProductForm();
        await PrepareProductFormData(form);
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductForm form)
    {
        if (!ModelState.IsValid)
        {
            await PrepareProductFormData(form);
            return View(form);
        }

        var product = new Product
        {
            Name = form.Name,
            Price = form.Price,
            Quantity = form.Quantity,
            Description = form.Description,
            CategoryId = form.CategoryId
        };

        // Handle images
        foreach (var formFile in form.Images)
        {
            var fileName = await _fileStorage.SaveFileAsync(formFile);
            product.Images.Add(new ImageUploaded { FileName = fileName });
        }

        // Handle tags
        var selectedTagIds = form.TagsForm.Items.Where(t => t.IsSelected).Select(t => t.Id);
        var selectedTags = await _context.Tags.Where(t => selectedTagIds.Contains(t.Id)).ToListAsync();
        product.Tags = selectedTags;

        _context.Add(product);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id.Value);

        if (product == null) return NotFound();

        var form = new ProductForm
        {
            Name = product.Name,
            Price = product.Price,
            Quantity = product.Quantity,
            Description = product.Description,
            CategoryId = product.CategoryId,
        };

        await PrepareProductFormData(form, product);
        ViewBag.Product = product; // Pass product to view to show existing images
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductForm form)
    {
        if (!ModelState.IsValid)
        {
            var product = await _context.Products.AsNoTracking().Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            await PrepareProductFormData(form, product);
            ViewBag.Product = product;
            return View(form);
        }

        var productToUpdate = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (productToUpdate == null) return NotFound();

        productToUpdate.Name = form.Name;
        productToUpdate.Price = form.Price;
        productToUpdate.Quantity = form.Quantity;
        productToUpdate.Description = form.Description;
        productToUpdate.CategoryId = form.CategoryId;

        // Handle new images
        foreach (var formFile in form.Images)
        {
            var fileName = await _fileStorage.SaveFileAsync(formFile);
            productToUpdate.Images.Add(new ImageUploaded { FileName = fileName });
        }

        // Handle tags
        productToUpdate.Tags.Clear();
        var selectedTagIds = form.TagsForm.Items.Where(t => t.IsSelected).Select(t => t.Id);
        var selectedTags = await _context.Tags.Where(t => selectedTagIds.Contains(t.Id)).ToListAsync();
        productToUpdate.Tags = selectedTags;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.Id == id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product != null)
        {
            // Delete all associated images
            foreach (var image in product.Images)
            {
                await _fileStorage.DeleteFileAsync(image.FileName);
                _context.Images.Remove(image);
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var image = await _context.Images.FindAsync(id);
        if (image == null) return NotFound();

        await _fileStorage.DeleteFileAsync(image.FileName);
        _context.Images.Remove(image);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private async Task PrepareProductFormData(ProductForm form, Product? product = null)
    {
        // Categories for dropdown
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", form.CategoryId);

        // Tags for checkboxes
        var allTags = await _context.Tags.ToListAsync();
        var productTagIds = product?.Tags.Select(t => t.Id).ToHashSet() ?? new HashSet<int>();

        form.TagsForm.Items = allTags.Select(tag => new ProductTagsFormItem
        {
            Id = tag.Id,
            Name = tag.Name,
            IsSelected = productTagIds.Contains(tag.Id)
        }).ToList();
    }
}