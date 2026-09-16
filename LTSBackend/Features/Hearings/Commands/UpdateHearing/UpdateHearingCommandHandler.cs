using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LTSBackend.Data;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;

namespace LTSBackend.Features.Hearings.Commands.UpdateHearing
{
    public class UpdateHearingCommandHandler : IRequestHandler<UpdateHearingCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permissionService;

        public UpdateHearingCommandHandler(AppDbContext context, ICurrentUserService currentUser, IPermissionService permissionService)
        {
            _context = context;
            _currentUser = currentUser;
            _permissionService = permissionService;
        }

        // =====================================================
        // HANDLE — edits an existing hearing's details
        // Same visibility rule as Create: firm isolation plus either full
        // case-directory visibility or actual assignment to the case.
        // =====================================================
        public async Task<bool> Handle(UpdateHearingCommand request, CancellationToken cancellationToken)
        {
            var hearing = await _context.Hearings
                .Include(h => h.Case)
                .FirstOrDefaultAsync(h => h.HearingID == request.Hearing.HearingId, cancellationToken);

            if (hearing == null || (hearing.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException("Hearing not found");

            // SECURITY FIX (IDOR): controller allows RoleNames.AllLawyers
            // (includes AssociateLawyer/Moharrir), who must be scoped to
            // their assigned cases only.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, hearing.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException("Hearing not found");
                }
            }

            // Note: entity has "Purpose"/"Outcome" (not HearingPurpose/HearingOutcome), and NO ModifiedDate field
            hearing.HearingDate = request.Hearing.HearingDate;
            hearing.CourtRoom = request.Hearing.CourtRoom;
            hearing.JudgeName = request.Hearing.JudgeName;
            hearing.Purpose = request.Hearing.HearingPurpose;
            hearing.Outcome = request.Hearing.HearingOutcome;
            hearing.NextHearingDate = request.Hearing.NextHearingDate;
            hearing.Remarks = request.Hearing.Remarks;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}