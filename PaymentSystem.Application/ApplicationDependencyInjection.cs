using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentSystem.Application.Common.Behaviors;
using PaymentSystem.Application.Services;

namespace PaymentSystem.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return AddApplication(services, null);
    }

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration? configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionMappingBehavior<,>));
        services.AddValidatorsFromAssembly(typeof(ApplicationDependencyInjection).Assembly);

        services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
