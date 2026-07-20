using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ReserveFlow.Application.Abstractions.Authentication;
using ReserveFlow.Application.Diagnostics;
using ReserveFlow.Domain.Abstractions;
using ReserveFlow.Domain.Users;
using ReserveFlow.Infrastructure.Authentication;
using ReserveFlow.Infrastructure.Options;
using ReserveFlow.Infrastructure.Repositories;

namespace ReserveFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var database = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(database.ConnectionString);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddAuthentication(configuration);
        services.AddObservability(configuration, environment);
        return services;
    }

    private static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()!;
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });

        services.AddAuthorization();
    }

    private static void AddObservability(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var observabilityOptions = configuration
            .GetSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        if (observabilityOptions.Enabled)
        {
            var tracesEndpoint = new Uri(observabilityOptions.TracesEndpoint);
            var metricsEndpoint = new Uri(observabilityOptions.MetricsEndpoint);

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: observabilityOptions.ServiceName,
                    serviceNamespace: observabilityOptions.ServiceNamespace,
                    serviceVersion: typeof(DependencyInjection).Assembly.GetName().Version?.ToString(),
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
                    .AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = metricsEndpoint;
                        exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                    }));
        }
    }
}
