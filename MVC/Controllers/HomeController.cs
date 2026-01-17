﻿using System.Diagnostics;
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
            .AsQueryable();

        if (searchForm.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.CategoryId == searchForm.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchForm.Query))
        {
            productsQuery = productsQuery.Where(p => p.Name.Contains(searchForm.Query));
        }

        var model = new HomeIndexPageModel
        {
            Categories = await _context.Categories.Include(c => c.Image).ToListAsync(),
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