using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ThaiX.Application.Common.Constants;
using ThaiX.Infrastructure.Identity;

namespace ThaiX.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

            // Apply pending migrations
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            // Seed roles
            await SeedRolesAsync(roleManager, logger);

            // Seed admin user with permissions
            await SeedAdminUserAsync(userManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        var roles = new[] { "Admin", "User-Manager", "Viewer" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogInformation("Creating {Role} role...", roleName);
                var result = await roleManager.CreateAsync(new ApplicationRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });

                if (result.Succeeded)
                {
                    logger.LogInformation("{Role} role created successfully", roleName);
                }
                else
                {
                    logger.LogError("Failed to create {Role} role: {Errors}", roleName,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        const string adminEmail = "admin@thaix.tryasp.net";
        const string username = "admin";
        const string adminPassword = "P@ssw0rd"; // Change in production

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            logger.LogInformation("Creating admin user...");
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = username,
                Email = adminEmail,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (!result.Succeeded)
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new InvalidOperationException(
                    $"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            logger.LogInformation("Admin user created successfully");

            // Assign Admin role
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Admin user assigned to Admin role");

            // Add all permissions as claims
            var permissions = Domain.Common.Constants.Permissions.GetAdminPermissions();
            var permissionClaims = permissions.Select(p => new Claim(ClaimTypeConstants.Permission, p));

            await userManager.AddClaimsAsync(adminUser, permissionClaims);
            logger.LogInformation("Admin user permissions synchronized ({Count} permissions)", permissions.Count);
        }
        else
        {
            logger.LogInformation("Admin user already exists");

            // Update existing admin user permissions
            var existingClaims = await userManager.GetClaimsAsync(adminUser);
            var existingPermissions = existingClaims
                .Where(c => c.Type == ClaimTypeConstants.Permission)
                .Select(c => c.Value)
                .ToHashSet();

            var allPermissions = Domain.Common.Constants.Permissions.GetAdminPermissions();

            // Add missing permissions
            var missingPermissions = allPermissions
                .Where(p => !existingPermissions.Contains(p))
                .Select(p => new Claim(ClaimTypeConstants.Permission, p))
                .ToList();

            if (missingPermissions.Any())
            {
                await userManager.AddClaimsAsync(adminUser, missingPermissions);
                logger.LogInformation("Added {Count} missing permissions to admin user", missingPermissions.Count);
            }
        }
    }
}
