using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseStatuses.Commands.DeleteCaseStatus;

public sealed class DeleteCaseStatusHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<DeleteCaseStatusHandler> _logger) : IRequestHandler<DeleteCaseStatusCommand, bool>
{
    // =====================================================
    // HANDLE — removes a case status the firm no longer needs
    // Same ownership check as Update (own firm's custom case status only),
    // plus an in-use check: cannot delete while cases still reference it.
    // =====================================================
    public async Task<bool> Handle(DeleteCaseStatusCommand request, CancellationToken cancellationToken)
    {
        var status = await _context.CaseStatuses.FirstOrDefaultAsync(x => x.StatusID == request.StatusID, cancellationToken);
        if (status == null)
        {
            _logger.LogWarning("Delete failed: Case status not found: {StatusID}", request.StatusID);
            throw new NotFoundException("Case status not found.");
        }

        if (status.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Delete denied: user {UserId} attempted to delete a global/other-firm status {StatusID}", _currentUser.UserID, request.StatusID);
            throw new NotFoundException("Case status not found.");
        }

        int caseCount = await _context.Cases.CountAsync(x => x.StatusID == request.StatusID, cancellationToken);
        if (caseCount > 0)
        {
            _logger.LogWarning("Delete failed: {Count} case(s) reference status: {StatusID}", caseCount, request.StatusID);
            throw new ValidationException(new()
            {
                $"Cannot delete status. {caseCount} case(s) are currently linked to it. Deactivate it instead."
            });
        }

        _context.CaseStatuses.Remove(status);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case status deleted successfully: {StatusID}", request.StatusID);

        return true;
    }
}
