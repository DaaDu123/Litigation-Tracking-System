using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Masters;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Departments.Commands.CreateDepartment;

public sealed class CreateDepartmentHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<CreateDepartmentHandler> _logger) : IRequestHandler<CreateDepartmentCommand, int>
{
    // =====================================================
    // HANDLE — creates a new department
    // Trims input, checks the name is unique among what the caller can
    // see (global departments + their own firm's), then saves it. FirmID is
    // set to the caller's own firm for a FirmAdmin, or left null (global,
    // visible to every firm) for a SuperAdmin.
    // =====================================================
    public async Task<int> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating department: {DepartmentName}", request.DepartmentName);

        request = request with
        {
            DepartmentName = request.DepartmentName.Trim(),
            DepartmentCode = request.DepartmentCode?.Trim(),
            Description = request.Description?.Trim()
        };

        // 1. Ensure department name is unique
        // NOTE: automatically scoped to global + own firm's departments
        // via the HasQueryFilter on Department in AppDbContext.
        bool nameExists = await _context.Departments.AnyAsync(x => x.DepartmentName.ToLower() == request.DepartmentName.ToLower(), cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning("Create failed: Department already exists: {DepartmentName}", request.DepartmentName);
            throw new ValidationException(new()
            {
                $"Department '{request.DepartmentName}' already exists."
            });
        }

        // 2. Ensure department code is unique (if provided)
        if (!string.IsNullOrWhiteSpace(request.DepartmentCode))
        {
            bool codeExists = await _context.Departments.AnyAsync(x => x.DepartmentCode != null &&x.DepartmentCode.ToLower() == request.DepartmentCode.ToLower(),cancellationToken);

            if (codeExists)
            {
                _logger.LogWarning("Create failed: Department code already exists: {DepartmentCode}", request.DepartmentCode);
                throw new ValidationException(new()
                {
                    $"Department code '{request.DepartmentCode}' already exists."
                });
            }
        }

        // 3. Create department
        // SuperAdmin creates a system-wide global department (FirmID
        // null). FirmAdmin creates one scoped to their own firm only.
        var department = new Department
        {
            FirmID = _currentUser.FirmID,
            DepartmentName = request.DepartmentName,
            DepartmentCode = request.DepartmentCode,
            Description = request.Description,
            IsActive = request.IsActive
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Department created successfully: {DepartmentID}", department.DepartmentID);

        return department.DepartmentID;
    }
}
