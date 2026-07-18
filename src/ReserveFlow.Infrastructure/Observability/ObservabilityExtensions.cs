using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ReserveFlow.Application.Diagnostics;
using ReserveFlow.Infrastructure.Options;

namespace ReserveFlow.Infrastructure.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var options = configuration
            .GetSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        if (!options.Enabled)
        {
            return services;
        }

        var tracesEndpoint = new Uri(options.TracesEndpoint);
        var metricsEndpoint = new Uri(options.MetricsEndpoint);

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: options.ServiceName,
                serviceNamespace: options.ServiceNamespace,
                serviceVersion: typeof(ObservabilityExtensions).Assembly.GetName().Version?.ToString(),
                serviceInstanceId: Environment.MachineName)
            .AddAttributes([
                new KeyValuePair<string, object>("deployment.environment", environment.EnvironmentName)
            ]);

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(resourceBuilder)
                .AddSource(ApplicationDiagnostics.ActivitySourceName)
                .AddSource("Npgsql")
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddHttpClientInstrumentation()
                // Send traces to Jaeger's OTLP/gRPC receiver.
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = tracesEndpoint;
                    exporter.Protocol = OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(resourceBuilder)
                .AddMeter(ApplicationDiagnostics.MeterName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                // Send metrics to Prometheus's OTLP/HTTP receiver.
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = metricsEndpoint;
                    exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                }));

        return services;
    }
}
