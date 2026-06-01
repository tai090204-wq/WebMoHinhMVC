using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebMoHinh.Data;
using WebMoHinh.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles =
    {
        "Admin",
        "Employee",
        "Company",
        "Customer"
    };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(
                new IdentityRole(role));
        }
    }

    var admin =
        await userManager.FindByNameAsync("admin");

    if (admin == null)
    {
        ApplicationUser user = new()
        {
            FullName = "Administrator",
            UserName = "admin",
            Email = "admin@gmail.com",
            Age = 25,
            Gender = "Nam",
            Job = "Admin",
            Address = "HCM"
        };

        var result =
            await userManager.CreateAsync(
                user,
                "Admin@123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(
                user,
                "Admin");
        }
    }
}

app.Run();