using Microsoft.AspNetCore.Identity;
using MyBlogApp.Models;

namespace MyBlogApp.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Check if Admin user already exists
            var adminUser = await userManager.FindByEmailAsync("admin@myblog.com");
            
            if (adminUser == null)
            {
                // Create the Admin user
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@myblog.com",
                    EmailConfirmed = true,
                    RegisteredDate = DateTime.Now
                };

                // Create user with password
                var result = await userManager.CreateAsync(admin, "Admin@123");

                if (result.Succeeded)
                {
                    // Assign Admin role having email and password as:
                    await userManager.AddToRoleAsync(admin, "Admin");
                    Console.WriteLine("Admin user created successfully!");
                    Console.WriteLine("Email: admin@myblog.com");
                    Console.WriteLine("Password: Admin@123");
                }
                else
                {
                    Console.WriteLine("Failed to create admin user:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"   - {error.Description}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Admin user already exists.");
            }
        }
    }
}