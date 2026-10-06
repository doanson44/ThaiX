using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Portfolios.Queries.ExportPortfolios;

public sealed record ExportPortfoliosQuery : IAppQuery<ExportPortfoliosResult>
{
    public string? SearchTerm { get; init; }
}

public sealed record ExportPortfoliosResult
{
    public required byte[] FileContent { get; init; }
    public required string FileName { get; init; }
}
