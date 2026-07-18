namespace ReserveFlow.Api.Options;

public sealed class HttpsOptions
{
    public const string SectionName = "Https";

    public int? Port { get; init; }
}
