using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.CaseParties.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseParties.Queries.GetCasePartyById
{
    public class GetCasePartyByIdHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService) : IRequestHandler<GetCasePartyByIdQuery, CasePartyDetailDTO>
    {
        // =====================================================
        // HANDLE — fetches a single party's details by ID
        // Firm isolation plus the same assignment check as
        // GetCasePartiesHandler; returns 404 (not 403) so a user who
        // shouldn't see the party isn't told it exists.
        // =====================================================
        public async Task<CasePartyDetailDTO> Handle(GetCasePartyByIdQuery request, CancellationToken cancellationToken)
        {
            var party = await _context.CaseParties.AsNoTracking().Include(p => p.Case).FirstOrDefaultAsync(p => p.PartyID == request.PartyID, cancellationToken);

            if (party == null || (party.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Party ID {request.PartyID} not found");

            // SECURITY FIX (IDOR): see GetCasePartiesHandler for full rationale -
            // firm scoping alone let any firm user read any case's parties
            // regardless of assignment. 404 (not 403) to avoid disclosing the
            // party's existence to a user who shouldn't see it.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, party.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Party ID {request.PartyID} not found");
                }
            }

            return new CasePartyDetailDTO
            {
                PartyID = party.PartyID,
                CaseID = party.CaseID,
                PartyType = party.PartyType,
                PartyName = party.PartyName,
                Organization = party.Organization,
                CNIC = party.CNIC,
                NTN = party.NTN,
                ContactNo = party.ContactNo,
                Email = party.Email,
                Address = party.Address,
                LawyerName = party.LawyerName,
                Remarks = party.Remarks
            };
        }
    }
}