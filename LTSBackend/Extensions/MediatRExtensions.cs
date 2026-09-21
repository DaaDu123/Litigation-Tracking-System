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

        // Order matters (behaviors run in registration order, outermost
        // first):
        //   1. AccountStatusGuardBehavior - reject blocked/removed/deleted
        //      accounts and blocked firms before anything else runs.
        //   2. ProfileCompletionBehavior - reject access to everything
        //      except the profile-completion allowlist until done.
        //   3. ValidationBehavior - validate before auditing, so a request
        //      that fails validation never gets an audit entry for
        //      something that didn't happen.
        //   4. AuditBehavior - last, so it only logs requests that passed
        //      every earlier gate.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AccountStatusGuardBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ProfileCompletionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));

        return services;
    }
}
