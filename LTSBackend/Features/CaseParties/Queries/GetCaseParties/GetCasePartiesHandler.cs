using LTSBackend.Data;
using LTSBackend.Features.CaseParties.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseParties.Queries.GetCaseParties
{
    public class GetCasePartiesHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService) : IRequestHandler<GetCasePartiesQuery, List<CasePartyDetailDTO>>
    {
        // =====================================================
        // HANDLE — lists a case's parties, scoped to who's allowed to see it
        // Only SuperAdmin/FirmAdmin/Partner (full case-directory
        // visibility) or a user actually assigned to this case can see
        // its parties — returns an empty list (not an error) otherwise, so
        // the case's existence isn't disclosed. Also enforces firm
        // isolation.
        // =====================================================
        public async Task<List<CasePartyDetailDTO>> Handle(GetCasePartiesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);

                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                    {
                        return new List<CasePartyDetailDTO>();
                    }
                }
            }

            var query = _context.CaseParties.AsNoTracking().Where(p => p.CaseID == request.CaseID);

            // Multi-tenant isolation
                query = query.Where(p => p.Case.FirmID == _currentUser.FirmID);

            return await query
                .Select(p => new CasePartyDetailDTO
                {
                    PartyID = p.PartyID,
                    CaseID = p.CaseID,
                    PartyType = p.PartyType,
                    PartyName = p.PartyName,
                    Organization = p.Organization,
                    CNIC = p.CNIC,
                    NTN = p.NTN,
                    ContactNo = p.ContactNo,
                    Email = p.Email,
                    Address = p.Address,
                    LawyerName = p.LawyerName,
                    Remarks = p.Remarks
                })
                .ToListAsync(cancellationToken);
        }
    }
}