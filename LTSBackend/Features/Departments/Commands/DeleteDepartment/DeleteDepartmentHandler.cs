using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Departments.Commands.DeleteDepartment;

public sealed class DeleteDepartmentHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<DeleteDepartmentHandler> _logger) : IRequestHandler<DeleteDepartmentCommand, bool>
{





    public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting department: {DepartmentID}", request.DepartmentID);


        var department = await _context.Departments.FirstOrDefaultAsync(x => x.DepartmentID == request.DepartmentID, cancellationToken);

        if (department == null)
        {
            _logger.LogWarning("Delete failed: Department not found: {DepartmentID}", request.DepartmentID);
            throw new NotFoundException("Department not found.");
        }



        if (department.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Delete denied: user {UserId} attempted to delete a global/other-firm department {DepartmentID}", _currentUser.UserID, request.DepartmentID);
            throw new NotFoundException("Department not found.");
        }




        int caseCount = await _context.Cases.CountAsync(x => x.ResponsibleDepartmentID == request.DepartmentID, cancellationToken);

        if (caseCount > 0)
        {
            _logger.LogWarning("Delete failed: {Count} case(s) reference department: {DepartmentID}", caseCount, request.DepartmentID);

            throw new ValidationException(new()
            {
                $"Cannot delete department. {caseCount} case(s) are currently linked to it. " +
                "Reassign or archive those cases first, or deactivate the department instead."
            });
        }


        if (await _context.CaseWorkflowTemplates.AnyAsync(x => x.DefaultDepartmentID == request.DepartmentID, cancellationToken))
        {
            throw new ValidationException(new() { "Cannot delete department. It is the default department of a workflow template. Change that template first, or deactivate the department instead." });
        }


        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Department deleted successfully: {DepartmentID}", request.DepartmentID);

        return true;
    }
}
