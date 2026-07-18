namespace ReserveFlow.Infrastructure.Options;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public bool Enabled { get; init; } = true;

    public string ServiceName { get; init; } = "ReserveFlow.Api";

    public string ServiceNamespace { get; init; } = "ReserveFlow";

    /// <summary>
    /// OTLP/gRPC endpoint that receives traces (Jaeger's OTLP receiver).
    /// Local: http://localhost:4317; inside a container: http://jaeger:4317.
    /// </summary>
    public string TracesEndpoint { get; init; } = "http://localhost:4317";

    /// <summary>
    /// OTLP/HTTP endpoint that receives metrics (Prometheus's native OTLP receiver).
    /// Programmatic configuration must include the signal path (/v1/metrics).
    /// Local: http://localhost:9090/api/v1/otlp/v1/metrics.
    /// </summary>
    public string MetricsEndpoint { get; init; } = "http://localhost:9090/api/v1/otlp/v1/metrics";
}
