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