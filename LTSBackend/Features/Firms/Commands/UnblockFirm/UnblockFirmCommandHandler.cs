using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Firms.Commands.UnblockFirm;

public class UnblockFirmCommandHandler(AppDbContext _context) : IRequestHandler<UnblockFirmCommand, bool>
{
    // =====================================================
    // HANDLE — reverses Block, restoring the firm's ability to log in
    // =====================================================
    public async Task<bool> Handle(UnblockFirmCommand request, CancellationToken cancellationToken)
    {
        var firm = await _context.Firms.FirstOrDefaultAsync(x => x.FirmID == request.FirmID, cancellationToken);
        if (firm == null || firm.IsDeleted)
            throw new NotFoundException("Firm not found.");

        firm.IsBlocked = false;
        firm.BlockedReason = null;
        firm.BlockedAt = null;
        firm.BlockedBy = null;
        firm.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
