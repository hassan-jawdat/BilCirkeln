using BilCirkeln.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BilCirkeln.Api.Data;

// Runs at startup: applies migrations, creates the roles and one staff account.
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await db.Database.MigrateAsync();

        foreach (var role in new[] { Roles.Customer, Roles.Staff })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // The staff login comes from user secrets, so no password is in the code.
        var email = config["SeedStaff:Email"];
        var password = config["SeedStaff:Password"];
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            logger.LogWarning("SeedStaff:Email/Password saknas i user secrets - inget personalkonto skapades.");
            return;
        }

        if (await userManager.FindByEmailAsync(email) == null)
        {
            var staff = new ApplicationUser { UserName = email, Email = email, FullName = "Personal" };
            var result = await userManager.CreateAsync(staff, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(staff, Roles.Staff);
            }
            else
            {
                logger.LogWarning("Kunde inte skapa personalkonto: {Errors}",
                    string.Join(" ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
