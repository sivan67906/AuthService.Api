using System;
using System.Threading.Tasks;
using AuthService.Domain.Constants;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Persistence;

public static class CommandDbContextSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var context = scope.ServiceProvider.GetRequiredService<CommandDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = Roles.Admin, NormalizedName = Roles.Admin.ToUpper(), Description = "Administrator" });
        }
        if (!await roleManager.RoleExistsAsync(Roles.User))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = Roles.User, NormalizedName = Roles.User.ToUpper(), Description = "Standard user" });
        }

        var adminEmail = "admin@example.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                UserName = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(admin, "Admin@123");
            if (!createResult.Succeeded)
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(";", createResult.Errors));
                return;
            }

            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
