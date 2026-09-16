using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Masters;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseStages.Commands.CreateCaseStage;

public sealed class CreateCaseStageHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<CreateCaseStageHandler> _logger) : IRequestHandler<CreateCaseStageCommand, int>
{
    // =====================================================
    // HANDLE — creates a new case stage
    // Trims input, checks the name is unique among what the caller can
    // see (global stages + their own firm's), then saves it. FirmID is
    // set to the caller's own firm for a FirmAdmin, or left null (global,
    // visible to every firm) for a SuperAdmin.
    // =====================================================
    public async Task<int> Handle(CreateCaseStageCommand request, CancellationToken cancellationToken)
    {
        request = request with { StageName = request.StageName.Trim(), Description = request.Description?.Trim() };

        bool exists = await _context.CaseStages.AnyAsync(x => x.StageName.ToLower() == request.StageName.ToLower(), cancellationToken);
        if (exists)
        {
            _logger.LogWarning("Create failed: Case stage already exists: {StageName}", request.StageName);
            throw new ValidationException(new() { $"Stage '{request.StageName}' already exists." });
        }

        var stage = new CaseStage
        {
            FirmID = _currentUser.FirmID,
            StageName = request.StageName,
            Description = request.Description,
            IsActive = request.IsActive
        };

        _context.CaseStages.Add(stage);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case stage created successfully: {StageID}", stage.StageID);

        return stage.StageID;
    }
}
