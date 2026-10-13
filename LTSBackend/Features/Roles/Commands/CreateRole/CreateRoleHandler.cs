using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Roles.Commands.CreateRole;

public sealed class CreateRoleHandler(AppDbContext _context) : IRequestHandler<CreateRoleCommand, int>
{






    public async Task<int> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        request = request with
        {
            RoleName = request.RoleName.Trim(),
            PermissionIds = request.PermissionIds.Distinct().ToList()
        };

        bool exists = await _context.Roles.AnyAsync(x => x.RoleName.ToLower() == request.RoleName.ToLower(), cancellationToken);
        if (exists)
            throw new ValidationException(new()
            {
                "Role already exists."
            });

        var validPermissions = await _context.Permissions
            .Where(x => request.PermissionIds.Contains(x.PermissionID))
            .Select(x => x.PermissionID)
            .ToListAsync(cancellationToken);

        if (validPermissions.Count != request.PermissionIds.Count)
            throw new ValidationException(new()
            {
                "One or more permissions are invalid."
            });




        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var role = new Role
                {
                    RoleName = request.RoleName,
                    Description = request.Description
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync(cancellationToken);
                var rolePermissions = validPermissions.Select(permissionId => new RolePermission
                {
                    RoleID = role.RoleID,
                    PermissionID = permissionId
                });

                await _context.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return role.RoleID;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}