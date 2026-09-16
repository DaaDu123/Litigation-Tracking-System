using LTSBackend.Data;
using LTSBackend.Features.Firms.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Firms.Queries.GetAllFirms;

public class GetAllFirmsQueryHandler(AppDbContext _context) : IRequestHandler<GetAllFirmsQuery, List<FirmDTO>>
{
    // =====================================================
    // HANDLE — lists every non-deleted firm workspace on the platform
    // Includes each firm's live user/case counts for the SuperAdmin
    // console.
    // =====================================================
    public async Task<List<FirmDTO>> Handle(GetAllFirmsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Firms
            .AsNoTracking()
            .Where(f => !f.IsDeleted)
            .OrderBy(f => f.FirmName)
            .Select(f => new FirmDTO
            {
                FirmID = f.FirmID,
                FirmName = f.FirmName,
                FirmCode = f.FirmCode,
                Address = f.Address,
                ContactEmail = f.ContactEmail,
                ContactPhone = f.ContactPhone,
                CustomDomain = f.CustomDomain,
                IsBlocked = f.IsBlocked,
                BlockedReason = f.BlockedReason,
                BlockedAt = f.BlockedAt,
                UserCount = f.Users.Count(u => !u.IsDeleted),
                CaseCount = _context.Cases.Count(c => c.FirmID == f.FirmID),
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
