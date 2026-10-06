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
    // =====================================================
    // HANDLE — accepts a pending Firm Admin request, creates a firm and
    // either creates a brand-new admin account (anonymous flow) or
    // promotes the requester's OWN existing account (from-account flow -
    // see SubmitFirmAdminRequestFromAccountCommand). Refuses if the
    // request isn't still Pending, re-checks uniqueness (something else
    // may have taken it since submission), then — in one retry-safe
    // transaction — creates the Firm, sets up the admin, marks the
    // request Approved, and writes an audit log entry. Emails the
    // requester on success (best-effort — a failed email doesn't roll
    // back the approval).
    // =====================================================
    public async Task<int> Handle(ApproveFirmAdminRequestCommand request, CancellationToken cancellationToken)
    {
        var firmAdminRequest = await _context.FirmAdminRequests.FirstOrDefaultAsync(x => x.RequestID == request.RequestID, cancellationToken);

        if (firmAdminRequest == null)
            throw new NotFoundException("Firm Admin request not found.");

        if (firmAdminRequest.Status != "Pending")
            throw new ValidationException([$"This request has already been {firmAdminRequest.Status.ToLower()}."]);

        // Registration collects only Email + Password now, so generate
        // internal placeholders here. FirmCode is purely an internal
        // identifier now (never shown to or entered by users) - a GUID
        // fragment guarantees uniqueness without needing a retry loop.
        // The Firm Admin replaces the placeholder FirmName (and everything
        // else) for real during mandatory profile completion.
        var placeholderCode = "FIRM-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        var emailLocalPart = firmAdminRequest.AdminEmail.Split('@')[0];
        var placeholderFirmName = firmAdminRequest.FirmName ?? $"{emailLocalPart}'s Firm (setup pending)";
        var placeholderAdminName = firmAdminRequest.AdminFullName ?? emailLocalPart;

        // EnableRetryOnFailure means a manually-opened transaction can't
        // span retried operations - see CreateFirmCommandHandler for the
        // full explanation of why CreateExecutionStrategy() is required.
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
                // FROM-ACCOUNT FLOW: the requester already has a real
                // account (see SubmitFirmAdminRequestFromAccountCommand) -
                // promote that SAME row in place rather than creating a
                // second one. Their email/password are reused exactly as
                // they already are; nothing is re-entered or duplicated.
                admin = await _context.Users.FirstOrDefaultAsync(x => x.UserID == firmAdminRequest.UserID.Value, cancellationToken)
                    ?? throw new NotFoundException("The requesting user account no longer exists.");

                if (admin.FirmID.HasValue)
                    throw new ValidationException(["This user already belongs to a firm."]);

                admin.RoleID = (int)UserRole.FirmAdmin;
                admin.FirmID = firm.FirmID;
                // The rest of their profile (name/phone/CNIC) was already
                // captured at registration - only the Firm's real details
                // (FirmName, Address, etc.) still need to be collected, so
                // this is flipped back to false to route them through the
                // same CompleteFirmAdminProfileCommand gate once more; it
                // simply re-saves their existing FullName/Phone/CNIC
                // alongside the new Firm details.
                admin.IsProfileCompleted = false;
            }
            else
            {
                // ANONYMOUS/LEGACY FLOW: no existing account - re-check
                // email uniqueness at approval time too (it could have
                // been taken by something else since submission), then
                // create a brand-new User row using the password hash
                // captured at submission time.
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
                    // Mandatory profile completion gate: the Firm Admin cannot
                    // reach the dashboard or any protected feature until they
                    // complete their profile (CompleteFirmAdminProfileCommand),
                    // enforced backend-side by ProfileCompletionBehavior.
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

        // Best-effort - don't fail the approval itself if the email send fails.
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
