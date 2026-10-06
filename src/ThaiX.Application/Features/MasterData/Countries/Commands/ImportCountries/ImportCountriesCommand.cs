using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MasterData.Countries.Commands.ImportCountries;

/// <summary>
/// Command to import countries from a CSV file.
/// CSV format: Code,Name
/// Existing records (matched by Code) are updated; new records are inserted.
/// </summary>
public sealed record ImportCountriesCommand : IAppCommand<ImportResult>
{
    /// <summary>
    /// CSV file stream.
    /// </summary>
    public required Stream CsvStream { get; init; }
}
