using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Firms.Commands.BlockFirm;

public class BlockFirmCommandHandler(AppDbContext _context, ILogger<BlockFirmCommandHandler> _logger) : IRequestHandler<BlockFirmCommand, bool>
{
    // =====================================================
    // HANDLE — suspends a firm workspace platform-wide
    // Marks the firm blocked (with reason/timestamp/who), then revokes
    // every active refresh token belonging to that firm's users so
    // already-logged-in sessions are cut off immediately too, not just
    // future login attempts.
    // =====================================================
    public async Task<bool> Handle(BlockFirmCommand request, CancellationToken cancellationToken)
    {
        var firm = await _context.Firms.FirstOrDefaultAsync(x => x.FirmID == request.FirmID, cancellationToken);
        if (firm == null || firm.IsDeleted)
            throw new NotFoundException("Firm not found.");

        firm.IsBlocked = true;
        firm.BlockedReason = request.Reason;
        firm.BlockedAt = DateTime.UtcNow;
        firm.BlockedBy = request.ActingUserID;
        firm.UpdatedAt = DateTime.UtcNow;

        // Revoke every active refresh token for this firm's users so
        // already-logged-in sessions can't keep calling the API either.
        var tokens = await _context.RefreshTokens
            .Where(t => !t.IsRevoked && _context.Users.Any(u => u.UserID == t.UserID && u.FirmID == firm.FirmID))
            .ToListAsync(cancellationToken);
        foreach (var t in tokens) t.IsRevoked = true;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("Firm {FirmID} blocked by {ActingUserID}. Reason: {Reason}", firm.FirmID, request.ActingUserID, request.Reason);
        return true;
    }
}
