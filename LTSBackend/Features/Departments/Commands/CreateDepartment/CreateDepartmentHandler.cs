using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Masters;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Departments.Commands.CreateDepartment;

public sealed class CreateDepartmentHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<CreateDepartmentHandler> _logger) : IRequestHandler<CreateDepartmentCommand, int>
{







    public async Task<int> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating department: {DepartmentName}", request.DepartmentName);

        request = request with
        {
            DepartmentName = request.DepartmentName.Trim(),
            DepartmentCode = request.DepartmentCode?.Trim(),
            Description = request.Description?.Trim()
        };




        bool nameExists = await _context.Departments.AnyAsync(x => x.DepartmentName.ToLower() == request.DepartmentName.ToLower(), cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning("Create failed: Department already exists: {DepartmentName}", request.DepartmentName);
            throw new ValidationException(new()
            {
                $"Department '{request.DepartmentName}' already exists."
            });
        }


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
