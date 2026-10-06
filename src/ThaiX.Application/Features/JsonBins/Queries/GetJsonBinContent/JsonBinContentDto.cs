namespace ThaiX.Application.Features.JsonBins.Queries.GetJsonBinContent;

public sealed record JsonBinContentDto
{
    public required Guid Id { get; init; }
    public required string ContentType { get; init; }
    public required bool IsCompressed { get; init; }
    public required byte[] Content { get; init; }
    public required long SizeBytes { get; init; }
}
