using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LTSBackend.Data;
using LTSBackend.Features.Hearings.DTOs;
using LTSBackend.Services.CurrentUser;

namespace LTSBackend.Features.Hearings.Queries.GetUpcomingHearings
{
    public class GetUpcomingHearingsQueryHandler : IRequestHandler<GetUpcomingHearingsQuery, PagedHearingResult<HearingDetailDTO>>
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetUpcomingHearingsQueryHandler(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        // =====================================================
        // HANDLE — paged list of future hearings across the caller's own firm
        // Firm-scoped (previously leaked every firm's upcoming hearings
        // to any logged-in user), optionally filtered by case or court.
        // Computes DaysRemaining and a Critical/High/Medium/Normal
        // HearingPriority label per hearing for the dashboard widget.
        // =====================================================
        public async Task<PagedHearingResult<HearingDetailDTO>> Handle(GetUpcomingHearingsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Hearings
                .AsNoTracking()
                .Include(h => h.Case)
                .Include(h => h.Court)
                .Where(h => h.HearingDate >= DateTime.UtcNow);

            // FIX: previously this leaked EVERY firm's upcoming hearings to any logged-in user
            query = query.Where(h => h.Case.FirmID == _currentUser.FirmID);

            if (request.CaseId.HasValue)
                query = query.Where(h => h.CaseID == request.CaseId.Value);

            if (request.CourtId.HasValue)
                query = query.Where(h => h.CourtID == request.CourtId.Value);

            query = query.OrderBy(h => h.HearingDate);

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