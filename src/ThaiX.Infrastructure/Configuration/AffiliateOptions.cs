namespace ThaiX.Infrastructure.Configuration;

public sealed class AffiliateOptions
{
    public const string SectionName = "Affiliate";

    public bool Enabled { get; set; }
    public string MexcAffiliateUrl { get; set; } = string.Empty;
}
