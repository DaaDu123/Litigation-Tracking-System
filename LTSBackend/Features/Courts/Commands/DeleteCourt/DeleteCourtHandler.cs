using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Courts.Commands.DeleteCourt;

public sealed class DeleteCourtHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<DeleteCourtHandler> _logger): IRequestHandler<DeleteCourtCommand, bool>
{
    // =====================================================
    // HANDLE — removes a court the firm no longer needs
    // Same ownership check as Update (own firm's custom court only),
    // plus an in-use check: cannot delete while cases still reference it.
    // =====================================================
    public async Task<bool> Handle(DeleteCourtCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting court: {CourtID}", request.CourtID);

        // 1. Find court
        var court = await _context.Courts.FirstOrDefaultAsync(x => x.CourtID == request.CourtID, cancellationToken);

        if (court == null)
        {
            _logger.LogWarning("Delete failed: Court not found: {CourtID}", request.CourtID);
            throw new NotFoundException("Court not found.");
        }

        // 1b. Ownership check: a FirmAdmin may delete only their OWN firm's
        // custom court - never a system-wide global court, which other
        // firms may depend on.
        if (court.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Delete denied: user {UserId} attempted to delete a global/other-firm court {CourtID}", _currentUser.UserID, request.CourtID);
            throw new NotFoundException("Court not found.");
        }

        // 2. Block delete if cases reference this court
        int caseCount = await _context.Cases.CountAsync(x => x.CourtID == request.CourtID, cancellationToken);

        if (caseCount > 0)
        {
            _logger.LogWarning("Delete failed: {Count} case(s) reference court: {CourtID}",caseCount,request.CourtID);

            throw new ValidationException(new()
            {
                $"Cannot delete court. {caseCount} case(s) are currently linked to it."
            });
        }

        // 3. Block delete if hearings reference this court
        int hearingCount = await _context.Hearings.CountAsync(x => x.CourtID == request.CourtID, cancellationToken);

        if (hearingCount > 0)
        {
            _logger.LogWarning("Delete failed: {Count} hearing(s) reference court: {CourtID}",hearingCount,request.CourtID);

            throw new ValidationException(new()
            {
                $"Cannot delete court. {hearingCount} hearing record(s) are currently linked to it."
            });
        }

        // 4. Delete court
        _context.Courts.Remove(court);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Court deleted successfully: {CourtID}", request.CourtID);

        return true;
    }
}
