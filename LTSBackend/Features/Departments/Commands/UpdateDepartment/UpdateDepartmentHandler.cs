using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Departments.Commands.UpdateDepartment;

public sealed class UpdateDepartmentHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<UpdateDepartmentHandler> _logger) : IRequestHandler<UpdateDepartmentCommand, bool>
{
    // =====================================================
    // HANDLE — edits an existing department
    // Ownership check: a FirmAdmin may only edit their OWN firm's custom
    // department — never a global one or another firm's (returns
    // NotFound rather than Forbidden, so its existence isn't disclosed).
    // Also re-checks name uniqueness before saving.
    // =====================================================
    public async Task<bool> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating department: {DepartmentID}", request.DepartmentID);

        request = request with
        {
            DepartmentName = request.DepartmentName.Trim(),
            DepartmentCode = request.DepartmentCode?.Trim(),
            Description = request.Description?.Trim()
        };

        // 1. Find department
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.DepartmentID == request.DepartmentID, cancellationToken);

        if (department == null)
        {
            _logger.LogWarning("Update failed: Department not found: {DepartmentID}", request.DepartmentID);
            throw new NotFoundException("Department not found.");
        }

        // 1b. Ownership check: a FirmAdmin may edit only their OWN firm's
        // custom department - never a system-wide global department.
        if (department.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Update denied: user {UserId} attempted to edit a global/other-firm department {DepartmentID}", _currentUser.UserID, request.DepartmentID);
            throw new NotFoundException("Department not found.");
        }

        // 2. Ensure new name is unique (excluding self)
        bool nameExists = await _context.Departments.AnyAsync(x => x.DepartmentID != request.DepartmentID && x.DepartmentName.ToLower() == request.DepartmentName.ToLower(),cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning("Update failed: Department name already exists: {DepartmentName}", request.DepartmentName);
            throw new ValidationException(new()
            {
                $"Department '{request.DepartmentName}' already exists."
            });
        }

        // 3. Ensure new code is unique (excluding self)
        if (!string.IsNullOrWhiteSpace(request.DepartmentCode))
        {
            bool codeExists = await _context.Departments.AnyAsync(x => x.DepartmentID != request.DepartmentID &&
            x.DepartmentCode != null && x.DepartmentCode.ToLower() == request.DepartmentCode.ToLower(),cancellationToken);

            if (codeExists)
            {
                _logger.LogWarning("Update failed: Department code already exists: {DepartmentCode}", request.DepartmentCode);
                throw new ValidationException(new()
                {
                    $"Department code '{request.DepartmentCode}' already exists."
                });
            }
        }

        // 4. Apply changes
        department.DepartmentName = request.DepartmentName;
        department.DepartmentCode = request.DepartmentCode;
        department.Description = request.Description;
        department.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Department updated successfully: {DepartmentID}", request.DepartmentID);

        return true;
    }
}
