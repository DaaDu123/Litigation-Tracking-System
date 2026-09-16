using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LTSBackend.Data;
using LTSBackend.Features.Hearings.DTOs;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;

namespace LTSBackend.Features.Hearings.Queries.GetHearingById
{
    public class GetHearingByIdQueryHandler : IRequestHandler<GetHearingByIdQuery, HearingDetailDTO>
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionService _permissionService;

        public GetHearingByIdQueryHandler(AppDbContext context, ICurrentUserService currentUser, IPermissionService permissionService)
        {
            _context = context;
            _currentUser = currentUser;
            _permissionService = permissionService;
        }

        // =====================================================
        // HANDLE — fetches a single hearing's full detail
        // Firm isolation plus the same assignment check as
        // GetCaseHearingsQueryHandler. Computes DaysRemaining and a
        // Critical/High/Medium/Normal HearingPriority label for display.
        // =====================================================
        public async Task<HearingDetailDTO> Handle(GetHearingByIdQuery request, CancellationToken cancellationToken)
        {
            var hearing = await _context.Hearings
                .Include(h => h.Case)
                .Include(h => h.Court)
                .Where(h => h.HearingID == request.HearingId)
                .FirstOrDefaultAsync(cancellationToken);

            if (hearing == null || (hearing.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException("Hearing not found");

            // SECURITY FIX (IDOR): see GetCaseHearingsQueryHandler for rationale.
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

            string? createdByName = await _context.Users
                .Where(u => u.UserID == hearing.CreatedBy)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(cancellationToken);

            int daysRemaining = (int)(hearing.HearingDate - DateTime.UtcNow).TotalDays;
            string priority = daysRemaining <= 1 ? "Critical" :
                            daysRemaining <= 7 ? "High" :
                            daysRemaining <= 15 ? "Medium" : "Normal";

            return new HearingDetailDTO
            {
                HearingId = hearing.HearingID,
                CaseId = hearing.CaseID,
                CaseNumber = hearing.Case?.CaseNumber,
                CaseTitle = hearing.Case?.CaseTitle,
                CourtId = hearing.CourtID,
                CourtName = hearing.Court?.CourtName,
                HearingDate = hearing.HearingDate,
                CourtRoom = hearing.CourtRoom,
                JudgeName = hearing.JudgeName,
                HearingPurpose = hearing.Purpose,
                HearingOutcome = hearing.Outcome,
                NextHearingDate = hearing.NextHearingDate,
                Remarks = hearing.Remarks,
                CreatedByUser = createdByName,
                CreatedDate = hearing.CreatedDate,
                DaysRemaining = daysRemaining,
                HearingPriority = priority
            };
        }
    }
}