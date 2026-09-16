using FluentValidation;
using LTSBackend.Comman.Behaviors;
using LTSBackend.Comman.Middleware;
using MediatR;

namespace LTSBackend.Extensions;

public static class MediatRExtensions
{
    public static IServiceCollection AddAppMediatR(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(Program));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

        // Order matters: validate before auditing, so a request that fails
        // validation never gets an audit entry for something that didn't happen.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));

        return services;
    }
}
