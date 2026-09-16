using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LTSBackend.Data;
using LTSBackend.Features.Hearings.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;

namespace LTSBackend.Features.Hearings.Queries.GetCaseHearings
{
    public class GetCaseHearingsQueryHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService) : IRequestHandler<GetCaseHearingsQuery, PagedHearingResult<HearingDetailDTO>>
    {
        

        // =====================================================
        // HANDLE — paged list of a case's hearings, scoped to who's allowed to see it
        // Same assignment-or-full-visibility rule as
        // GetCaseAssignmentsHandler (empty page, not an error, if
        // denied), plus firm isolation. Newest-first, with priority
        // labels computed per hearing.
        // =====================================================
        public async Task<PagedHearingResult<HearingDetailDTO>> Handle(GetCaseHearingsQuery request, CancellationToken cancellationToken)
        {
            // SECURITY FIX (IDOR): firm scoping alone let any firm user - including
            // AssociateLawyer/Moharrir/InternParalegal - read hearings for a case
            // they aren't assigned to. Mirrors GetCaseAssignmentsHandler.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.CaseId, cancellationToken);
                    if (!isAssignedToCase)
                    {
                        return new PagedHearingResult<HearingDetailDTO> { Items = new List<HearingDetailDTO>(), TotalCount = 0, PageNumber = request.PageNumber, PageSize = request.PageSize };
                    }
                }
            }

            var query = _context.Hearings
                .AsNoTracking()
                .Include(h => h.Case)
                .Include(h => h.Court)
                .Where(h => h.CaseID == request.CaseId);

            // Multi-tenant isolation
            query = query.Where(h => h.Case.FirmID == _currentUser.FirmID);

            query = query.OrderByDescending(h => h.HearingDate);

            int totalCount = await query.CountAsync(cancellationToken);

            var hearings = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var creatorIds = hearings.Select(h => h.CreatedBy).Distinct().ToList();
            var creatorNames = await _context.Users
                .AsNoTracking()
                .Where(u => creatorIds.Contains(u.UserID))
                .ToDictionaryAsync(u => u.UserID, u => u.FullName, cancellationToken);

            var hearingDTOs = HearingMappingHelper.MapToDetailDtos(hearings, creatorNames);

            return new PagedHearingResult<HearingDetailDTO>
            {
                Items = hearingDTOs,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}