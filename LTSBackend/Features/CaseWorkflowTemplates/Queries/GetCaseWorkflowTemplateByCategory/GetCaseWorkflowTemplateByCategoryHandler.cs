using LTSBackend.Data;
using LTSBackend.Features.CaseWorkflowTemplates.DTOs;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseWorkflowTemplates.Queries.GetCaseWorkflowTemplateByCategory;

public sealed class GetCaseWorkflowTemplateByCategoryHandler(AppDbContext _context, ICurrentUserService _currentUser) : IRequestHandler<GetCaseWorkflowTemplateByCategoryQuery, CaseWorkflowTemplateDTO?>
{
    // =====================================================
    // HANDLE — powers the "Workflow Preview" on the New Case wizard.
    // A firm's own template for the category wins over the global one.
    // =====================================================
    public async Task<CaseWorkflowTemplateDTO?> Handle(GetCaseWorkflowTemplateByCategoryQuery request, CancellationToken cancellationToken)
    {
        var templates = await _context.CaseWorkflowTemplates.AsNoTracking()
            .Include(t => t.Category).Include(t => t.DefaultDepartment).Include(t => t.InitialStatus)
            .Include(t => t.Stages).ThenInclude(s => s.Stage)
            .Include(t => t.Documents).ThenInclude(d => d.DocumentType)
            .AsSplitQuery()
            .Where(t => t.CategoryID == request.CategoryID && t.IsActive)
            .ToListAsync(cancellationToken);

        var chosen = templates.FirstOrDefault(t => t.FirmID != null && t.FirmID == _currentUser.FirmID)
                     ?? templates.FirstOrDefault(t => t.FirmID == null);

        return chosen == null ? null : CaseWorkflowTemplateMapper.ToDto(chosen);
    }
}
