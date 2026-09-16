using LTSBackend.Data;
using LTSBackend.Features.Departments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Departments.Queries.GetAllDepartments;

public sealed class GetAllDepartmentsHandler(AppDbContext _context, ILogger<GetAllDepartmentsHandler> _logger) : IRequestHandler<GetAllDepartmentsQuery, List<DepartmentDTO>>
{
    // =====================================================
    // HANDLE — lists departments for dropdowns / admin screens
    // Optionally filtered by search text and by active-only. Visibility
    // (global + own firm) is enforced by the entity's EF Core
    // HasQueryFilter, not by this handler.
    // =====================================================
    public async Task<List<DepartmentDTO>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all departments (ActiveOnly={ActiveOnly})", request.ActiveOnly);
        var query = _context.Departments.AsNoTracking().AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        var departments = await query.OrderBy(x => x.DepartmentName)
            .Select(x => new DepartmentDTO
            {
                DepartmentID = x.DepartmentID,
                DepartmentName = x.DepartmentName,
                DepartmentCode = x.DepartmentCode,
                Description = x.Description,
                IsActive = x.IsActive,
                IsGlobal = x.FirmID == null
            })
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Retrieved {Count} departments", departments.Count);
        return departments;
    }
}
