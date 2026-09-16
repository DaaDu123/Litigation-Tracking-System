using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseStages.Commands.UpdateCaseStage;

public sealed class UpdateCaseStageHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<UpdateCaseStageHandler> _logger) : IRequestHandler<UpdateCaseStageCommand, bool>
{
    // =====================================================
    // HANDLE — edits an existing case stage
    // Ownership check: a FirmAdmin may only edit their OWN firm's custom
    // case stage — never a global one or another firm's (returns
    // NotFound rather than Forbidden, so its existence isn't disclosed).
    // Also re-checks name uniqueness before saving.
    // =====================================================
    public async Task<bool> Handle(UpdateCaseStageCommand request, CancellationToken cancellationToken)
    {
        request = request with { StageName = request.StageName.Trim(), Description = request.Description?.Trim() };

        var stage = await _context.CaseStages.FirstOrDefaultAsync(x => x.StageID == request.StageID, cancellationToken);
        if (stage == null)
        {
            _logger.LogWarning("Update failed: Case stage not found: {StageID}", request.StageID);
            throw new NotFoundException("Case stage not found.");
        }

        if (stage.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Update denied: user {UserId} attempted to edit a global/other-firm stage {StageID}", _currentUser.UserID, request.StageID);
            throw new NotFoundException("Case stage not found.");
        }

        bool nameExists = await _context.CaseStages.AnyAsync(x => x.StageID != request.StageID && x.StageName.ToLower() == request.StageName.ToLower(), cancellationToken);
        if (nameExists)
        {
            _logger.LogWarning("Update failed: Case stage name already exists: {StageName}", request.StageName);
            throw new ValidationException(new() { $"Stage '{request.StageName}' already exists." });
        }

        stage.StageName = request.StageName;
        stage.Description = request.Description;
        stage.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case stage updated successfully: {StageID}", request.StageID);

        return true;
    }
}
