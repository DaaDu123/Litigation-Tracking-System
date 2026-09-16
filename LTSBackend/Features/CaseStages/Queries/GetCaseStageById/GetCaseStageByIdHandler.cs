using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Features.CaseStages.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseStages.Queries.GetCaseStageById;

public sealed class GetCaseStageByIdHandler(AppDbContext _context, ILogger<GetCaseStageByIdHandler> _logger) : IRequestHandler<GetCaseStageByIdQuery, CaseStageDTO>
{
    // =====================================================
    // HANDLE — fetches a single case stage by ID
    // =====================================================
    public async Task<CaseStageDTO> Handle(GetCaseStageByIdQuery request, CancellationToken cancellationToken)
    {
        var stage = await _context.CaseStages.AsNoTracking().FirstOrDefaultAsync(x => x.StageID == request.StageID, cancellationToken);

        if (stage == null)
        {
            _logger.LogWarning("Case stage not found: {StageID}", request.StageID);
            throw new NotFoundException("Case stage not found.");
        }

        return new CaseStageDTO
        {
            StageID = stage.StageID,
            StageName = stage.StageName,
            Description = stage.Description,
            IsActive = stage.IsActive,
            IsGlobal = stage.FirmID == null
        };
    }
}
