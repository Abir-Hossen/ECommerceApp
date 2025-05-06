using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ECommerceApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Identity options for development (disable email confirmation, relax password policy)
builder.Services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ECommerceApp.Models.ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register UserManager<ApplicationUser> and RoleManager<IdentityRole> for DI
builder.Services.AddScoped<UserManager<ECommerceApp.Models.ApplicationUser>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// Register a dummy email sender for development/testing
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, ECommerceApp.Services.DummyEmailSender>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();


// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ECommerceApp.Models.ApplicationUser>>();
    var dbContext = services.GetRequiredService<ECommerceApp.Data.ApplicationDbContext>();

    string[] roles = new[] { "Admin", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }


    // Seed default admin user
    var adminEmail = "admin@ecommerce.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ECommerceApp.Models.ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // Seed default customer user
    var customerEmail = "customer@ecommerce.com";
    var customerUser = await userManager.FindByEmailAsync(customerEmail);
    if (customerUser == null)
    {
        customerUser = new ECommerceApp.Models.ApplicationUser { UserName = customerEmail, Email = customerEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(customerUser, "Customer@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(customerUser, "Customer");
        }
    }

    // Seed initial products if none exist
    if (!dbContext.Products.Any())
    {
        dbContext.Products.AddRange(
            new ECommerceApp.Models.Product { Name = "Laptop", Price = 999.99m, Stock = 10 },
            new ECommerceApp.Models.Product { Name = "Smartphone", Price = 499.99m, Stock = 25 },
            new ECommerceApp.Models.Product { Name = "Headphones", Price = 79.99m, Stock = 50 },
            new ECommerceApp.Models.Product { Name = "Keyboard", Price = 39.99m, Stock = 30 },
            new ECommerceApp.Models.Product { Name = "Mouse", Price = 29.99m, Stock = 40 }
        );
        await dbContext.SaveChangesAsync();
    }
}

app.Run();
