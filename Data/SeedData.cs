using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PLM.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roleNames = { "Admin", "Engineering User", "Approver", "Operations User" };

            // Seed Roles
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed users for each role
            var seedUsers = new[]
            {
                new { Email = "admin@plm.local", Password = "Admin@123!", Role = "Admin" },
                new { Email = "engineer@plm.local", Password = "Engineer@123!", Role = "Engineering User" },
                new { Email = "approver@plm.local", Password = "Approver@123!", Role = "Approver" },
                new { Email = "operations@plm.local", Password = "Operations@123!", Role = "Operations User" }
            };

            foreach (var seedUser in seedUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(seedUser.Email);
                if (existingUser == null)
                {
                    var newUser = new IdentityUser
                    {
                        UserName = seedUser.Email,
                        Email = seedUser.Email,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(newUser, seedUser.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, seedUser.Role);
                    }
                }
            }
        }
    }
}
