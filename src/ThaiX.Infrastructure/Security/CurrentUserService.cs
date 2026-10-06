using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Infrastructure.Security;

/// <summary>
/// Implementation of ICurrentUserService using ASP.NET Core HttpContext.
/// Provides access to current user identity and permissions.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    public string Email
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        }
    }

    public string UserName
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.Name)
                ?? user?.Identity?.Name
                ?? user?.FindFirstValue(ClaimTypes.Email)
                ?? string.Empty;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }

    public IReadOnlyCollection<string> GetPermissions()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return Array.Empty<string>();

        return user.FindAll(Application.Common.Constants.ClaimTypeConstants.Permission)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();
    }

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return false;

        return user.HasClaim(Application.Common.Constants.ClaimTypeConstants.Permission, permission);
    }

    public bool HasAllPermissions(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            return false;

        var userPermissions = GetPermissions();
        return permissions.All(p => userPermissions.Contains(p));
    }

    public bool HasAnyPermission(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            return false;

        var userPermissions = GetPermissions();
        return permissions.Any(p => userPermissions.Contains(p));
    }
}
