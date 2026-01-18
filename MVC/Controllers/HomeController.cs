﻿﻿﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Models.Forms;
using MVC.Models.PageModels;

namespace MVC.Controllers;

public class HomeController : Controller
{
    private readonly StoreContext _context;

    public HomeController(StoreContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index([FromQuery] HomePageSearchForm searchForm)
    {
        var productsQuery = _context.Products
            .Include(p => p.Images)
            .AsSplitQuery()
            .AsQueryable();

        // Сначала получаем все категории
        var allCategories = await _context.Categories.Include(c => c.Image).ToListAsync();

        if (searchForm.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.CategoryId == searchForm.CategoryId.Value);
            // Находим имя категории и сохраняем в ViewBag
            ViewBag.ListTitle = allCategories.FirstOrDefault(c => c.Id == searchForm.CategoryId.Value)?.Name ?? "Products";
        }
        else
        {
            // Если категория не выбрана, используем заголовок по умолчанию
            ViewBag.ListTitle = "All Products";
        }
        
        if (!string.IsNullOrWhiteSpace(searchForm.Query))
        {
            productsQuery = productsQuery.Where(p => p.Name.Contains(searchForm.Query));
        }

        var model = new HomeIndexPageModel
        {
            Categories = allCategories, // Используем уже полученный список
            Products = await productsQuery.ToListAsync(),
            SearchForm = searchForm
        };

        return View(model);
    }

    public async Task<IActionResult> Product(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    
    public IActionResult Privacy()
    {
        return View();
    }
}