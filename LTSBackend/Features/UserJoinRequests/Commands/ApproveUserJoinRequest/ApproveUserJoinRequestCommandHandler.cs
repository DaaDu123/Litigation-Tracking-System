using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Models.Security;
using LTSBackend.Data;
using LTSBackend.Services.Audit;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LTSBackend.Services.AccessRequests;

namespace LTSBackend.Features.UserJoinRequests.Commands.ApproveUserJoinRequest;

public class ApproveUserJoinRequestCommandHandler(AppDbContext _context, IEmailService _emailService, IAuditService _auditService,
    ILogger<ApproveUserJoinRequestCommandHandler> _logger) : IRequestHandler<ApproveUserJoinRequestCommand, int>
{



















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

            await AccessRequestSlot.ReleaseAsync(_context, joinRequest.UserID, UserAccessRequestSlot.TargetFirmAdmin, cancellationToken);

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
