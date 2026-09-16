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
    // Validates the FirmCode and admin email are both unique platform-wide,
    // then inside a single retry-safe transaction (see the
    // CreateExecutionStrategy note below) creates the Firm row and
    // bootstraps its first FirmAdmin user account, already active and
    // ready to log in.
    // =====================================================
    public async Task<int> Handle(CreateFirmCommand request, CancellationToken cancellationToken)
    {
        // 1. FirmCode must be unique
        bool codeExists = await _context.Firms
            .AsNoTracking()
            .AnyAsync(x => x.FirmCode == request.FirmCode, cancellationToken);

        if (codeExists)
            throw new ValidationException([$"Firm code '{request.FirmCode}' is already in use."]);

        // 2. Admin email must be unique across the whole platform
        bool emailExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Email == request.AdminEmail, cancellationToken);

        if (emailExists)
            throw new ValidationException([$"Email '{request.AdminEmail}' already exists."]);

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

            // 3. Create the firm
            var firm = new Firm
            {
                FirmName = request.FirmName,
                FirmCode = request.FirmCode.Trim().ToUpperInvariant(),
                Address = request.Address,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                CreatedBy = request.ActingUserID,
                CreatedAt = DateTime.UtcNow
            };
            _context.Firms.Add(firm);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Bootstrap the firm's first Firm Admin account
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
}
