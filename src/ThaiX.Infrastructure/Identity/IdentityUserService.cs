using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Infrastructure.Identity;

/// <summary>
/// Service for managing Identity users and their permissions.
/// Handles permission claim synchronization.
/// </summary>
public sealed class IdentityUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Synchronizes user permissions by replacing all existing permission claims.
    /// </summary>
    public async Task SyncPermissionsAsync(Guid userId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new InvalidOperationException($"User with ID '{userId}' not found.");

        // Remove existing permission claims
        var existingClaims = await _userManager.GetClaimsAsync(user);
        var permissionClaims = existingClaims
            .Where(c => c.Type == ClaimTypeConstants.Permission)
            .ToList();

        if (permissionClaims.Any())
        {
            var removeResult = await _userManager.RemoveClaimsAsync(user, permissionClaims);
            if (!removeResult.Succeeded)
                throw new InvalidOperationException($"Failed to remove existing permission claims: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
        }

        // Add new permission claims
        var newClaims = permissions
            .Distinct()
            .Select(p => new Claim(ClaimTypeConstants.Permission, p))
            .ToList();

        if (newClaims.Any())
        {
            var addResult = await _userManager.AddClaimsAsync(user, newClaims);
            if (!addResult.Succeeded)
                throw new InvalidOperationException($"Failed to add permission claims: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
        }
    }

    /// <summary>
    /// Adds a single permission to the user.
    /// </summary>
    public async Task AddPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new InvalidOperationException($"User with ID '{userId}' not found.");

        var existingClaims = await _userManager.GetClaimsAsync(user);
        if (existingClaims.Any(c => c.Type == ClaimTypeConstants.Permission && c.Value == permission))
            return; // Permission already exists

        var result = await _userManager.AddClaimAsync(user, new Claim(ClaimTypeConstants.Permission, permission));
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to add permission: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    /// <summary>
    /// Removes a single permission from the user.
    /// </summary>
    public async Task RemovePermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new InvalidOperationException($"User with ID '{userId}' not found.");

        var existingClaims = await _userManager.GetClaimsAsync(user);
        var permissionClaim = existingClaims
            .FirstOrDefault(c => c.Type == ClaimTypeConstants.Permission && c.Value == permission);

        if (permissionClaim == null)
            return; // Permission doesn't exist

        var result = await _userManager.RemoveClaimAsync(user, permissionClaim);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to remove permission: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    /// <summary>
    /// Gets all permissions for a user.
    /// </summary>
    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Array.Empty<string>();

        var claims = await _userManager.GetClaimsAsync(user);
        return claims
            .Where(c => c.Type == ClaimTypeConstants.Permission)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets all users with a specific permission.
    /// </summary>
    public async Task<IReadOnlyCollection<ApplicationUser>> GetUsersWithPermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        var usersWithClaim = await _userManager.GetUsersForClaimAsync(
            new Claim(ClaimTypeConstants.Permission, permission));

        return usersWithClaim.ToList().AsReadOnly();
    }

    /// <summary>
    /// Synchronizes permissions from role membership.
    /// This is called when a user's roles change.
    /// </summary>
    public async Task SyncPermissionsFromRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new InvalidOperationException($"User with ID '{userId}' not found.");

        // Get all roles for the user
        var roleNames = await _userManager.GetRolesAsync(user);

        // For each role, get permissions (this would be implemented based on your role-permission mapping)
        // For now, we'll use a simple mapping:
        // - Admin role gets all permissions
        // - Other roles get specific permissions based on role name

        var permissions = new HashSet<string>();

        foreach (var roleName in roleNames)
        {
            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Admin gets all permissions
                permissions.UnionWith(Domain.Common.Constants.Permissions.GetAdminPermissions());
            }
            else
            {
                // Map role names to permissions (customize as needed)
                var rolePermissions = GetPermissionsForRole(roleName);
                permissions.UnionWith(rolePermissions);
            }
        }

        await SyncPermissionsAsync(userId, permissions, cancellationToken);
    }

    /// <summary>
    /// Maps role names to permissions.
    /// TODO: Replace with database-driven role-permission mapping.
    /// </summary>
    private static IEnumerable<string> GetPermissionsForRole(string roleName)
    {
        return roleName.ToLowerInvariant() switch
        {
            "admin" => Domain.Common.Constants.Permissions.GetAdminPermissions(),
            "user-manager" => new[]
            {
                Domain.Common.Constants.Permissions.UserRead,
                Domain.Common.Constants.Permissions.UserWrite,
                Domain.Common.Constants.Permissions.RoleRead
            },
            "viewer" => new[]
            {
                Domain.Common.Constants.Permissions.UserRead,
                Domain.Common.Constants.Permissions.RoleRead
            },
            _ => Array.Empty<string>()
        };
    }
}
