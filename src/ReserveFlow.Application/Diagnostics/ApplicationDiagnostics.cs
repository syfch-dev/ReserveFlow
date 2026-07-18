using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ReserveFlow.Application.Diagnostics;

/// <summary>
/// The single source for custom traces and metrics produced by the Application layer.
/// <see cref="ActivitySource"/> and <see cref="Meter"/> are BCL primitives, so they
/// can be used in the Application layer without violating dependency rules.
/// Exporter configuration (OTLP, etc.) remains in the Infrastructure layer.
/// </summary>
public static class ApplicationDiagnostics
{
    public const string ActivitySourceName = "ReserveFlow.Application";
    public const string MeterName = "ReserveFlow.Application";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private static readonly Meter Meter = new(MeterName);

    public static readonly Counter<long> UsersRegistered = Meter.CreateCounter<long>(
        "reserveflow.users.registered",
        unit: "{user}",
        description: "Number of successfully registered users.");
}
