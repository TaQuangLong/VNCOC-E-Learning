using ChurchLearn.Api.Domain.Entities;
using ChurchLearn.Api.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ChurchLearn.Api.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        string[] roles = [AppRoles.Student, AppRoles.Admin, AppRoles.SuperAdmin];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var email = config["SuperAdmin:Email"] ?? "superadmin@churchlearn.local";
        var password = config["SuperAdmin:Password"] ?? "Admin@123456!";

        if (await userManager.FindByEmailAsync(email) is null)
        {
            var superAdmin = new AppUser
            {
                UserName = email,
                Email = email,
                DisplayName = "Super Admin",
                IsActive = true,
                EmailConfirmed = true,
            };
            var result = await userManager.CreateAsync(superAdmin, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
        }
    }
}
