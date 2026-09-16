using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.CaseStatuses.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseStatuses.Queries.GetCaseStatusById;

public sealed class GetCaseStatusByIdHandler(AppDbContext _context, ILogger<GetCaseStatusByIdHandler> _logger) : IRequestHandler<GetCaseStatusByIdQuery, CaseStatusDTO>
{
    // =====================================================
    // HANDLE — fetches a single case status by ID
    // =====================================================
    public async Task<CaseStatusDTO> Handle(GetCaseStatusByIdQuery request, CancellationToken cancellationToken)
    {
        var status = await _context.CaseStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.StatusID == request.StatusID, cancellationToken);

        if (status == null)
        {
            _logger.LogWarning("Case status not found: {StatusID}", request.StatusID);
            throw new NotFoundException("Case status not found.");
        }

        return new CaseStatusDTO
        {
            StatusID = status.StatusID,
            StatusName = status.StatusName,
            SequenceNo = status.SequenceNo,
            ColorCode = status.ColorCode,
            IsClosed = status.IsClosed,
            IsActive = status.IsActive,
            IsGlobal = status.FirmID == null
        };
    }
}
