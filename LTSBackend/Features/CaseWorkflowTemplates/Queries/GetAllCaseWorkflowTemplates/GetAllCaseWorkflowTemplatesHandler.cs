using LTSBackend.Data;
using LTSBackend.Features.CaseWorkflowTemplates.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseWorkflowTemplates.Queries.GetAllCaseWorkflowTemplates;

public sealed class GetAllCaseWorkflowTemplatesHandler(AppDbContext _context) : IRequestHandler<GetAllCaseWorkflowTemplatesQuery, List<CaseWorkflowTemplateDTO>>
{
    // =====================================================
    // HANDLE — lists the global + own-firm templates the caller can see
    // (tenant scoping comes from the HasQueryFilter on the template).
    // =====================================================
    public async Task<List<CaseWorkflowTemplateDTO>> Handle(GetAllCaseWorkflowTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CaseWorkflowTemplates.AsNoTracking()
            .Include(t => t.Category).Include(t => t.DefaultDepartment).Include(t => t.InitialStatus)
            .Include(t => t.Stages).ThenInclude(s => s.Stage)
            .Include(t => t.Documents).ThenInclude(d => d.DocumentType)
            .AsSplitQuery();

        if (request.ActiveOnly) query = query.Where(t => t.IsActive);

        var list = await query.OrderBy(t => t.Category.CategoryName).ToListAsync(cancellationToken);
        return list.Select(CaseWorkflowTemplateMapper.ToDto).ToList();
    }
}
