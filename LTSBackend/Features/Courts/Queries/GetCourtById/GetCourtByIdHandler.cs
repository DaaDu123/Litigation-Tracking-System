using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.Courts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Courts.Queries.GetCourtById;

public sealed class GetCourtByIdHandler(AppDbContext _context, ILogger<GetCourtByIdHandler> _logger) : IRequestHandler<GetCourtByIdQuery, CourtDTO>
{
    // =====================================================
    // HANDLE — fetches a single court by ID
    // =====================================================
    public async Task<CourtDTO> Handle(GetCourtByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching court: {CourtID}", request.CourtID);

        var court = await _context.Courts.AsNoTracking().FirstOrDefaultAsync(x => x.CourtID == request.CourtID, cancellationToken);

        if (court == null)
        {
            _logger.LogWarning("Court not found: {CourtID}", request.CourtID);
            throw new NotFoundException("Court not found.");
        }

        return new CourtDTO
        {
            CourtID = court.CourtID,
            CourtName = court.CourtName,
            CourtType = court.CourtType,
            Jurisdiction = court.Jurisdiction,
            Address = court.Address,
            IsActive = court.IsActive,
            CreatedDate = court.CreatedDate,
            IsGlobal = court.FirmID == null
        };
    }
}
