using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Firms.Commands.UpdateFirm;

public class UpdateFirmCommandHandler(AppDbContext _context) : IRequestHandler<UpdateFirmCommand, bool>
{
    // =====================================================
    // HANDLE — edits a firm workspace's own details
    // Name, address, contact info, and custom domain only — does not
    // touch block/delete state.
    // =====================================================
    public async Task<bool> Handle(UpdateFirmCommand request, CancellationToken cancellationToken)
    {
        var firm = await _context.Firms.FirstOrDefaultAsync(x => x.FirmID == request.FirmID, cancellationToken);
        if (firm == null || firm.IsDeleted)
            throw new NotFoundException("Firm not found.");

        firm.FirmName = request.FirmName;
        firm.Address = request.Address;
        firm.ContactEmail = request.ContactEmail;
        firm.ContactPhone = request.ContactPhone;
        firm.CustomDomain = request.CustomDomain;
        firm.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
