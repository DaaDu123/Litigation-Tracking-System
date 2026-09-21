using LTSBackend.Comman.Enum;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Security;
using LTSBackend.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Firms.Commands.CreateFirm;

public class CreateFirmCommandHandler(AppDbContext _context,IPasswordService _passwordService,ILogger<CreateFirmCommandHandler> _logger) : IRequestHandler<CreateFirmCommand, int>
{
    // =====================================================
    // HANDLE — provisions a new firm workspace + its first FirmAdmin (UC-00)
    // FirmCode is generated internally (never entered by the SuperAdmin -
    // it's purely an internal reference now, same as the self-service
    // FirmAdminRequest approval flow). Validates the admin email is
    // unique platform-wide, then inside a single retry-safe transaction
    // (see the CreateExecutionStrategy note below) creates the Firm row
    // and bootstraps its first FirmAdmin user account, already active
    // and ready to log in.
    // =====================================================
    public async Task<int> Handle(CreateFirmCommand request, CancellationToken cancellationToken)
    {
        // 1. Admin email must be unique across the whole platform
        bool emailExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Email == request.AdminEmail, cancellationToken);

        if (emailExists)
            throw new ValidationException([$"Email '{request.AdminEmail}' already exists."]);

        var firmCode = await GenerateUniqueFirmCodeAsync(request.FirmName, cancellationToken);

        // EnableRetryOnFailure (Program.cs / AppDbContextFactory) means EF
        // Core's SqlServerRetryingExecutionStrategy is active, which does
        // NOT allow a manually-opened transaction to span multiple retried
        // operations - it throws InvalidOperationException at runtime if
        // you try. Every retriable unit of work (transaction + everything
        // inside it) must instead run through CreateExecutionStrategy().
        // ExecuteAsync(...), which knows how to safely retry the *whole*
        // block (including re-opening the transaction) as one atomic unit.
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            // 2. Create the firm
            var firm = new Firm
            {
                FirmName = request.FirmName,
                FirmCode = firmCode,
                Address = request.Address,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                CreatedBy = request.ActingUserID,
                CreatedAt = DateTime.UtcNow
            };
            _context.Firms.Add(firm);
            await _context.SaveChangesAsync(cancellationToken);

            // 3. Bootstrap the firm's first Firm Admin account
            var admin = new User
            {
                EmployeeNo = $"EMP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}",
                FullName = request.AdminFullName,
                Email = request.AdminEmail,
                PasswordHash = _passwordService.HashPassword(request.AdminPassword),
                RoleID = (int)UserRole.FirmAdmin,
                FirmID = firm.FirmID,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(admin);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Firm {FirmName} ({FirmCode}) created with admin {AdminEmail}",
                firm.FirmName, firm.FirmCode, admin.Email);

            return firm.FirmID;
        });
    }

    /// <summary>
    /// Generates an internal-only FirmCode from a slugified prefix of the
    /// firm name plus a short random suffix, retrying on the rare
    /// collision. Never shown to or entered by any user - purely an
    /// internal reference column on Firm, same role it plays in
    /// ApproveFirmAdminRequestCommandHandler's placeholder generation.
    /// </summary>
    private async Task<string> GenerateUniqueFirmCodeAsync(string firmName, CancellationToken cancellationToken)
    {
        var slug = new string(firmName.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (slug.Length > 12)
            slug = slug[..12];
        if (slug.Length == 0)
            slug = "FIRM";

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"{slug}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            bool exists = await _context.Firms.AsNoTracking().AnyAsync(x => x.FirmCode == candidate, cancellationToken);
            if (!exists)
                return candidate;
        }

        // Astronomically unlikely to ever be reached, but fail safe rather
        // than loop forever.
        return $"{slug}-{Guid.NewGuid():N}"[..30];
    }
}
