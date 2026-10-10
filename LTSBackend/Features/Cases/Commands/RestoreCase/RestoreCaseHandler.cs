using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Cases.Commands.RestoreCase;

public sealed class RestoreCaseHandler(AppDbContext _context, IAuditService _auditService, ICurrentUserService _currentUser, ILogger<RestoreCaseHandler> _logger) : IRequestHandler<RestoreCaseCommand, bool>
{
    // =====================================================
    // HANDLE — restores an archived case (firm-scoped), clears the archive
    // markers and writes an audit line.
    // =====================================================
    public async Task<bool> Handle(RestoreCaseCommand request, CancellationToken cancellationToken)
    {
        int userId = _currentUser.UserID ?? throw new UnauthorizedException("Unable to determine the current user's identity.");

        var c = await _context.Cases.FirstOrDefaultAsync(x => x.CaseID == request.CaseID && x.FirmID == _currentUser.FirmID, cancellationToken);
        if (c == null)
            throw new NotFoundException($"Case ID {request.CaseID} not found");

        if (!c.IsArchived)
            throw new ValidationException(["This case is not archived."]);

        c.IsArchived = false;
        c.ArchivedDate = null;
        c.ArchivedBy = null;
        c.ArchiveReason = null;
        c.ModifiedBy = userId;
        c.ModifiedDate = DateTime.UtcNow;

        _context.AuditLogs.Add(_auditService.Create(userId, $"Case Restore: {c.CaseNumber}"));
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case restored: {CaseID} by {UserID}", c.CaseID, userId);
        return true;
    }
}
