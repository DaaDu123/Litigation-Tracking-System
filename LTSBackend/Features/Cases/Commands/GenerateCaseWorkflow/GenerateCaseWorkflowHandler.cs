using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Cases.Commands.GenerateCaseWorkflow;

public sealed class GenerateCaseWorkflowHandler(AppDbContext _context, IAuditService _auditService, ICurrentUserService _currentUser, ILogger<GenerateCaseWorkflowHandler> _logger) : IRequestHandler<GenerateCaseWorkflowCommand, bool>
{
    // =====================================================
    // HANDLE — gives an existing case created BEFORE workflow templates a
    // workflow + document checklist, from the category's active template
    // (firm's own wins over the global one).
    //
    // Non-destructive: the case's Status, Stage, Department and every
    // document are left exactly as they are. The case's CURRENT stage is
    // honoured: template stages before it are marked Completed, it is
    // Active, the rest Pending; if the current stage isn't part of the
    // template it is placed first as the Active stage so no history is
    // lost. A closed case gets all stages Completed. Refuses cases that
    // already have a workflow. One SaveChanges = one atomic transaction;
    // a concurrent double-click is caught by the unique (CaseID, StageID)
    // index and reported cleanly.
    // =====================================================
    public async Task<bool> Handle(GenerateCaseWorkflowCommand request, CancellationToken cancellationToken)
    {
        int userId = _currentUser.UserID ?? throw new UnauthorizedException("Unable to determine the current user's identity.");

        var c = await _context.Cases.FirstOrDefaultAsync(x => x.CaseID == request.CaseID && x.FirmID == _currentUser.FirmID, cancellationToken);
        if (c == null)
            throw new NotFoundException($"Case ID {request.CaseID} not found");

        if (c.IsArchived)
            throw new ValidationException(["This case is archived. Restore it before generating a workflow."]);

        if (await _context.CaseWorkflowStages.AnyAsync(x => x.CaseID == c.CaseID, cancellationToken))
            throw new ValidationException(["This case already has a workflow."]);

        var templates = await _context.CaseWorkflowTemplates.AsNoTracking()
            .Include(t => t.Stages).ThenInclude(s => s.Stage)
            .Include(t => t.Documents).ThenInclude(d => d.DocumentType)
            .AsSplitQuery()
            .Where(t => t.CategoryID == c.CategoryID && t.IsActive)
            .ToListAsync(cancellationToken);
        var template = templates.FirstOrDefault(t => t.FirmID == _currentUser.FirmID) ?? templates.FirstOrDefault(t => t.FirmID == null);
        if (template == null)
            throw new ValidationException(["No active workflow template is configured for this case's category. Configure one in Master Data first."]);

        var templateStages = template.Stages.Where(s => s.Stage.IsActive).OrderBy(s => s.SequenceNo).Select(s => s.StageID).ToList();
        if (templateStages.Count == 0)
            throw new ValidationException(["The workflow template for this category has no active stages."]);

        // Order: [current stage first if it is not part of the template] + template order
        var order = new List<int>();
        if (!templateStages.Contains(c.StageID)) order.Add(c.StageID);
        order.AddRange(templateStages);

        int currentIdx = order.IndexOf(c.StageID);
        var now = DateTime.UtcNow;
        for (int i = 0; i < order.Count; i++)
        {
            string state = c.IsClosed || i < currentIdx ? CaseWorkflowStageState.Completed
                         : i == currentIdx ? CaseWorkflowStageState.Active
                         : CaseWorkflowStageState.Pending;
            _context.CaseWorkflowStages.Add(new CaseWorkflowStage
            {
                CaseID = c.CaseID,
                StageID = order[i],
                SequenceNo = i + 1,
                Status = state,
                StartedDate = state == CaseWorkflowStageState.Pending ? null : now,
                CompletedDate = state == CaseWorkflowStageState.Completed ? now : null
            });
        }

        var docs = template.Documents.Where(d => d.DocumentType.IsActive).ToList();
        foreach (var d in docs)
        {
            _context.CaseDocumentRequirements.Add(new CaseDocumentRequirement
            {
                CaseID = c.CaseID,
                DocumentTypeID = d.DocumentTypeID,
                IsRequired = d.IsRequired
            });
        }

        c.WorkflowTemplateID = template.TemplateID;
        c.WorkflowTemplateVersion = template.Version;
        c.ModifiedBy = userId;
        c.ModifiedDate = now;

        _context.AuditLogs.Add(_auditService.Create(userId,
            $"Case Workflow Generate: {c.CaseNumber} from template {template.TemplateID} v{template.Version} ({order.Count} stage(s), {docs.Count} checklist item(s))"));

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Workflow generation conflict for case {CaseID}", c.CaseID);
            throw new ValidationException(["The workflow was just generated by someone else. Reload the case."]);
        }

        _logger.LogInformation("Workflow generated for case {CaseID} from template {TemplateID} v{Version}", c.CaseID, template.TemplateID, template.Version);
        return true;
    }
}
