using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Comman.Middleware;
using LTSBackend.Data;
using LTSBackend.Models.Cases;
using LTSBackend.Models.Security;
using LTSBackend.Services;
using LTSBackend.Services.ProfileService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler(AppDbContext _context, IPasswordService _passwordService, IFileService _fileService, ILogger<CreateUserCommandHandler> _logger) : IRequestHandler<CreateUserCommand, int>
{















    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create user request for email: {Email}", request.Email);




        var fullName = request.FullName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? request.Phone : request.Phone.Trim();
        var department = string.IsNullOrWhiteSpace(request.Department) ? request.Department : request.Department.Trim();






        var actingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == request.ActingUserID, cancellationToken);
        if (actingUser == null)
        {
            _logger.LogWarning("User creation failed: acting user not found: {ActingUserId}", request.ActingUserID);
            throw new ValidationException(["Could not identify the requesting user."]);
        }






        if (actingUser.FirmID == null)
        {
            _logger.LogWarning("SuperAdmin {ActingUserId} attempted to create a user via /api/users instead of /api/firms", request.ActingUserID);
            throw new ValidationException(["SuperAdmin cannot create a firm user via this endpoint - create the firm first via POST /api/firms."]);
        }



        var existingUser = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        bool isReuse = false;

        if (existingUser != null)
        {
            if (!existingUser.IsDeleted)
            {


                _logger.LogWarning("User creation failed: Email already exists and is active: {Email}", email);
                throw new ValidationException([$"Email '{email}' already exists and is currently assigned to another user."]);
            }





            bool sameFirm = existingUser.FirmID == actingUser.FirmID;

            if (!sameFirm && !existingUser.IsReleasedForReuse)
            {
                _logger.LogWarning("User creation failed: Email {Email} is a deleted record owned by firm {OwnerFirmId}, not released, requested by firm {RequestingFirmId}",email, existingUser.FirmID, actingUser.FirmID);
                throw new ValidationException([$"Email '{email}' already exists and is reserved by another firm's deleted user record. " + "It must be released for reassignment before it can be reused here."]);
            }



            isReuse = true;
        }





        int roleId = request.RoleID is > 0 ? request.RoleID.Value : (int)UserRole.InternParalegal;

        if (!System.Enum.IsDefined(typeof(UserRole), roleId))
        {
            _logger.LogWarning("User creation failed: Invalid RoleID: {RoleID}", roleId);
            throw new ValidationException([$"Invalid role. Role ID {roleId} does not exist"]);
        }

        bool roleExists = await _context.Roles.AsNoTracking().AnyAsync(x => x.RoleID == roleId, cancellationToken);

        if (!roleExists)
        {
            _logger.LogWarning("User creation failed: Role not found: {RoleID}", roleId);
            throw new NotFoundException($"Role ID {roleId} not found");
        }



        var actingRole = actingUser.GetRole();
        if (actingRole == null || !RoleHierarchy.CanAssignRole(actingRole.Value, roleId))
        {
            _logger.LogWarning("User {ActingUserId} with role {ActingRole} attempted to assign disallowed role {TargetRoleId}", request.ActingUserID, actingRole, roleId);
            throw new ValidationException(["You are not authorized to assign this role."]);
        }


        var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.RoleID == roleId, cancellationToken);

        _logger.LogInformation("Role fetched: {RoleName}", role?.RoleName);


        if (!string.IsNullOrEmpty(department))
        {
            bool deptExists = await _context.Departments.AsNoTracking().AnyAsync(x => x.DepartmentName == department, cancellationToken);

            if (!deptExists)
            {
                _logger.LogInformation("Department not found, user will be created without a department");
            }
        }


        string passwordHash = _passwordService.HashPassword(request.Password);


        string? profileImagePath = null;
        if (request.ProfileImage is { Length: > 0 })
        {
            try
            {
                profileImagePath = await _fileService.SaveFileAsync(request.ProfileImage, "profile_pictures");
                _logger.LogInformation("Profile image uploaded for: {Email}", email);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload profile image for: {Email}", email);
            }
        }



        int resultUserId;
        User? newUser = null;

        if (isReuse)
        {








            var userToRestore = await _context.Users.IgnoreQueryFilters().FirstAsync(x => x.Email == email, cancellationToken);

            _logger.LogInformation("Reusing soft-deleted user record {UserID} (previously firm {PreviousFirmId}) for email {Email}, new firm {NewFirmId}",userToRestore.UserID, userToRestore.FirmID, email, actingUser.FirmID);

            userToRestore.FullName = fullName;
            userToRestore.PasswordHash = passwordHash;
            userToRestore.Phone = phone;
            userToRestore.Department = department;
            userToRestore.Designation = null;
            userToRestore.ProfileImage = profileImagePath;
            userToRestore.RoleID = roleId;
            userToRestore.FirmID = actingUser.FirmID;
            userToRestore.IsExternal = false;
            userToRestore.IsActive = true;
            userToRestore.IsDeleted = false;
            userToRestore.IsReleasedForReuse = false;
            userToRestore.FailedLoginAttempts = 0;
            userToRestore.LastLogin = null;
            userToRestore.PasswordChangedDate = DateTime.UtcNow;
            userToRestore.SecurityStamp = Guid.NewGuid().ToString("N");
            userToRestore.UpdatedAt = DateTime.UtcNow;



            resultUserId = userToRestore.UserID;
        }
        else
        {

            string employeeNo = GenerateEmployeeNo();

            newUser = new User
            {
                EmployeeNo = employeeNo,
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash,
                Phone = phone,
                Department = department,
                Designation = null,
                ProfileImage = profileImagePath,
                RoleID = roleId,
                FirmID = actingUser.FirmID,
                IsExternal = false,
                IsActive = true,
                IsDeleted = false,
                IsReleasedForReuse = false,
                LastLogin = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _context.Users.Add(newUser);
            resultUserId = 0;
        }







        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_Email", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning(ex, "Concurrent create/reuse race detected for email: {Email}", email);
            throw new ValidationException([$"Email '{email}' was just claimed by another request. Please try again."]);
        }

        if (!isReuse && newUser != null)
        {


            resultUserId = newUser.UserID;
        }






        _context.Notifications.Add(new Notification
        {
            NotificationTypeID = 8,
            UserID = resultUserId,
            Subject = "Complete Your Profile",
            Message = "Your account has been created. Please sign in and complete your profile " + "(name, phone, department, and other details) to get started.",
            Priority = "Medium",
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Action} successfully with ID: {UserID} and Role: {RoleName}", isReuse ? "restored/reused" : "created", resultUserId, role?.RoleName);



        return resultUserId;
    }


    private static string GenerateEmployeeNo()
    {
        var now = DateTime.UtcNow;
        var randomPart = GenerateRandomString(4);
        return $"EMP-{now:yyyyMMdd}-{randomPart}";
    }


    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}