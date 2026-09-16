using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Cases.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Cases.Queries.GetCaseById;

public class GetCaseByIdHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService, ILogger<GetCaseByIdHandler> _logger) : IRequestHandler<GetCaseByIdQuery, CaseDTO?>
{
    // =====================================================
    // HANDLE — fetches a single case's full detail (firm + assignment scoped)
    // Firm-scoped lookup, then the same assignment check as GetAllCases:
    // a user without full case-directory visibility must be actively
    // assigned to THIS case, or gets 404 (not 403) so the case's
    // existence isn't disclosed.
    // =====================================================
    public async Task<CaseDTO?> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching case: {CaseID}", request.CaseID);

        // 1. Find case with all relations (firm-scoped)
        var query = _context.Cases
            .AsNoTracking()
            .Include(x => x.Court)
            .Include(x => x.Category)
            .Include(x => x.Status)
            .Include(x => x.Stage)
            .Include(x => x.Department)
            .Include(x => x.LegalOfficer)
            .Where(x => x.CaseID == request.CaseID);

            query = query.Where(x => x.FirmID == _currentUser.FirmID);

        var caseRecord = await query.FirstOrDefaultAsync(cancellationToken);

        if (caseRecord == null)
        {
            _logger.LogWarning("Case not found: {CaseID}", request.CaseID);
            throw new NotFoundException($"Case ID {request.CaseID} not found");
        }

        // 1b. SECURITY FIX (BOLA): the query above only enforces firm-level
        // (tenant) scoping. Per the Roles & Permissions Matrix, only
        // SuperAdmin / FirmAdmin / Partner may view every case in the
        // firm's directory — AssociateLawyer, Moharrir, and
        // InternParalegal must only be able to open cases they are
        // actively assigned to. Previously this endpoint had no
        // assignment check at all, so any authenticated firm user
        // could read any case in the firm just by guessing/incrementing
        // the CaseID. We return 404 (not 403) so an unassigned case's
        // existence isn't disclosed to a user who shouldn't see it.
        if (_currentUser.UserID.HasValue)
        {
            bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);

            if (!hasFullVisibility)
            {
                bool isAssigned = await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.CaseID, cancellationToken);

                if (!isAssigned)
                {
                    _logger.LogWarning(
                        "Access denied: User {UserId} is not assigned to case {CaseID}",
                        _currentUser.UserID.Value,
                        request.CaseID);
                    throw new NotFoundException($"Case ID {request.CaseID} not found");
                }
            }
        }

        // 2. Map to DTO
        // Department / LegalOfficer are nullable FKs (per schema),
        // accessed with null-conditional to avoid NullReferenceException.
        var caseDto = new CaseDTO
        {
            CaseID = caseRecord.CaseID,
            InternalReferenceNo = caseRecord.InternalReferenceNo,
            CaseNumber = caseRecord.CaseNumber,
            CaseTitle = caseRecord.CaseTitle,
            CaseDescription = caseRecord.CaseDescription,
            CourtID = caseRecord.CourtID,
            CourtName = caseRecord.Court.CourtName,
            CategoryID = caseRecord.CategoryID,
            CategoryName = caseRecord.Category.CategoryName,
            StatusID = caseRecord.StatusID,
            StatusName = caseRecord.Status.StatusName,
            StageID = caseRecord.StageID,
            StageName = caseRecord.Stage.StageName,
            DepartmentID = caseRecord.ResponsibleDepartmentID,
            DepartmentName = caseRecord.Department?.DepartmentName ?? "Not Assigned",
            LegalOfficerID = caseRecord.CurrentLegalOfficerID,
            LegalOfficerName = caseRecord.LegalOfficer?.FullName ?? "Not Assigned",
            Priority = caseRecord.Priority,
            SubjectMatter = caseRecord.SubjectMatter,
            FilingDate = caseRecord.FilingDate,
            InstitutionDate = caseRecord.InstitutionDate,
            RegistrationDate = caseRecord.RegistrationDate,
            ExpectedDisposalDate = caseRecord.ExpectedDisposalDate,
            ClaimedAmount = caseRecord.ClaimedAmount,
            PotentialLiability = caseRecord.PotentialLiability,
            IsArchived = caseRecord.IsArchived,
            CreatedDate = caseRecord.CreatedDate
        };

        _logger.LogInformation("Case successfully fetched: {CaseID}", request.CaseID);

        return caseDto;
    }
}
