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








    public async Task<bool> Handle(UpdateCaseCommand request,CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating case: {CaseID}", request.CaseID);

        int currentUserId = GetCurrentUserId();


        var caseQuery = _context.Cases.Where(x => x.CaseID == request.CaseID);
            caseQuery = caseQuery.Where(x => x.FirmID == _currentUser.FirmID);
        var caseToUpdate = await caseQuery.FirstOrDefaultAsync(cancellationToken);

        if (caseToUpdate == null)
        {
            _logger.LogWarning("Case not found: {CaseID}", request.CaseID);
            throw new NotFoundException($"Case ID {request.CaseID} not found");
        }


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


        if (request.CategoryID.HasValue && request.CategoryID > 0)
        {
            bool categoryExists = await _context.CaseCategories.AsNoTracking().AnyAsync(x => x.CategoryID == request.CategoryID, cancellationToken);

            if (!categoryExists)
            {
                _logger.LogWarning("Category not found: {CategoryID}", request.CategoryID);
                throw new NotFoundException($"Category ID {request.CategoryID} not found");
            }




            if (request.CategoryID.Value != caseToUpdate.CategoryID
                && await _context.CaseWorkflowStages.AsNoTracking().AnyAsync(x => x.CaseID == request.CaseID, cancellationToken))
            {
                throw new ValidationException(new List<string>
                {
                    "The Case Category cannot be changed after the case workflow has been generated."
                });
            }

            caseToUpdate.CategoryID = request.CategoryID.Value;
        }


        if (request.StageID.HasValue && request.StageID > 0)
        {
            bool stageExists = await _context.CaseStages.AsNoTracking().AnyAsync(x => x.StageID == request.StageID, cancellationToken);

            if (!stageExists)
            {
                _logger.LogWarning("Stage not found: {StageID}", request.StageID);
                throw new NotFoundException($"Stage ID {request.StageID} not found");
            }

            if (request.StageID.Value != caseToUpdate.StageID)
            {

                var workflowRows = await _context.CaseWorkflowStages.Where(x => x.CaseID == request.CaseID).ToListAsync(cancellationToken);

                if (workflowRows.Count > 0)
                {
                    var target = workflowRows.FirstOrDefault(x => x.StageID == request.StageID.Value);
                    if (target == null)
                    {
                        throw new ValidationException(new List<string>
                        {
                            "The selected stage is not part of this case's workflow."
                        });
                    }

                    var stageNow = DateTime.UtcNow;
                    foreach (var row in workflowRows)
                    {
                        if (row.SequenceNo < target.SequenceNo)
                        {
                            row.Status = CaseWorkflowStageState.Completed;
                            row.StartedDate ??= stageNow;
                            row.CompletedDate ??= stageNow;
                        }
                        else if (row.SequenceNo == target.SequenceNo)
                        {
                            row.Status = CaseWorkflowStageState.Active;
                            row.StartedDate ??= stageNow;
                            row.CompletedDate = null;
                        }
                        else
                        {
                            row.Status = CaseWorkflowStageState.Pending;
                            row.StartedDate = null;
                            row.CompletedDate = null;
                        }
                    }
                }
            }

            caseToUpdate.StageID = request.StageID.Value;
        }


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


        if (request.ResponsibleDepartmentID.HasValue && request.ResponsibleDepartmentID > 0)
        {
            bool departmentOk = await _context.Departments.AsNoTracking().AnyAsync(x => x.DepartmentID == request.ResponsibleDepartmentID && x.IsActive, cancellationToken);

            if (!departmentOk)
            {
                throw new NotFoundException($"Department ID {request.ResponsibleDepartmentID} not found or is inactive");
            }

            caseToUpdate.ResponsibleDepartmentID = request.ResponsibleDepartmentID.Value;
        }


        if (!string.IsNullOrEmpty(request.CaseNumber))
        {

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

            if (request.IsArchived.Value != caseToUpdate.IsArchived)
            {
                caseToUpdate.IsArchived = request.IsArchived.Value;
                caseToUpdate.ArchivedDate = request.IsArchived.Value ? DateTime.UtcNow : null;
                caseToUpdate.ArchivedBy = request.IsArchived.Value ? currentUserId : null;
                if (!request.IsArchived.Value) caseToUpdate.ArchiveReason = null;
            }
        }


        caseToUpdate.ModifiedBy = currentUserId;
        caseToUpdate.ModifiedDate = DateTime.UtcNow;


        var auditLog = _auditService.Create(currentUserId, $"Case Update: {caseToUpdate.CaseNumber}");
        _context.AuditLogs.Add(auditLog);


        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case successfully updated: {CaseID}", request.CaseID);

        return true;
    }












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