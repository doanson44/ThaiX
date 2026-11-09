using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Infrastructure.Services.Games;

public sealed class GuestIdentityTokenService : IGuestIdentityTokenService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(30);
    private readonly IDataProtector _protector;

    public GuestIdentityTokenService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("ThaiX.Games.GuestIdentity.v1");
    }

    public GuestIdentityToken Create()
    {
        var playerId = Guid.NewGuid();
        var expiresAtUtc = DateTime.UtcNow.Add(TokenLifetime);
        var payload = $"{playerId:N}|{expiresAtUtc.Ticks.ToString(CultureInfo.InvariantCulture)}";
        return new GuestIdentityToken(playerId, _protector.Protect(payload), expiresAtUtc);
    }

    public bool IsValid(Guid playerId, string? token)
    {
        if (playerId == Guid.Empty || string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var payload = _protector.Unprotect(token);
            var parts = payload.Split('|', 2);
            return parts.Length == 2
                && Guid.TryParseExact(parts[0], "N", out var tokenPlayerId)
                && tokenPlayerId == playerId
                && long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
                && ticks > DateTime.UtcNow.Ticks;
        }
        catch
        {
            return false;
        }
    }
}
