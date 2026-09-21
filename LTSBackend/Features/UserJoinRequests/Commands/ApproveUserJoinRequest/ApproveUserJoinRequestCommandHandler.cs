using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Models.Security;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.UserJoinRequests.Commands.ApproveUserJoinRequest;

public class ApproveUserJoinRequestCommandHandler(AppDbContext _context, IEmailService _emailService, IAuditService _auditService,
    ILogger<ApproveUserJoinRequestCommandHandler> _logger) : IRequestHandler<ApproveUserJoinRequestCommand, int>
{
    // =====================================================
    // HANDLE — accepts a pending join request.
    //
    // New flow (joinRequest.UserID is set): the requester already has a
    // real, profile-completed account with no firm yet. Approving simply
    // attaches them to this firm - FirmID set, role forced to
    // InternParalegal (business rule: accepted users always start as
    // Intern/Paralegal regardless of anything requested), membership
    // state set to Active.
    //
    // Legacy flow (joinRequest.UserID is null, from before this field
    // existed): falls back to the original behavior of creating a brand
    // new User row from the snapshot captured at submission time.
    //
    // Note: _context.UserJoinRequests already carries a tenant query
    // filter (see AppDbContext.OnModelCreating) scoped to the acting
    // FirmAdmin's own FirmID claim, so a FirmAdmin can never even load
    // (let alone approve) a request aimed at a different firm.
    // =====================================================
    public async Task<int> Handle(ApproveUserJoinRequestCommand request, CancellationToken cancellationToken)
    {
        var joinRequest = await _context.UserJoinRequests.FirstOrDefaultAsync(x => x.RequestID == request.RequestID, cancellationToken);

        if (joinRequest == null)
            throw new NotFoundException("Join request not found.");

        if (joinRequest.Status != "Pending")
            throw new ValidationException([$"This request has already been {joinRequest.Status.ToLower()}."]);

        var strategy = _context.Database.CreateExecutionStrategy();

        var resultUserId = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            int userId;
            string userEmail;
            string userFullName;

            if (joinRequest.UserID.HasValue)
            {
                var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.UserID == joinRequest.UserID.Value, cancellationToken)
                    ?? throw new NotFoundException("The requesting user account no longer exists.");

                if (user.FirmID.HasValue)
                    throw new ValidationException(["This user already belongs to a firm."]);

                // Business rule: default role is always Intern/Paralegal on
                // acceptance, regardless of anything the requester picked.
                // The Firm Admin can change it afterward via ChangeFirmUserRole.
                user.RoleID = (int)UserRole.InternParalegal;
                user.FirmID = joinRequest.FirmID;
                user.MembershipStatus = MembershipStatuses.Active;
                user.LastFirmID = joinRequest.FirmID;

                userId = user.UserID;
                userEmail = user.Email;
                userFullName = user.FullName;
            }
            else
            {
                // Legacy path - request predates the UserID link.
                if (string.IsNullOrWhiteSpace(joinRequest.Email) || string.IsNullOrWhiteSpace(joinRequest.PasswordHash))
                    throw new ValidationException(["This legacy request is missing required account details and cannot be approved. Reject it and ask the requester to register again."]);

                bool emailTaken = await _context.Users.IgnoreQueryFilters().AsNoTracking().AnyAsync(x => x.Email == joinRequest.Email && !x.IsDeleted, cancellationToken);
                if (emailTaken)
                    throw new ValidationException([$"Email '{joinRequest.Email}' already exists. Reject this request."]);

                var newUser = new User
                {
                    EmployeeNo = $"EMP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}",
                    FullName = joinRequest.FullName ?? joinRequest.Email.Split('@')[0],
                    Email = joinRequest.Email,
                    PasswordHash = joinRequest.PasswordHash,
                    Phone = joinRequest.Phone,
                    Department = joinRequest.Department,
                    RoleID = (int)UserRole.InternParalegal,
                    FirmID = joinRequest.FirmID,
                    MembershipStatus = MembershipStatuses.Active,
                    LastFirmID = joinRequest.FirmID,
                    IsActive = true,
                    IsDeleted = false,
                    IsProfileCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync(cancellationToken);

                userId = newUser.UserID;
                userEmail = newUser.Email;
                userFullName = newUser.FullName;
            }

            joinRequest.Status = "Approved";
            joinRequest.ReviewedBy = request.ActingUserID;
            joinRequest.ReviewedAt = DateTime.UtcNow;
            joinRequest.CreatedUserID = userId;

            var auditLog = _auditService.Create(request.ActingUserID, $"Approved join request #{joinRequest.RequestID} - {userEmail} joined firm {joinRequest.FirmID} as {UserRole.InternParalegal}");
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return (userId, userEmail, userFullName);
        });

        _logger.LogInformation("User join request {RequestId} approved by {ActingUserId} - user {UserId} attached to firm", joinRequest.RequestID, request.ActingUserID, resultUserId.userId);

        try
        {
            await _emailService.SendNotificationEmailAsync(resultUserId.userEmail, resultUserId.userFullName,
                "Your Request to Join the Firm Was Approved",
                "Great news! Your request to join the firm has been approved. You've joined as Intern/Paralegal - your Firm Admin can update your role at any time. Log back in to get started.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval email to {Email}", resultUserId.userEmail);
        }

        return resultUserId.userId;
    }
}
