using LTSBackend.Data;
using LTSBackend.Features.Firms.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Firms.Queries.GetFirmById;

public class GetFirmByIdQueryHandler(AppDbContext _context) : IRequestHandler<GetFirmByIdQuery, FirmDTO?>
{
    // =====================================================
    // HANDLE — fetches a single firm workspace's details by ID
    // =====================================================
    public async Task<FirmDTO?> Handle(GetFirmByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Firms
            .AsNoTracking()
            .Where(f => f.FirmID == request.FirmID && !f.IsDeleted)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}
