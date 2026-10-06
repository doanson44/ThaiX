using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.JsonBins.Queries.ValidateJsonBinCode;

/// <summary>
/// Checks whether a JsonBin code already exists (among non-deleted rows).
/// Pass <see cref="ExcludeId"/> when editing so the current row is ignored.
/// </summary>
public sealed record ValidateJsonBinCodeQuery : IAppQuery<ValidateJsonBinCodeResult>
{
    public required string Code { get; init; }
    public Guid? ExcludeId { get; init; }
}

public sealed record ValidateJsonBinCodeResult
{
    public required string Code { get; init; }
    public required bool Exists { get; init; }
    public bool IsAvailable => !Exists;
}
