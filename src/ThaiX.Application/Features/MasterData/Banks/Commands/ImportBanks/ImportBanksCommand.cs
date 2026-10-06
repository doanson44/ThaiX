using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.MasterData.Banks.Commands.ImportBanks;

/// <summary>
/// Command to import banks from a CSV file.
/// CSV format: Code,Name,CountryCode
/// Existing records (matched by Code) are updated; new records are inserted.
/// </summary>
public sealed record ImportBanksCommand : IAppCommand<ImportResult>
{
    /// <summary>
    /// CSV file stream.
    /// </summary>
    public required Stream CsvStream { get; init; }
}
