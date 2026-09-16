using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.CaseParties.Commands.CreateCaseParty
{
    public class CreateCasePartyHandler(AppDbContext _context,IAuditService _auditService,ICurrentUserService _currentUser,IPermissionService _permissionService,
        IHttpContextAccessor _httpContextAccessor,ILogger<CreateCasePartyHandler> _logger) : IRequestHandler<CreateCasePartyCommand, long>
    {
        // =====================================================
        // HANDLE — adds a new party (plaintiff/defendant/etc.) to a case
        // Verifies the case belongs to the caller's own firm, and — since
        // Create is open to AssociateLawyer/Moharrir at the controller —
        // that the caller has full case-directory visibility or is
        // actually assigned to this specific case.
        // =====================================================
        public async Task<long> Handle(CreateCasePartyCommand request, CancellationToken cancellationToken)
        {
            var caseEntity = await _context.Cases.FirstOrDefaultAsync(c => c.CaseID == request.Party.CaseID, cancellationToken);
            if (caseEntity == null || (caseEntity.FirmID != _currentUser.FirmID))
                throw new NotFoundException($"Case ID {request.Party.CaseID} not found");

            // SECURITY FIX (IDOR): Create is open to AssociateLawyer/Moharrir at
            // the controller (RoleNames.AllLawyers), who per the roles spec
            // must not touch a case they aren't assigned to. Firm scoping alone
            // isn't enough - mirror the assignment check used for reads.
            if (_currentUser.UserID.HasValue)
            {
                bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
                if (!hasFullVisibility)
                {
                    bool isAssignedToCase = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.Party.CaseID, cancellationToken);
                    if (!isAssignedToCase)
                        throw new NotFoundException($"Case ID {request.Party.CaseID} not found");
                }
            }

            int currentUserId = GetCurrentUserId();

            var party = new CaseParty
            {
                CaseID = request.Party.CaseID,
                PartyType = request.Party.PartyType,
                PartyName = request.Party.PartyName,
                Organization = request.Party.Organization,
                CNIC = request.Party.CNIC,
                NTN = request.Party.NTN,
                ContactNo = request.Party.ContactNo,
                Email = request.Party.Email,
                Address = request.Party.Address,
                LawyerName = request.Party.LawyerName,
                Remarks = request.Party.Remarks
            };

            _context.CaseParties.Add(party);

            _context.AuditLogs.Add(_auditService.Create(currentUserId,
                $"Case Party Created: {party.PartyName} ({party.PartyType}) for Case {request.Party.CaseID}"));

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Case party created: {PartyID} for Case {CaseID}", party.PartyID, request.Party.CaseID);

            return party.PartyID;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}