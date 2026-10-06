namespace ThaiX.Client.Services.Caching;

public sealed class ClientCacheOptions
{
    public const string SectionName = "ClientCache";

    public bool Enabled { get; set; } = true;
}
