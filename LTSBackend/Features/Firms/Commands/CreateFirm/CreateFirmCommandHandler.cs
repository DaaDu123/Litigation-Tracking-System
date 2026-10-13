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










    public async Task<int> Handle(CreateFirmCommand request, CancellationToken cancellationToken)
    {

        bool emailExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Email == request.AdminEmail, cancellationToken);

        if (emailExists)
            throw new ValidationException([$"Email '{request.AdminEmail}' already exists."]);

        var firmCode = await GenerateUniqueFirmCodeAsync(request.FirmName, cancellationToken);









        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);


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



        return $"{slug}-{Guid.NewGuid():N}"[..30];
    }
}
