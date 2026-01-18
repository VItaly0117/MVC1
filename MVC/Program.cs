using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Models.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Register StoreContext with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlite(connectionString));

// 2. Add Identity with custom settings
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
    {
        // Lenient password settings for testing purposes
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 3;
    })
    .AddEntityFrameworkStores<StoreContext>();

// 3. Register our custom file service and HttpContextAccessor
builder.Services.AddScoped<IFileStorage, FileStorage>();
builder.Services.AddHttpContextAccessor();

// 4. Configure Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Account/Login"; // Path to the login page
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 5. Initialize Admin and Manager roles on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var roles = new[] { RoleConstants.Admin, RoleConstants.Manager };
    var userManager = services.GetRequiredService<UserManager<User>>();
    var context = services.GetRequiredService<StoreContext>();
    await context.Database.EnsureCreatedAsync();

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
    }
    
    // Seed Admin User if no users exist
    if (!userManager.Users.Any())
    {
        var adminUser = new User { UserName = "admin@example.com", Email = "admin@example.com", FullName = "Admin Owner", EmailConfirmed = true };
        var result = await userManager.CreateAsync(adminUser, "123"); // Simple password for development
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, RoleConstants.Admin);
        }
    }
    
    // Seed Database with sample data
    if (!context.Products.Any())
    {
        var tagNew = new Tag { Name = "New" };
        var tagGaming = new Tag { Name = "Gaming" };
        var tagForWork = new Tag { Name = "For Work" };
        var tagApple = new Tag { Name = "Apple" };
        var tagAndroid = new Tag { Name = "Android" };
        var tagBestSeller = new Tag { Name = "Best Seller" };

        var catLaptops = new Category { Name = "Laptops" };
        var catPhones = new Category { Name = "Smartphones" };
        var catAccessories = new Category { Name = "Accessories" };

        var products = new List<Product>
        {
            new() { Name = "MacBook Pro 16", Price = 2499.99m, Quantity = 15, Category = catLaptops, Tags = new List<Tag> { tagForWork, tagApple, tagBestSeller }, Description = "The most powerful MacBook Pro ever." },
            new() { Name = "Dell XPS 15", Price = 1899.00m, Quantity = 20, Category = catLaptops, Tags = new List<Tag> { tagForWork }, Description = "Stunning display and powerful performance." },
            new() { Name = "Razer Blade 15", Price = 2199.50m, Quantity = 10, Category = catLaptops, Tags = new List<Tag> { tagGaming }, Description = "The ultimate gaming laptop." },
            new() { Name = "iPhone 15 Pro", Price = 999.00m, Quantity = 50, Category = catPhones, Tags = new List<Tag> { tagNew, tagApple, tagBestSeller }, Description = "The latest and greatest from Apple." },
            new() { Name = "Samsung Galaxy S24 Ultra", Price = 1199.00m, Quantity = 45, Category = catPhones, Tags = new List<Tag> { tagNew, tagAndroid }, Description = "Experience the new standard of mobile AI." },
            new() { Name = "Google Pixel 8 Pro", Price = 899.00m, Quantity = 30, Category = catPhones, Tags = new List<Tag> { tagAndroid }, Description = "The power of Google AI in your hand." },
            new() { Name = "Logitech MX Master 3S", Price = 99.99m, Quantity = 100, Category = catAccessories, Tags = new List<Tag> { tagForWork, tagBestSeller }, Description = "An icon remastered for performance." },
            new() { Name = "Apple Magic Keyboard", Price = 149.00m, Quantity = 75, Category = catAccessories, Tags = new List<Tag> { tagApple }, Description = "A remarkably comfortable and precise typing experience." },
            new() { Name = "Sony WH-1000XM5 Headphones", Price = 399.99m, Quantity = 40, Category = catAccessories, Tags = new List<Tag> { tagBestSeller }, Description = "Industry-leading noise cancellation." },
            new() { Name = "Anker 737 Power Bank", Price = 129.99m, Quantity = 60, Category = catAccessories, Description = "High-speed, high-capacity portable charging." }
        };
        await context.Products.AddRangeAsync(products);

        await context.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANT: Authentication must be placed before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();