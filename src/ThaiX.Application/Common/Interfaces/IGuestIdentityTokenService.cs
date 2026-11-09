namespace ThaiX.Application.Common.Interfaces;

public interface IGuestIdentityTokenService
{
    GuestIdentityToken Create();
    bool IsValid(Guid playerId, string? token);
}

public sealed record GuestIdentityToken(Guid PlayerId, string Token, DateTime ExpiresAtUtc);
