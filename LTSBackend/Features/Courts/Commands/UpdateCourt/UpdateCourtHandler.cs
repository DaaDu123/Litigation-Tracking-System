using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Courts.Commands.UpdateCourt;

public sealed class UpdateCourtHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<UpdateCourtHandler> _logger) : IRequestHandler<UpdateCourtCommand, bool>
{
    // =====================================================
    // HANDLE — edits an existing court
    // Ownership check: a FirmAdmin may only edit their OWN firm's custom
    // court — never a global one or another firm's (returns
    // NotFound rather than Forbidden, so its existence isn't disclosed).
    // Also re-checks name uniqueness before saving.
    // =====================================================
    public async Task<bool> Handle(UpdateCourtCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating court: {CourtID}", request.CourtID);

        request = request with
        {
            CourtName = request.CourtName.Trim(),
            CourtType = request.CourtType?.Trim(),
            Jurisdiction = request.Jurisdiction?.Trim(),
            Address = request.Address?.Trim()
        };

        // 1. Find court (the query filter already hides other firms'
        // custom courts from a non-SuperAdmin caller)
        var court = await _context.Courts.FirstOrDefaultAsync(x => x.CourtID == request.CourtID, cancellationToken);

        if (court == null)
        {
            _logger.LogWarning("Update failed: Court not found: {CourtID}", request.CourtID);
            throw new NotFoundException("Court not found.");
        }

        // 1b. Ownership check: a FirmAdmin may edit only their OWN firm's
        // custom court - never a system-wide global court (FirmID
        // null), which would affect every other firm using it.
        if (court.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Update denied: user {UserId} attempted to edit a global/other-firm court {CourtID}", _currentUser.UserID, request.CourtID);
            throw new NotFoundException("Court not found.");
        }

        // 2. Name uniqueness check (self-excluding)
        bool nameExists = await _context.Courts.AnyAsync(x => x.CourtID != request.CourtID && x.CourtName.ToLower() == request.CourtName.ToLower(),cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning("Update failed: Court name already exists: {CourtName}", request.CourtName);
            throw new ValidationException(new()
            {
                $"Court '{request.CourtName}' already exists."
            });
        }

        // 3. Apply changes
        court.CourtName = request.CourtName;
        court.CourtType = request.CourtType;
        court.Jurisdiction = request.Jurisdiction;
        court.Address = request.Address;
        court.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Court updated successfully: {CourtID}", request.CourtID);

        return true;
    }
}
