using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services.AccessRequests;
using LTSBackend.Services.Audit;
using LTSBackend.Services.Email;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.FirmAdminRequests.Commands.ApproveFirmAdminRequest;

public class ApproveFirmAdminRequestCommandHandler(AppDbContext _context,IEmailService _emailService,IAuditService _auditService,
    ILogger<ApproveFirmAdminRequestCommandHandler> _logger) : IRequestHandler<ApproveFirmAdminRequestCommand, int>
{












    public async Task<int> Handle(ApproveFirmAdminRequestCommand request, CancellationToken cancellationToken)
    {
        var firmAdminRequest = await _context.FirmAdminRequests.FirstOrDefaultAsync(x => x.RequestID == request.RequestID, cancellationToken);

        if (firmAdminRequest == null)
            throw new NotFoundException("Firm Admin request not found.");

        if (firmAdminRequest.Status != "Pending")
            throw new ValidationException([$"This request has already been {firmAdminRequest.Status.ToLower()}."]);







        var placeholderCode = "FIRM-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        var emailLocalPart = firmAdminRequest.AdminEmail.Split('@')[0];
        var placeholderFirmName = firmAdminRequest.FirmName ?? $"{emailLocalPart}'s Firm (setup pending)";
        var placeholderAdminName = firmAdminRequest.AdminFullName ?? emailLocalPart;




        var strategy = _context.Database.CreateExecutionStrategy();

        var newFirmId = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var firm = new Firm
            {
                FirmName = placeholderFirmName,
                FirmCode = placeholderCode,
                Address = firmAdminRequest.Address,
                ContactEmail = firmAdminRequest.ContactEmail,
                ContactPhone = firmAdminRequest.ContactPhone,
                CreatedBy = request.ActingUserID,
                CreatedAt = DateTime.UtcNow
            };
            _context.Firms.Add(firm);
            await _context.SaveChangesAsync(cancellationToken);

            User admin;

            if (firmAdminRequest.UserID.HasValue)
            {





                admin = await _context.Users.FirstOrDefaultAsync(x => x.UserID == firmAdminRequest.UserID.Value, cancellationToken)
                    ?? throw new NotFoundException("The requesting user account no longer exists.");

                if (admin.FirmID.HasValue)
                    throw new ValidationException(["This user already belongs to a firm."]);

                admin.RoleID = (int)UserRole.FirmAdmin;
                admin.FirmID = firm.FirmID;







                admin.IsProfileCompleted = false;
            }
            else
            {





                bool emailTaken = await _context.Users.AsNoTracking().AnyAsync(x => x.Email == firmAdminRequest.AdminEmail, cancellationToken);

                if (emailTaken)
                    throw new ValidationException([$"Email '{firmAdminRequest.AdminEmail}' already exists. Reject this request."]);

                if (string.IsNullOrWhiteSpace(firmAdminRequest.AdminPasswordHash))
                    throw new ValidationException(["This request is missing required account details and cannot be approved. Reject it and ask the requester to submit again."]);

                admin = new User
                {
                    EmployeeNo = $"EMP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}",
                    FullName = placeholderAdminName,
                    Email = firmAdminRequest.AdminEmail,
                    PasswordHash = firmAdminRequest.AdminPasswordHash,
                    Phone = firmAdminRequest.AdminPhone,
                    RoleID = (int)UserRole.FirmAdmin,
                    FirmID = firm.FirmID,
                    IsActive = true,
                    IsDeleted = false,




                    IsProfileCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(admin);
            }

            await _context.SaveChangesAsync(cancellationToken);

            firmAdminRequest.Status = "Approved";
            firmAdminRequest.ReviewedBy = request.ActingUserID;
            firmAdminRequest.ReviewedAt = DateTime.UtcNow;
            firmAdminRequest.CreatedFirmID = firm.FirmID;

            await AccessRequestSlot.ReleaseAsync(_context, firmAdminRequest.UserID, LTSBackend.Models.Security.UserAccessRequestSlot.TargetSuperAdmin, cancellationToken);

            var auditLog = _auditService.Create(request.ActingUserID,$"Approved Firm Admin request #{firmAdminRequest.RequestID} - created firm '{firm.FirmName}' ({firm.FirmCode}) with admin {admin.Email}");
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return firm.FirmID;
        });

        _logger.LogInformation("Firm Admin request {RequestId} approved by {ActingUserId} - firm {FirmId} created",firmAdminRequest.RequestID, request.ActingUserID, newFirmId);


        try
        {
            await _emailService.SendNotificationEmailAsync(
                firmAdminRequest.AdminEmail,
                placeholderAdminName,
                "Your Firm Admin Request Was Approved",
                $"Great news! Your request to create a firm workspace has been approved. " +
                $"You can now log in with the email and password you registered with. " +
                $"On first login you'll be asked to complete your profile and set up your firm's details before you can access your dashboard.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval email to {Email}", firmAdminRequest.AdminEmail);
        }

        return newFirmId;
    }
}
