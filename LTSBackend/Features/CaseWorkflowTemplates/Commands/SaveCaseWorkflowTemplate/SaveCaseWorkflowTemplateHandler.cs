using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Masters;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseWorkflowTemplates.Commands.SaveCaseWorkflowTemplate;

public sealed class SaveCaseWorkflowTemplateHandler(AppDbContext _context, ICurrentUserService _currentUser, IAuditService _auditService, ILogger<SaveCaseWorkflowTemplateHandler> _logger) : IRequestHandler<SaveCaseWorkflowTemplateCommand, int>
{
    // =====================================================
    // HANDLE — creates or replaces the caller's workflow template for a category
    // Scope: a FirmAdmin/Partner saves their OWN firm's template
    // (FirmID = their firm); SuperAdmin (no firm) saves the global one.
    // Every referenced master (category, department, status, stages,
    // document types) must be active and visible to the caller - the
    // global tenant query filters on each master guarantee a firm can never
    // reference another firm's data. Existing stage/document lines are
    // replaced, never duplicated. Cases that were already created are not
    // affected (they hold their own copies).
    //
    // TRANSACTIONAL: validation runs first (read-only), then the whole
    // replace (delete old lines -> insert new lines -> bump Version -> audit)
    // runs in ONE transaction wrapped in CreateExecutionStrategy
    // (EnableRetryOnFailure compatible). Any failure rolls everything back, so
    // a template is never left half-deleted or half-saved, and the
    // ChangeTracker is cleared before a retry so nothing is applied twice.
    // A global (SuperAdmin) template may only reference GLOBAL masters.
    // =====================================================
    public async Task<int> Handle(SaveCaseWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Saving workflow template for category {CategoryID}", request.CategoryID);

        var firmId = _currentUser.FirmID;
        bool globalScope = firmId == null;
        var errors = new List<string>();

        // NOTE: every lookup below is automatically scoped to global + own firm by the master HasQueryFilters.
        // A global template (SuperAdmin) must additionally reference global masters only, otherwise every firm
        // would inherit a template pointing at one firm's private master.
        var category = await _context.CaseCategories.AsNoTracking().FirstOrDefaultAsync(x => x.CategoryID == request.CategoryID && x.IsActive && (!globalScope || x.FirmID == null), cancellationToken);
        if (category == null) errors.Add("Case Category not found or inactive.");

        if (request.DefaultDepartmentID.HasValue)
        {
            bool deptOk = await _context.Departments.AsNoTracking().AnyAsync(x => x.DepartmentID == request.DefaultDepartmentID && x.IsActive && (!globalScope || x.FirmID == null), cancellationToken);
            if (!deptOk) errors.Add("Default Department not found or inactive.");
        }

        bool statusOk = await _context.CaseStatuses.AsNoTracking().AnyAsync(x => x.StatusID == request.InitialStatusID && x.IsActive && (!globalScope || x.FirmID == null), cancellationToken);
        if (!statusOk) errors.Add("Initial Status not found or inactive.");

        var stageIds = request.StageIDs.Distinct().ToList();
        int stageCount = await _context.CaseStages.AsNoTracking().CountAsync(x => stageIds.Contains(x.StageID) && x.IsActive && (!globalScope || x.FirmID == null), cancellationToken);
        if (stageCount != stageIds.Count) errors.Add("One or more stages were not found or are inactive.");

        var requiredIds = request.RequiredDocumentTypeIDs.Distinct().ToList();
        var optionalIds = request.OptionalDocumentTypeIDs.Distinct().ToList();
        var docIds = requiredIds.Concat(optionalIds).Distinct().ToList();
        int docCount = await _context.DocumentTypes.AsNoTracking().CountAsync(x => docIds.Contains(x.DocumentTypeID) && x.IsActive && (!globalScope || x.FirmID == null), cancellationToken);
        if (docCount != docIds.Count) errors.Add("One or more Document Types were not found or are inactive.");

        if (errors.Count > 0) throw new ValidationException(errors);

        var strategy = _context.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                // A retried attempt must start from a clean tracker (the failed attempt may have left tracked/deleted entries).
                _context.ChangeTracker.Clear();

                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var template = await _context.CaseWorkflowTemplates
                        .Include(t => t.Stages)
                        .Include(t => t.Documents)
                        .FirstOrDefaultAsync(t => t.CategoryID == request.CategoryID && t.FirmID == firmId, cancellationToken);

                    bool isNew = template == null;
                    if (isNew)
                    {
                        template = new CaseWorkflowTemplate
                        {
                            FirmID = firmId,
                            CategoryID = request.CategoryID,
                            Version = 1,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = _currentUser.UserID
                        };
                        _context.CaseWorkflowTemplates.Add(template);
                    }
                    else
                    {
                        // Remove the old lines and flush them FIRST (inside the transaction) so the unique
                        // (Template, Stage/Sequence/DocumentType) indexes can never collide with the new lines.
                        _context.CaseWorkflowTemplateStages.RemoveRange(template!.Stages);
                        _context.CaseWorkflowTemplateDocuments.RemoveRange(template.Documents);
                        await _context.SaveChangesAsync(cancellationToken);

                        template.Stages.Clear();
                        template.Documents.Clear();
                        template.Version += 1;
                        template.ModifiedDate = DateTime.UtcNow;
                        template.ModifiedBy = _currentUser.UserID;
                    }

                    template.DefaultDepartmentID = request.DefaultDepartmentID;
                    template.InitialStatusID = request.InitialStatusID;
                    template.IsActive = request.IsActive;

                    int seq = 1;
                    foreach (var stageId in stageIds)
                    {
                        template.Stages.Add(new CaseWorkflowTemplateStage { StageID = stageId, SequenceNo = seq++ });
                    }
                    foreach (var docId in requiredIds)
                    {
                        template.Documents.Add(new CaseWorkflowTemplateDocument { DocumentTypeID = docId, IsRequired = true });
                    }
                    foreach (var docId in optionalIds)
                    {
                        template.Documents.Add(new CaseWorkflowTemplateDocument { DocumentTypeID = docId, IsRequired = false });
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    _context.AuditLogs.Add(_auditService.Create(_currentUser.UserID,
                        $"Workflow Template {(isNew ? "Create" : "Update")}: Category {request.CategoryID}, Template {template.TemplateID}, v{template.Version}, " +
                        $"{stageIds.Count} stage(s), {requiredIds.Count} required / {optionalIds.Count} optional document(s), Active={template.IsActive}"));
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Workflow template saved: {TemplateID} v{Version}", template.TemplateID, template.Version);
                    return template.TemplateID;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }
        catch (DbUpdateException ex)
        {
            // Two admins saving the same category at once: the unique (FirmID, CategoryID) index rejects the loser.
            _logger.LogWarning(ex, "Workflow template save conflict for category {CategoryID}", request.CategoryID);
            throw new ValidationException(["The workflow template for this category was changed by someone else at the same time. Reload and try again."]);
        }
    }
}
