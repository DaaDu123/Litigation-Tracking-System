using LTSBackend.Data;
using LTSBackend.Features.FirmAdminRequests.DTOs;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.FirmAdminRequests.Queries.GetMyFirmAdminRequest;

public class GetMyFirmAdminRequestQueryHandler(AppDbContext _context, ICurrentUserService _currentUser)
    : IRequestHandler<GetMyFirmAdminRequestQuery, FirmAdminRequestDTO?>
{
    // =====================================================
    // HANDLE — the caller's own most recent Firm Admin request that was
    // submitted FROM their account (UserID == the caller). Legacy/
    // anonymous requests (UserID null) are intentionally excluded here -
    // those aren't tied to any logged-in identity to match against.
    // =====================================================
    public async Task<FirmAdminRequestDTO?> Handle(GetMyFirmAdminRequestQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserID is null)
            return null;

        var firmAdminRequest = await _context.FirmAdminRequests.AsNoTracking()
            .Where(x => x.UserID == _currentUser.UserID)
            .OrderByDescending(x => x.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (firmAdminRequest == null)
            return null;

        return new FirmAdminRequestDTO
        {
            RequestID = firmAdminRequest.RequestID,
            FirmName = firmAdminRequest.FirmName,
            FirmCode = firmAdminRequest.FirmCode,
            Address = firmAdminRequest.Address,
            ContactEmail = firmAdminRequest.ContactEmail,
            ContactPhone = firmAdminRequest.ContactPhone,
            AdminFullName = firmAdminRequest.AdminFullName,
            AdminEmail = firmAdminRequest.AdminEmail,
            AdminPhone = firmAdminRequest.AdminPhone,
            Status = firmAdminRequest.Status,
            RequestedAt = firmAdminRequest.RequestedAt,
            ReviewedBy = firmAdminRequest.ReviewedBy,
            ReviewedAt = firmAdminRequest.ReviewedAt,
            RejectionReason = firmAdminRequest.RejectionReason,
            CreatedFirmID = firmAdminRequest.CreatedFirmID
        };
    }
}
