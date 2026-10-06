using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MasterData.Cities.Commands.ImportCities;

/// <summary>
/// Command to import cities from a CSV file.
/// CSV format: Code,Name,CountryCode
/// Existing records (matched by Code) are updated; new records are inserted.
/// </summary>
public sealed record ImportCitiesCommand : IAppCommand<ImportResult>
{
    /// <summary>
    /// CSV file stream.
    /// </summary>
    public required Stream CsvStream { get; init; }
}
