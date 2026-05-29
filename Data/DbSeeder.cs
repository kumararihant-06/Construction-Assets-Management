using ConstructionAssetAPI.Entities;
using ConstructionAssetAPI.Enums;
using Microsoft.AspNetCore.Identity;

namespace ConstructionAssetAPI.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        // Seed roles
        foreach (var role in Enum.GetNames<UserRole>())
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        // Seed bootstrap admin
        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@local.com";
        var adminPassword = configuration["AdminSeed:Password"] ?? "Admin@123";

        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Bootstrap Admin"
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, UserRole.Admin.ToString());
        }
    }
}