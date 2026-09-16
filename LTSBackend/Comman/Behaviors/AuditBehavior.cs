using System.Security.Claims;
using LTSBackend.Services.Audit;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LTSBackend.Comman.Behaviors;

public class AuditBehavior<TRequest, TResponse>(IAuditService _auditService, IAuditLogService _auditLogService, IHttpContextAccessor httpContextAccessor,
        ILogger<AuditBehavior<TRequest, TResponse>> _logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        bool isCommand = typeof(TRequest).Name.EndsWith("Command");
        var commandName = typeof(TRequest).Name;
        if (isCommand)
        {
            _logger.LogDebug("Executing command: {CommandName}", commandName);
        }
        try
        {
            var response = await next();
            if (isCommand)
            {
                int? actingUserId = null;
                var claim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null && int.TryParse(claim.Value, out var uid))
                {
                    actingUserId = uid;
                }
                var auditLog = _auditService.Create(actingUserId, $"Command executed: {commandName}");
                await _auditLogService.LogAsync(auditLog);
                _logger.LogDebug("Audit logged for command: {CommandName} by user {UserId}", commandName, actingUserId);
            }
            return response;
        }
        catch (Exception ex)
        {
            if (isCommand)
            {
                _logger.LogError(ex, "Command failed: {CommandName}", commandName);
            }
            throw;
        }
    }
}