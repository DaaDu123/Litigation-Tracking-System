using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Cases.Commands.ArchiveCase;

public sealed class ArchiveCaseHandler(AppDbContext _context, IAuditService _auditService, ICurrentUserService _currentUser, ILogger<ArchiveCaseHandler> _logger) : IRequestHandler<ArchiveCaseCommand, bool>
{







    public async Task<bool> Handle(ArchiveCaseCommand request, CancellationToken cancellationToken)
    {
        int userId = _currentUser.UserID ?? throw new UnauthorizedException("Unable to determine the current user's identity.");

        var c = await _context.Cases.FirstOrDefaultAsync(x => x.CaseID == request.CaseID && x.FirmID == _currentUser.FirmID, cancellationToken);
        if (c == null)
            throw new NotFoundException($"Case ID {request.CaseID} not found");

        if (c.IsArchived)
            throw new ValidationException(["This case is already archived."]);

        string? reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

        c.IsArchived = true;
        c.ArchivedDate = DateTime.UtcNow;
        c.ArchivedBy = userId;
        c.ArchiveReason = reason;
        c.ModifiedBy = userId;
        c.ModifiedDate = DateTime.UtcNow;

        _context.AuditLogs.Add(_auditService.Create(userId, $"Case Archive: {c.CaseNumber}" + (reason == null ? "" : $" — {reason}")));
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case archived: {CaseID} by {UserID}", c.CaseID, userId);
        return true;
    }
}
