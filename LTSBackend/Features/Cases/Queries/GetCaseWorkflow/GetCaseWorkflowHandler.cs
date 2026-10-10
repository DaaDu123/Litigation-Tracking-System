using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Cases.DTOs;
using LTSBackend.Services.CurrentUser;
using LTSBackend.Services.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Cases.Queries.GetCaseWorkflow;

public class GetCaseWorkflowHandler(AppDbContext _context, ICurrentUserService _currentUser, IPermissionService _permissionService, ILogger<GetCaseWorkflowHandler> _logger) : IRequestHandler<GetCaseWorkflowQuery, CaseWorkflowDTO>
{
    // =====================================================
    // HANDLE — a case's own workflow progress + document checklist
    // Same visibility rule as GetCaseById (firm scoped; users without
    // full directory visibility must be assigned, otherwise 404 so the
    // case's existence isn't disclosed). The checklist's "fulfilled" state
    // is derived from the real Documents table (same CaseID + DocumentTypeID),
    // never stored, and only counts are exposed - not file details - so no
    // per-document permission is bypassed.
    // =====================================================
    public async Task<CaseWorkflowDTO> Handle(GetCaseWorkflowQuery request, CancellationToken cancellationToken)
    {
        var caseInfo = await _context.Cases.AsNoTracking()
            .Where(x => x.CaseID == request.CaseID && x.FirmID == _currentUser.FirmID)
            .Select(x => new { x.CategoryID, x.WorkflowTemplateID, x.WorkflowTemplateVersion })
            .FirstOrDefaultAsync(cancellationToken);
        if (caseInfo == null)
            throw new NotFoundException($"Case ID {request.CaseID} not found");

        if (_currentUser.UserID.HasValue)
        {
            bool hasFullVisibility = await _permissionService.HasFullCaseDirectoryVisibilityAsync(_currentUser.UserID.Value, cancellationToken);
            if (!hasFullVisibility && !await _permissionService.IsUserAssignedToCaseAsync(_currentUser.UserID.Value, request.CaseID, cancellationToken))
            {
                _logger.LogWarning("Workflow access denied: User {UserId} is not assigned to case {CaseID}", _currentUser.UserID.Value, request.CaseID);
                throw new NotFoundException($"Case ID {request.CaseID} not found");
            }
        }

        var stages = await _context.CaseWorkflowStages.AsNoTracking()
            .Where(x => x.CaseID == request.CaseID)
            .OrderBy(x => x.SequenceNo)
            .Select(x => new CaseWorkflowStageDTO
            {
                StageID = x.StageID,
                StageName = x.Stage.StageName,
                SequenceNo = x.SequenceNo,
                Status = x.Status,
                StartedDate = x.StartedDate,
                CompletedDate = x.CompletedDate,
                Notes = x.Notes
            }).ToListAsync(cancellationToken);

        var requirements = await _context.CaseDocumentRequirements.AsNoTracking()
            .Where(x => x.CaseID == request.CaseID)
            .OrderByDescending(x => x.IsRequired).ThenBy(x => x.DocumentType.TypeName)
            .Select(x => new CaseDocumentRequirementDTO
            {
                DocumentTypeID = x.DocumentTypeID,
                TypeName = x.DocumentType.TypeName,
                IsRequired = x.IsRequired
            }).ToListAsync(cancellationToken);

        // Latest-version documents per type on this case
        var uploaded = await _context.Documents.AsNoTracking()
            .Where(d => d.CaseID == request.CaseID && d.IsLatest)
            .GroupBy(d => d.DocumentTypeID)
            .Select(g => new { DocumentTypeID = g.Key, Count = g.Count(), Approved = g.Count(d => !d.IsDraft) })
            .ToListAsync(cancellationToken);

        foreach (var r in requirements)
        {
            var u = uploaded.FirstOrDefault(x => x.DocumentTypeID == r.DocumentTypeID);
            r.UploadedCount = u?.Count ?? 0;
            r.IsFulfilled = (u?.Approved ?? 0) > 0;
        }

        // Active template currently in force for the case's category (firm's own wins over global)
        var activeTemplates = await _context.CaseWorkflowTemplates.AsNoTracking()
            .Where(t => t.CategoryID == caseInfo.CategoryID && t.IsActive)
            .Select(t => new { t.FirmID, t.Version })
            .ToListAsync(cancellationToken);
        var current = activeTemplates.FirstOrDefault(t => t.FirmID == _currentUser.FirmID) ?? activeTemplates.FirstOrDefault(t => t.FirmID == null);

        bool legacy = stages.Count == 0;
        return new CaseWorkflowDTO
        {
            CaseID = request.CaseID,
            IsLegacy = legacy,
            WorkflowTemplateID = caseInfo.WorkflowTemplateID,
            WorkflowTemplateVersion = caseInfo.WorkflowTemplateVersion,
            CurrentTemplateVersion = current?.Version,
            TemplateChangedSinceCreation = caseInfo.WorkflowTemplateVersion.HasValue && current != null && current.Version != caseInfo.WorkflowTemplateVersion.Value,
            CanGenerate = legacy && current != null,
            Stages = stages,
            Documents = requirements
        };
    }
}
