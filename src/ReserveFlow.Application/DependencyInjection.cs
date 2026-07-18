using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ReserveFlow.Application.Messaging;

namespace ReserveFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly);
        services.AddSingleton(TimeProvider.System);
        services.AddHandlersFromAssembly(assembly);

        return services;
    }

    private static void AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        Type[] openHandlerTypes =
        [
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<,>)
        ];

        foreach (var implementation in assembly.DefinedTypes.Where(IsConcreteClass))
        {
            foreach (var handlerInterface in GetHandlerInterfaces(implementation, openHandlerTypes))
            {
                services.AddScoped(handlerInterface, implementation);
            }
        }
    }

    private static bool IsConcreteClass(Type type) =>
        type is { IsAbstract: false, IsInterface: false };

    private static IEnumerable<Type> GetHandlerInterfaces(Type implementation, Type[] openHandlerTypes) =>
        implementation.GetInterfaces()
            .Where(i => i.IsGenericType && openHandlerTypes.Contains(i.GetGenericTypeDefinition()));
}
