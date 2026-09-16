using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Masters;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Courts.Commands.CreateCourt;

public sealed class CreateCourtHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<CreateCourtHandler> _logger) : IRequestHandler<CreateCourtCommand, int>
{
    // =====================================================
    // HANDLE — creates a new court
    // Trims input, checks the name is unique among what the caller can
    // see (global courts + their own firm's), then saves it. FirmID is
    // set to the caller's own firm for a FirmAdmin, or left null (global,
    // visible to every firm) for a SuperAdmin.
    // =====================================================
    public async Task<int> Handle(CreateCourtCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating court: {CourtName}", request.CourtName);

        request = request with
        {
            CourtName = request.CourtName.Trim(),
            CourtType = request.CourtType?.Trim(),
            Jurisdiction = request.Jurisdiction?.Trim(),
            Address = request.Address?.Trim()
        };

        // 1. Ensure court name is unique
        // NOTE: this AnyAsync is automatically scoped to what the
        // caller can see (global courts + their own firm's) by the
        // HasQueryFilter on Court in AppDbContext - no manual FirmID
        // filter needed here.
        bool exists = await _context.Courts.AnyAsync(x => x.CourtName.ToLower() == request.CourtName.ToLower(), cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Create failed: Court already exists: {CourtName}", request.CourtName);
            throw new ValidationException(new()
            {
                $"Court '{request.CourtName}' already exists."
            });
        }

        // 2. Create court
        // SuperAdmin creates a system-wide global court (FirmID null,
        // visible to every firm). FirmAdmin creates a court scoped to
        // their own firm only.
        var court = new Court
        {
            FirmID = _currentUser.FirmID,
            CourtName = request.CourtName,
            CourtType = request.CourtType,
            Jurisdiction = request.Jurisdiction,
            Address = request.Address,
            IsActive = request.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        _context.Courts.Add(court);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Court created successfully: {CourtID}", court.CourtID);

        return court.CourtID;
    }
}
