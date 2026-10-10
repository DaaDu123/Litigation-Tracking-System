using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseWorkflowTemplates.Commands.DeleteCaseWorkflowTemplate;

public sealed class DeleteCaseWorkflowTemplateHandler(AppDbContext _context, ICurrentUserService _currentUser, IAuditService _auditService, ILogger<DeleteCaseWorkflowTemplateHandler> _logger) : IRequestHandler<DeleteCaseWorkflowTemplateCommand, bool>
{
    // =====================================================
    // HANDLE — deletes a workflow template the caller owns
    // Only the caller's own scope can be deleted (a firm can't delete the
    // global template, SuperAdmin-owned). Deleting a template removes only
    // its own stage/document lines (cascade); no master data is touched and
    // existing cases keep their already-generated workflow + checklist.
    // =====================================================
    public async Task<bool> Handle(DeleteCaseWorkflowTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.CaseWorkflowTemplates.FirstOrDefaultAsync(t => t.TemplateID == request.TemplateID, cancellationToken);

        if (template == null)
            throw new NotFoundException($"Workflow template {request.TemplateID} not found");

        if (template.FirmID != _currentUser.FirmID)
            throw new ValidationException(new() { "You can only delete your own firm's workflow templates." });

        // Template + its audit line commit together in the single SaveChanges (one implicit transaction).
        _context.CaseWorkflowTemplates.Remove(template);
        _context.AuditLogs.Add(_auditService.Create(_currentUser.UserID, $"Workflow Template Delete: Template {template.TemplateID}, Category {template.CategoryID}, v{template.Version}"));
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Workflow template deleted: {TemplateID}", request.TemplateID);
        return true;
    }
}
