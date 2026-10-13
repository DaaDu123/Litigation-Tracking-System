using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Departments.Commands.UpdateDepartment;

public sealed class UpdateDepartmentHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<UpdateDepartmentHandler> _logger) : IRequestHandler<UpdateDepartmentCommand, bool>
{







    public async Task<bool> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating department: {DepartmentID}", request.DepartmentID);

        request = request with
        {
            DepartmentName = request.DepartmentName.Trim(),
            DepartmentCode = request.DepartmentCode?.Trim(),
            Description = request.Description?.Trim()
        };


        var department = await _context.Departments.FirstOrDefaultAsync(x => x.DepartmentID == request.DepartmentID, cancellationToken);

        if (department == null)
        {
            _logger.LogWarning("Update failed: Department not found: {DepartmentID}", request.DepartmentID);
            throw new NotFoundException("Department not found.");
        }



        if (department.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Update denied: user {UserId} attempted to edit a global/other-firm department {DepartmentID}", _currentUser.UserID, request.DepartmentID);
            throw new NotFoundException("Department not found.");
        }


        bool nameExists = await _context.Departments.AnyAsync(x => x.DepartmentID != request.DepartmentID && x.DepartmentName.ToLower() == request.DepartmentName.ToLower(),cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning("Update failed: Department name already exists: {DepartmentName}", request.DepartmentName);
            throw new ValidationException(new()
            {
                $"Department '{request.DepartmentName}' already exists."
            });
        }


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


        department.DepartmentName = request.DepartmentName;
        department.DepartmentCode = request.DepartmentCode;
        department.Description = request.Description;
        department.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Department updated successfully: {DepartmentID}", request.DepartmentID);

        return true;
    }
}
