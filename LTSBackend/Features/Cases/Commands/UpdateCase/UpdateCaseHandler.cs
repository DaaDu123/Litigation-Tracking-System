using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LTSBackend.Features.Cases.Commands.UpdateCase;

public class UpdateCaseHandler(AppDbContext _context, IAuditService _auditService, ILogger<UpdateCaseHandler> _logger, IHttpContextAccessor _httpContextAccessor, ICurrentUserService _currentUser) : IRequestHandler<UpdateCaseCommand, bool>
{
    // =====================================================
    // HANDLE — edits an existing case's core details
    // Firm-scoped lookup (can't touch another firm's case). Every field
    // is a partial/optional update — only fields actually supplied on the
    // request are validated and applied (Court/Category/Stage/Legal
    // Officer existence checks, case-number-uniqueness-within-firm check,
    // etc.). Stamps ModifiedBy/ModifiedDate and writes an audit log entry.
    // =====================================================
    public async Task<bool> Handle(UpdateCaseCommand request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating case: {CaseID}", request.CaseID);

        int currentUserId = GetCurrentUserId();

        // 1. Find Case (firm-scoped - can't touch another firm's case)
        var caseQuery = _context.Cases.Where(x => x.CaseID == request.CaseID);
            caseQuery = caseQuery.Where(x => x.FirmID == _currentUser.FirmID);
        var caseToUpdate = await caseQuery.FirstOrDefaultAsync(cancellationToken);

        if (caseToUpdate == null)
        {
            _logger.LogWarning("Case not found: {CaseID}", request.CaseID);
            throw new NotFoundException($"Case ID {request.CaseID} not found");
        }

        // 2. Validate Court if it is being changed
        if (request.CourtID.HasValue && request.CourtID > 0)
        {
            bool courtExists = await _context.Courts.AsNoTracking().AnyAsync(x => x.CourtID == request.CourtID, cancellationToken);

            if (!courtExists)
            {
                _logger.LogWarning("Court not found: {CourtID}", request.CourtID);
                throw new NotFoundException($"Court ID {request.CourtID} not found");
            }

            caseToUpdate.CourtID = request.CourtID.Value;
        }

        // 3. Validate Category if it is being changed
        if (request.CategoryID.HasValue && request.CategoryID > 0)
        {
            bool categoryExists = await _context.CaseCategories.AsNoTracking().AnyAsync(x => x.CategoryID == request.CategoryID, cancellationToken);

            if (!categoryExists)
            {
                _logger.LogWarning("Category not found: {CategoryID}", request.CategoryID);
                throw new NotFoundException($"Category ID {request.CategoryID} not found");
            }

            caseToUpdate.CategoryID = request.CategoryID.Value;
        }

        // 4. Validate Stage if it is being changed
        if (request.StageID.HasValue && request.StageID > 0)
        {
            bool stageExists = await _context.CaseStages.AsNoTracking().AnyAsync(x => x.StageID == request.StageID, cancellationToken);

            if (!stageExists)
            {
                _logger.LogWarning("Stage not found: {StageID}", request.StageID);
                throw new NotFoundException($"Stage ID {request.StageID} not found");
            }

            caseToUpdate.StageID = request.StageID.Value;
        }

        // 5. Validate Legal Officer if it is being changed
        if (request.CurrentLegalOfficerID.HasValue && request.CurrentLegalOfficerID > 0)
        {
            bool officerExists = await _context.Users.AsNoTracking().AnyAsync(x => x.UserID == request.CurrentLegalOfficerID &&x.IsActive && !x.IsDeleted,cancellationToken);

            if (!officerExists)
            {
                _logger.LogWarning("Legal Officer not found: {LegalOfficerID}",request.CurrentLegalOfficerID);
                throw new NotFoundException($"Legal Officer ID {request.CurrentLegalOfficerID} not found");
            }

            caseToUpdate.CurrentLegalOfficerID = request.CurrentLegalOfficerID.Value;
        }

        // 6. Update optional fields
        if (!string.IsNullOrEmpty(request.CaseNumber))
        {
            // Check whether another case is already using the same number
            bool duplicateExists = await _context.Cases
                .AsNoTracking()
                .AnyAsync(x => x.CaseNumber == request.CaseNumber &&
                               x.CaseID != request.CaseID &&
                               x.FirmID == caseToUpdate.FirmID,cancellationToken);

            if (duplicateExists)
            {
                throw new ValidationException(new List<string>
                {
                    $"Case Number '{request.CaseNumber}' is already in use"
                });
            }

            caseToUpdate.CaseNumber = request.CaseNumber;
        }

        if (!string.IsNullOrEmpty(request.CaseTitle))
        {
            caseToUpdate.CaseTitle = request.CaseTitle;
        }

        if (!string.IsNullOrEmpty(request.CaseDescription))
        {
            caseToUpdate.CaseDescription = request.CaseDescription;
        }

        if (!string.IsNullOrEmpty(request.Priority))
        {
            caseToUpdate.Priority = request.Priority;
        }

        if (!string.IsNullOrEmpty(request.SubjectMatter))
        {
            caseToUpdate.SubjectMatter = request.SubjectMatter;
        }

        if (request.ExpectedDisposalDate.HasValue)
        {
            caseToUpdate.ExpectedDisposalDate = request.ExpectedDisposalDate;
        }

        if (request.ClaimedAmount.HasValue)
        {
            caseToUpdate.ClaimedAmount = request.ClaimedAmount.Value;
        }

        if (request.PotentialLiability.HasValue)
        {
            caseToUpdate.PotentialLiability = request.PotentialLiability.Value;
        }

        if (request.IsArchived.HasValue)
        {
            caseToUpdate.IsArchived = request.IsArchived.Value;
        }

        // 7. Update timestamps
        caseToUpdate.ModifiedBy = currentUserId;
        caseToUpdate.ModifiedDate = DateTime.UtcNow;

        // 8. Create Audit Log
        var auditLog = _auditService.Create(currentUserId, $"Case Update: {caseToUpdate.CaseNumber}");
        _context.AuditLogs.Add(auditLog);

        // 9. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case successfully updated: {CaseID}", request.CaseID);

        return true;
    }
    // SECURITY FIX: previously defaulted to UserID = 1 (which, per
    // AppDbContext.SeedUsers, IS the SuperAdmin account) whenever the
    // identity claim was missing or unparsable, instead of failing the
    // request. [Authorize] on the controller should make that case
    // unreachable in practice, but silently falling back to the
    // highest-privileged account's ID is exactly the wrong failure mode
    // for defense-in-depth: any future change that weakens the auth
    // pipeline (a misconfigured policy, a bypassed filter, a bug) would
    // silently attribute case updates/audit entries to SuperAdmin rather
    // than being rejected outright. Throw instead, matching how every
    // other authenticated handler in this codebase treats a missing
    // identity claim.
    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Case update rejected: missing or invalid user identity claim");
            throw new UnauthorizedException("Unable to determine the current user's identity.");
        }

        return userId;
    }
}