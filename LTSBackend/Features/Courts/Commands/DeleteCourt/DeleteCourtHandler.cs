using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Courts.Commands.DeleteCourt;

public sealed class DeleteCourtHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<DeleteCourtHandler> _logger): IRequestHandler<DeleteCourtCommand, bool>
{





    public async Task<bool> Handle(DeleteCourtCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting court: {CourtID}", request.CourtID);


        var court = await _context.Courts.FirstOrDefaultAsync(x => x.CourtID == request.CourtID, cancellationToken);

        if (court == null)
        {
            _logger.LogWarning("Delete failed: Court not found: {CourtID}", request.CourtID);
            throw new NotFoundException("Court not found.");
        }




        if (court.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Delete denied: user {UserId} attempted to delete a global/other-firm court {CourtID}", _currentUser.UserID, request.CourtID);
            throw new NotFoundException("Court not found.");
        }


        int caseCount = await _context.Cases.CountAsync(x => x.CourtID == request.CourtID, cancellationToken);

        if (caseCount > 0)
        {
            _logger.LogWarning("Delete failed: {Count} case(s) reference court: {CourtID}",caseCount,request.CourtID);

            throw new ValidationException(new()
            {
                $"Cannot delete court. {caseCount} case(s) are currently linked to it."
            });
        }


        int hearingCount = await _context.Hearings.CountAsync(x => x.CourtID == request.CourtID, cancellationToken);

        if (hearingCount > 0)
        {
            _logger.LogWarning("Delete failed: {Count} hearing(s) reference court: {CourtID}",hearingCount,request.CourtID);

            throw new ValidationException(new()
            {
                $"Cannot delete court. {hearingCount} hearing record(s) are currently linked to it."
            });
        }


        _context.Courts.Remove(court);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Court deleted successfully: {CourtID}", request.CourtID);

        return true;
    }
}
