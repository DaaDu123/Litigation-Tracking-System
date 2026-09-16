using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.CaseParties.Commands.UpdateCaseParty
{
    public class UpdateCasePartyHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,
        IPermissionService _permissionService,IHttpContextAccessor _httpContextAccessor) : IRequestHandler<UpdateCasePartyCommand, bool>
    {
        // =====================================================
        // HANDLE — edits an existing party's details
        // Same visibility rule as Create: firm isolation plus either full
        // case-directory visibility or actual assignment to the case.
        // =====================================================
        public async Task<bool> Handle(UpdateCasePartyCommand request, CancellationToken cancellationToken)
        {
            var party = await _context.CaseParties.Include(p => p.Case).FirstOrDefaultAsync(p => p.PartyID == request.Party.PartyID, cancellationToken);

            if (party == null || (party.Case.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Party ID {request.Party.PartyID} not found");

            // SECURITY FIX (IDOR): see CreateCasePartyHandler for rationale -
            // Update is open to AssociateLawyer/Moharrir who must be scoped to
            // their assigned cases only.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, party.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Party ID {request.Party.PartyID} not found");
                }
            }

            party.PartyType = request.Party.PartyType;
            party.PartyName = request.Party.PartyName;
            party.Organization = request.Party.Organization;
            party.CNIC = request.Party.CNIC;
            party.NTN = request.Party.NTN;
            party.ContactNo = request.Party.ContactNo;
            party.Email = request.Party.Email;
            party.Address = request.Party.Address;
            party.LawyerName = request.Party.LawyerName;
            party.Remarks = request.Party.Remarks;

            int currentUserId = GetCurrentUserId();
            _context.AuditLogs.Add(_auditService.Create(currentUserId, $"Case Party Updated: {party.PartyName} (PartyID {party.PartyID})"));

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