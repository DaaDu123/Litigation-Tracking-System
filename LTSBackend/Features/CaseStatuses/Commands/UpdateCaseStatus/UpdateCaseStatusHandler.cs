using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseStatuses.Commands.UpdateCaseStatus;

public sealed class UpdateCaseStatusHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<UpdateCaseStatusHandler> _logger) : IRequestHandler<UpdateCaseStatusCommand, bool>
{
    // =====================================================
    // HANDLE — edits an existing case status
    // Ownership check: a FirmAdmin may only edit their OWN firm's custom
    // case status — never a global one or another firm's (returns
    // NotFound rather than Forbidden, so its existence isn't disclosed).
    // Also re-checks name uniqueness before saving.
    // =====================================================
    public async Task<bool> Handle(UpdateCaseStatusCommand request, CancellationToken cancellationToken)
    {
        request = request with { StatusName = request.StatusName.Trim(), ColorCode = request.ColorCode.Trim() };

        var status = await _context.CaseStatuses.FirstOrDefaultAsync(x => x.StatusID == request.StatusID, cancellationToken);
        if (status == null)
        {
            _logger.LogWarning("Update failed: Case status not found: {StatusID}", request.StatusID);
            throw new NotFoundException("Case status not found.");
        }

        if (status.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Update denied: user {UserId} attempted to edit a global/other-firm status {StatusID}", _currentUser.UserID, request.StatusID);
            throw new NotFoundException("Case status not found.");
        }

        bool nameExists = await _context.CaseStatuses.AnyAsync(x => x.StatusID != request.StatusID && x.StatusName.ToLower() == request.StatusName.ToLower(), cancellationToken);
        if (nameExists)
        {
            _logger.LogWarning("Update failed: Case status name already exists: {StatusName}", request.StatusName);
            throw new ValidationException(new() { $"Status '{request.StatusName}' already exists." });
        }

        status.StatusName = request.StatusName;
        status.SequenceNo = request.SequenceNo;
        status.ColorCode = request.ColorCode;
        status.IsClosed = request.IsClosed;
        status.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case status updated successfully: {StatusID}", request.StatusID);

        return true;
    }
}
