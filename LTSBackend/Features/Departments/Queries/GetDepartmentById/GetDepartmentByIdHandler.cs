using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Departments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Departments.Queries.GetDepartmentById;

public sealed class GetDepartmentByIdHandler(AppDbContext _context, ILogger<GetDepartmentByIdHandler> _logger) : IRequestHandler<GetDepartmentByIdQuery, DepartmentDTO>
{
    // =====================================================
    // HANDLE — fetches a single department by ID
    // =====================================================
    public async Task<DepartmentDTO> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching department: {DepartmentID}", request.DepartmentID);
        var department = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.DepartmentID == request.DepartmentID, cancellationToken);

        if (department == null)
        {
            _logger.LogWarning("Department not found: {DepartmentID}", request.DepartmentID);
            throw new NotFoundException("Department not found.");
        }

        return new DepartmentDTO
        {
            DepartmentID = department.DepartmentID,
            DepartmentName = department.DepartmentName,
            DepartmentCode = department.DepartmentCode,
            Description = department.Description,
            IsActive = department.IsActive,
            IsGlobal = department.FirmID == null
        };
    }
}
