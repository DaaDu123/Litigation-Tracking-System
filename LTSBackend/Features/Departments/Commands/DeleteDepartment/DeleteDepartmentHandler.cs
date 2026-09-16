using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Departments.Commands.DeleteDepartment;

public sealed class DeleteDepartmentHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<DeleteDepartmentHandler> _logger) : IRequestHandler<DeleteDepartmentCommand, bool>
{
    // =====================================================
    // HANDLE — removes a department the firm no longer needs
    // Same ownership check as Update (own firm's custom department only),
    // plus an in-use check: cannot delete while cases still reference it.
    // =====================================================
    public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting department: {DepartmentID}", request.DepartmentID);

        // 1. Find department
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.DepartmentID == request.DepartmentID, cancellationToken);

        if (department == null)
        {
            _logger.LogWarning("Delete failed: Department not found: {DepartmentID}", request.DepartmentID);
            throw new NotFoundException("Department not found.");
        }

        // 1b. Ownership check: a FirmAdmin may delete only their OWN firm's
        // custom department - never a system-wide global department.
        if (department.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Delete denied: user {UserId} attempted to delete a global/other-firm department {DepartmentID}", _currentUser.UserID, request.DepartmentID);
            throw new NotFoundException("Department not found.");
        }

        // 2. Block delete if cases reference this department
        // (Cases.ResponsibleDepartmentID has an FK with DeleteBehavior.NoAction,
        // so an unchecked delete would otherwise fail with a raw SQL FK error)
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

        // 3. Delete department
        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Department deleted successfully: {DepartmentID}", request.DepartmentID);

        return true;
    }
}
