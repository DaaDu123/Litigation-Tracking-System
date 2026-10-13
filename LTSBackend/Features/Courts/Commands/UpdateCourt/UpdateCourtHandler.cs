using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Courts.Commands.UpdateCourt;

public sealed class UpdateCourtHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<UpdateCourtHandler> _logger) : IRequestHandler<UpdateCourtCommand, bool>
{







    public async Task<bool> Handle(UpdateCourtCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating court: {CourtID}", request.CourtID);

        request = request with
        {
            CourtName = request.CourtName.Trim(),
            CourtType = request.CourtType?.Trim(),
            Jurisdiction = request.Jurisdiction?.Trim(),
            Address = request.Address?.Trim()
        };



        var court = await _context.Courts.FirstOrDefaultAsync(x => x.CourtID == request.CourtID, cancellationToken);

        if (court == null)
        {
            _logger.LogWarning("Update failed: Court not found: {CourtID}", request.CourtID);
            throw new NotFoundException("Court not found.");
        }




        if (court.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Update denied: user {UserId} attempted to edit a global/other-firm court {CourtID}", _currentUser.UserID, request.CourtID);
            throw new NotFoundException("Court not found.");
        }


        bool nameExists = await _context.Courts.AnyAsync(x => x.CourtID != request.CourtID && x.CourtName.ToLower() == request.CourtName.ToLower(),cancellationToken);

        if (nameExists)
        {
            _logger.LogWarning("Update failed: Court name already exists: {CourtName}", request.CourtName);
            throw new ValidationException(new()
            {
                $"Court '{request.CourtName}' already exists."
            });
        }


        court.CourtName = request.CourtName;
        court.CourtType = request.CourtType;
        court.Jurisdiction = request.Jurisdiction;
        court.Address = request.Address;
        court.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Court updated successfully: {CourtID}", request.CourtID);

        return true;
    }
}
