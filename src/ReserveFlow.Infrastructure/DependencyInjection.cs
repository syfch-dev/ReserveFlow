using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ReserveFlow.Application.Abstractions.Authentication;
using ReserveFlow.Domain.Abstractions;
using ReserveFlow.Domain.Users;
using ReserveFlow.Infrastructure.Authentication;
using ReserveFlow.Infrastructure.Observability;
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
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddObservability(configuration, environment);
        
        return services;
    }
}
