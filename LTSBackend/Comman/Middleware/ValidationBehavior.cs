using FluentValidation;
using MediatR;

namespace LTSBackend.Comman.Middleware;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(x => x is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var errorMessages = failures.Select(x => x.ErrorMessage).ToList();

            _logger.LogWarning(
                "Validation failed for request {RequestType}. Errors: {Errors}",
                typeof(TRequest).Name,
                string.Join(", ", errorMessages));
            throw new LTSBackend.Comman.Exceptions.ValidationException(errorMessages);
        }

        return await next();
    }
}