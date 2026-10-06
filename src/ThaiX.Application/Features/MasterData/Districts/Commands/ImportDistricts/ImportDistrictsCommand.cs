using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MasterData.Districts.Commands.ImportDistricts;

/// <summary>
/// Command to import districts from a CSV file.
/// CSV format: Code,Name,CityCode
/// Existing records (matched by Code) are updated; new records are inserted.
/// </summary>
public sealed record ImportDistrictsCommand : IAppCommand<ImportResult>
{
    /// <summary>
    /// CSV file stream.
    /// </summary>
    public required Stream CsvStream { get; init; }
}
