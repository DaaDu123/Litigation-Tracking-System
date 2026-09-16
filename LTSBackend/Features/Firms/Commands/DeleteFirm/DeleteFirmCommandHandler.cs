using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Firms.Commands.DeleteFirm;

public class DeleteFirmCommandHandler(AppDbContext _context, ILogger<DeleteFirmCommandHandler> _logger)
    : IRequestHandler<DeleteFirmCommand, bool>
{
    // =====================================================
    // HANDLE — permanently removes a firm workspace
    // Soft-deletes the firm (IsDeleted + IsBlocked), and deactivates
    // every user under it so none of them can log in. Intended to run
    // after the firm's data has already been exported.
    // =====================================================
    public async Task<bool> Handle(DeleteFirmCommand request, CancellationToken cancellationToken)
    {
        var firm = await _context.Firms.FirstOrDefaultAsync(x => x.FirmID == request.FirmID, cancellationToken);
        if (firm == null || firm.IsDeleted)
            throw new NotFoundException("Firm not found.");

        firm.IsDeleted = true;
        firm.IsBlocked = true;
        firm.DeletedAt = DateTime.UtcNow;
        firm.DeletedBy = request.ActingUserID;
        firm.UpdatedAt = DateTime.UtcNow;

        // Deactivate every user under this firm too so they can't login.
        var users = await _context.Users.Where(u => u.FirmID == firm.FirmID).ToListAsync(cancellationToken);
        foreach (var u in users)
        {
            u.IsActive = false;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("Firm {FirmID} removed by {ActingUserID}", firm.FirmID, request.ActingUserID);
        return true;
    }
}
