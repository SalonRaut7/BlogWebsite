using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyBlogApp.Data;
using MyBlogApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Configure Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;           // Must have at least one digit (0-9)
    options.Password.RequiredLength = 6;            // Minimum 6 characters
    options.Password.RequireNonAlphanumeric = true; // Must have special character (@#$%^&* etc.)
    options.Password.RequireUppercase = true;       // Must have at least one uppercase letter
    options.Password.RequireLowercase = true;       // Must have at least one lowercase letter

    // Lockout settings (after failed login attempts)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Lock for 5 minutes
    options.Lockout.MaxFailedAccessAttempts = 5;    // Lock after 5 failed attempts
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;         // Each email can only be used once

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;   // Don't require email confirmation (you can enable this later)
})
.AddEntityFrameworkStores<AppDbContext>()  // Tell Identity to use our AppDbContext
.AddDefaultTokenProviders();               // For password reset tokens, etc.

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";          // Redirect here if not logged in
    options.LogoutPath = "/Account/Logout";        // Logout path
    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect here if user doesn't have permission
    options.ExpireTimeSpan = TimeSpan.FromDays(7); // Cookie expires after 7 days
    options.SlidingExpiration = true;              // Reset expiration time on each request
});

var app = builder.Build();

// Seed Admin User 
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedAdminUser(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An error occurred while seeding the admin user: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();


app.UseAuthentication();  // Who are you? (checks if user is logged in)
app.UseAuthorization();   // What can you do? (checks permissions/roles)

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Post}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
