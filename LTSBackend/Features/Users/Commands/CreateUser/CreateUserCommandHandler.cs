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
    // =====================================================
    // HANDLE — creates a new firm user, or reclaims a soft-deleted email
    // Enforces: SuperAdmin can't create firm users here (must bootstrap a
    // firm via POST /api/firms instead); a live email can never be
    // reused; a soft-deleted email is only reusable by its own original
    // firm, or by any firm once a SuperAdmin has explicitly released it
    // (ReleaseUserEmailCommand); the target role must exist and must pass
    // RoleHierarchy.CanAssignRole against the acting user's own role (no
    // assigning a role above your own, no assigning SuperAdmin). On the
    // reuse path, the original row (same UserID/EmployeeNo/CreatedAt) is
    // restored and reassigned rather than inserting a new one, and its
    // SecurityStamp is rotated to invalidate any stale tokens. Guards
    // against a concurrent duplicate-email race via the DB's unique index
    // rather than trusting the earlier existence check alone.
    // =====================================================
    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create user request for email: {Email}", request.Email);

        // Trim leading/trailing whitespace on all free-text fields so an
        // accidental leading/trailing space typed by the user is never
        // persisted.
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? request.Phone : request.Phone.Trim();
        var department = string.IsNullOrWhiteSpace(request.Department) ? request.Department : request.Department.Trim();

        // 0. Load the acting user first (needed both for the
        // role-hierarchy checks below AND for the email-reuse
        // ownership check in step 1 - we need to know the acting
        // firm before we can decide whether a soft-deleted row with
        // this email belongs to "us" or to another firm).
        var actingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserID == request.ActingUserID, cancellationToken);
        if (actingUser == null)
        {
            _logger.LogWarning("User creation failed: acting user not found: {ActingUserId}", request.ActingUserID);
            throw new ValidationException(["Could not identify the requesting user."]);
        }

        // 2c. Multi-tenancy: new user inherits the acting user's
        // firm. SuperAdmin has no firm of their own, so they can't
        // create firm-scoped users via this endpoint - firms are
        // bootstrapped (with their first Firm Admin) via
        // POST /api/firms instead.
        if (actingUser.FirmID == null)
        {
            _logger.LogWarning("SuperAdmin {ActingUserId} attempted to create a user via /api/users instead of /api/firms", request.ActingUserID);
            throw new ValidationException(["SuperAdmin cannot create a firm user via this endpoint - create the firm first via POST /api/firms."]);
        }

        // 1. Check whether the email already exists, and whether a
        // soft-deleted row for it can be reused.
        var existingUser = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        bool isReuse = false;

        if (existingUser != null)
        {
            if (!existingUser.IsDeleted)
            {
                // Live user already owns this email - own firm or another
                // firm, doesn't matter, nobody else can take it.
                _logger.LogWarning("User creation failed: Email already exists and is active: {Email}", email);
                throw new ValidationException([$"Email '{email}' already exists and is currently assigned to another user."]);
            }

            // existingUser.IsDeleted == true: a soft-deleted record for
            // this email exists somewhere. Ownership rule: FirmID is left
            // untouched on delete, so it still tells us who "owns" the
            // reservation.
            bool sameFirm = existingUser.FirmID == actingUser.FirmID;

            if (!sameFirm && !existingUser.IsReleasedForReuse)
            {
                _logger.LogWarning("User creation failed: Email {Email} is a deleted record owned by firm {OwnerFirmId}, not released, requested by firm {RequestingFirmId}",email, existingUser.FirmID, actingUser.FirmID);
                throw new ValidationException([$"Email '{email}' already exists and is reserved by another firm's deleted user record. " + "It must be released for reassignment before it can be reused here."]);
            }

            // Either the same firm reclaiming its own deleted user, or a
            // different firm reusing a record that was explicitly released.
            isReuse = true;
        }

        // 2. Default the role when the caller didn't supply one (the
        // "quick add" flow only collects Email + Temp Password) — every
        // new user starts as Intern/Paralegal, the lowest-privilege role,
        // and the Firm Admin can promote them later from User Management.
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

        // 2b. Enforce role hierarchy — the acting user cannot
        // assign a role above their own or assign SuperAdmin
        var actingRole = actingUser.GetRole();
        if (actingRole == null || !RoleHierarchy.CanAssignRole(actingRole.Value, roleId))
        {
            _logger.LogWarning("User {ActingUserId} with role {ActingRole} attempted to assign disallowed role {TargetRoleId}", request.ActingUserID, actingRole, roleId);
            throw new ValidationException(["You are not authorized to assign this role."]);
        }

        // 3. Get role details
        var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.RoleID == roleId, cancellationToken);

        _logger.LogInformation("Role fetched: {RoleName}", role?.RoleName);

        // 4. Validate Department if one was provided
        if (!string.IsNullOrEmpty(department))
        {
            bool deptExists = await _context.Departments.AsNoTracking().AnyAsync(x => x.DepartmentName == department, cancellationToken);

            if (!deptExists)
            {
                _logger.LogInformation("Department not found, user will be created without a department");
            }
        }

        // 5. Hash the password
        string passwordHash = _passwordService.HashPassword(request.Password);

        // 6. Handle profile image upload
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

        // 7/8. Either reuse the existing soft-deleted record, or
        // create a brand new User row.
        int resultUserId;
        User? newUser = null;

        if (isReuse)
        {
            // REUSE PATH — restore the original row instead of
            // inserting a new one. Same UserID, same EmployeeNo,
            // same historical CreatedAt/audit trail; everything
            // else is overwritten with the new assignment's data.
            // Must re-fetch WITH tracking + IgnoreQueryFilters(),
            // since the earlier lookup was a fresh query and the
            // row is invisible to normal tracked queries while
            // IsDeleted == true.
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
            userToRestore.IsReleasedForReuse = false; // reservation consumed
            userToRestore.FailedLoginAttempts = 0;
            userToRestore.LastLogin = null;
            userToRestore.PasswordChangedDate = DateTime.UtcNow;
            userToRestore.SecurityStamp = Guid.NewGuid().ToString("N"); // invalidate any stale tokens
            userToRestore.UpdatedAt = DateTime.UtcNow;
            // EmployeeNo, UserID and CreatedAt deliberately untouched —
            // this is still, by identity, the same record/person history.

            resultUserId = userToRestore.UserID;
        }
        else
        {
            // NORMAL PATH — brand new person, brand new row.
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
            resultUserId = 0; // populated after SaveChanges below
        }

        // 9. Persist. Race-condition guard: two concurrent requests
        // could both pass the email-existence check above before
        // either commits (classic TOCTOU). The filtered unique index
        // on Users.Email is the real backstop - catch its violation
        // here and turn it into the same clean validation message
        // instead of an unhandled 500.
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
            // SaveChanges populates the identity value straight onto the
            // tracked entity — no need for a second round-trip.
            resultUserId = newUser.UserID;
        }

        // 10. Queue the "Complete Your Profile" notification (in-app +
        // email, dispatched by NotificationEmailDispatcherService) so a
        // user created via the quick-add flow (Email + Temp Password
        // only) is prompted to fill in their name/phone/department/etc.
        // Needs resultUserId, so this is a second, tiny SaveChanges.
        _context.Notifications.Add(new Notification
        {
            NotificationTypeID = 8, // CompleteProfile (seeded in AppDbContext)
            UserID = resultUserId,
            Subject = "Complete Your Profile",
            Message = "Your account has been created. Please sign in and complete your profile " + "(name, phone, department, and other details) to get started.",
            Priority = "Medium",
            CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Action} successfully with ID: {UserID} and Role: {RoleName}", isReuse ? "restored/reused" : "created", resultUserId, role?.RoleName);

        // No manual audit log here — the AuditBehavior pipeline already handles this.

        return resultUserId;
    }

    // Builds a new EmployeeNo in the EMP-yyyyMMdd-XXXX format.
    private static string GenerateEmployeeNo()
    {
        var now = DateTime.UtcNow;
        var randomPart = GenerateRandomString(4);
        return $"EMP-{now:yyyyMMdd}-{randomPart}";
    }

    // Generates a short random alphanumeric suffix for the employee number.
    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}