using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactAddress;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record AddContactAddressCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required string Street { get; init; }
    public required string CountryCode { get; init; }
    public string CityCode { get; init; } = string.Empty;
    public string DistrictCode { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}
