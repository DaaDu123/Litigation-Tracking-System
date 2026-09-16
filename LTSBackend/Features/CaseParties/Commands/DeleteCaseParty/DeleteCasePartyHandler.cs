using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.CaseParties.Commands.DeleteCaseParty
{
    public class DeleteCasePartyHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<DeleteCasePartyCommand, bool>
    {
        // =====================================================
        // HANDLE — removes a party from a case
        // Enforces firm isolation (party's case must belong to the
        // caller's own firm) before deleting; writes an audit log entry.
        // =====================================================
        public async Task<bool> Handle(DeleteCasePartyCommand request, CancellationToken cancellationToken)
        {
            var party = await _context.CaseParties.Include(p => p.Case).FirstOrDefaultAsync(p => p.PartyID == request.PartyID, cancellationToken);

            if (party == null || (party.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Party ID {request.PartyID} not found");

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Case Party Deleted: {party.PartyName} (PartyID {party.PartyID})"));

            _context.CaseParties.Remove(party);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}