using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LTSBackend.Data;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Services.CurrentUser;

namespace LTSBackend.Features.Hearings.Commands.DeleteHearing
{
    public class DeleteHearingCommandHandler(AppDbContext _context, ICurrentUserService _currentUser) : IRequestHandler<DeleteHearingCommand, bool>
    {
        // =====================================================
        // HANDLE — permanently removes a hearing and its attendance records
        // Firm-scoped lookup, then deletes child HearingAttendance rows
        // first (FK-safe order) before removing the hearing itself.
        // =====================================================
        public async Task<bool> Handle(DeleteHearingCommand request, CancellationToken cancellationToken)
        {
            var hearing = await _context.Hearings.Include(h => h.Case).FirstOrDefaultAsync(h => h.HearingID == request.HearingId, cancellationToken);

            if (hearing == null || (hearing.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException("Hearing not found");

            // Delete associated attendance records first
            var attendanceRecords = await _context.HearingAttendances.Where(ha => ha.HearingID == request.HearingId).ToListAsync(cancellationToken);

            foreach (var record in attendanceRecords)
            {
                _context.HearingAttendances.Remove(record);
            }

            _context.Hearings.Remove(hearing);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}