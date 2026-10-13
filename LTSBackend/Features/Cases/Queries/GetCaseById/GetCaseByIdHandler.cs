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







    public async Task<CaseDTO?> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching case: {CaseID}", request.CaseID);


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
            ArchivedDate = caseRecord.ArchivedDate,
            ArchiveReason = caseRecord.ArchiveReason,
            CreatedDate = caseRecord.CreatedDate
        };

        _logger.LogInformation("Case successfully fetched: {CaseID}", request.CaseID);

        return caseDto;
    }
}
