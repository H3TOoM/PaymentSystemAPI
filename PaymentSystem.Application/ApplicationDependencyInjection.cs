using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PaymentSystem.Application.Common.Behaviors;

namespace PaymentSystem.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionMappingBehavior<,>));
        return services;
    }
}
