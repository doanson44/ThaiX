using Microsoft.AspNetCore.Authorization;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Infrastructure.Security;

/// <summary>
/// Authorization handler for permission-based requirements.
/// Checks if the user has the required permission claim.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasClaim(c =>
            c.Type == ClaimTypeConstants.Permission &&
            c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Represents a permission-based authorization requirement.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}
