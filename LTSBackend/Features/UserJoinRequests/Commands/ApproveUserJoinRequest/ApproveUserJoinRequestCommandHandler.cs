using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services.Audit;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.UserJoinRequests.Commands.ApproveUserJoinRequest;

public class ApproveUserJoinRequestCommandHandler(AppDbContext _context, IEmailService _emailService, IAuditService _auditService,
    ILogger<ApproveUserJoinRequestCommandHandler> _logger) : IRequestHandler<ApproveUserJoinRequestCommand, int>
{
    // =====================================================
    // HANDLE — accepts a pending join request, activates the requested user
    // Note: _context.UserJoinRequests already carries a tenant query
    // filter (see AppDbContext.OnModelCreating) scoped to the acting
    // FirmAdmin's own FirmID claim, so a FirmAdmin can never even load
    // (let alone approve) a request aimed at a different firm - it comes
    // back as if it doesn't exist, same as querying another firm's Users.
    //
    // Refuses if the request isn't still Pending, re-checks email
    // uniqueness (something else may have taken it since submission),
    // then - in one retry-safe transaction - creates the firm-scoped
    // User (reusing the password hash captured at submission time, never
    // re-touching the plaintext), marks the request Approved, and writes
    // an audit log entry. Emails the requester on success (best-effort -
    // a failed email doesn't roll back the approval).
    // =====================================================
    public async Task<int> Handle(ApproveUserJoinRequestCommand request, CancellationToken cancellationToken)
    {
        var joinRequest = await _context.UserJoinRequests.FirstOrDefaultAsync(x => x.RequestID == request.RequestID, cancellationToken);

        if (joinRequest == null)
            throw new NotFoundException("Join request not found.");

        if (joinRequest.Status != "Pending")
            throw new ValidationException([$"This request has already been {joinRequest.Status.ToLower()}."]);

        // Defense in depth - the requested role was validated at submission
        // time, but re-check here too before it ever becomes a live User.
        if (!RoleHierarchy.IsJoinableRole(joinRequest.RequestedRoleID))
            throw new ValidationException(["This request's role is no longer valid. Reject this request."]);

        // Re-check uniqueness at approval time too - the email could have
        // been taken by something else (e.g. a direct FirmAdmin CreateUser
        // call, or another approved request) in the time since submission.
        bool emailTaken = await _context.Users.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.Email == joinRequest.Email && !x.IsDeleted, cancellationToken);

        if (emailTaken)
            throw new ValidationException([$"Email '{joinRequest.Email}' already exists. Reject this request."]);

        var strategy = _context.Database.CreateExecutionStrategy();

        var newUserId = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var user = new User
            {
                EmployeeNo = $"EMP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}",
                FullName = joinRequest.FullName,
                Email = joinRequest.Email,
                PasswordHash = joinRequest.PasswordHash,
                Phone = joinRequest.Phone,
                Department = joinRequest.Department,
                RoleID = joinRequest.RequestedRoleID,
                FirmID = joinRequest.FirmID,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            joinRequest.Status = "Approved";
            joinRequest.ReviewedBy = request.ActingUserID;
            joinRequest.ReviewedAt = DateTime.UtcNow;
            joinRequest.CreatedUserID = user.UserID;

            var auditLog = _auditService.Create(request.ActingUserID,$"Approved join request #{joinRequest.RequestID} - created user {user.Email} ({(UserRole)user.RoleID!.Value}) in firm {joinRequest.FirmID}");
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return user.UserID;
        });

        _logger.LogInformation("User join request {RequestId} approved by {ActingUserId} - user {UserId} created", joinRequest.RequestID, request.ActingUserID, newUserId);

        // Best-effort - don't fail the approval itself if the email send fails.
        try
        {
            await _emailService.SendNotificationEmailAsync(joinRequest.Email,joinRequest.FullName,"Your Request to Join the Firm Was Approved","Great news! Your request to join the firm has been approved. You can now log in with the email and password you registered with.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval email to {Email}", joinRequest.Email);
        }

        return newUserId;
    }
}
