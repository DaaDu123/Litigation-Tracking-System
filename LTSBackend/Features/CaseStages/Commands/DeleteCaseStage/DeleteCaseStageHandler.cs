using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseStages.Commands.DeleteCaseStage;

public sealed class DeleteCaseStageHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<DeleteCaseStageHandler> _logger) : IRequestHandler<DeleteCaseStageCommand, bool>
{
    // =====================================================
    // HANDLE — removes a case stage the firm no longer needs
    // Same ownership check as Update (own firm's custom case stage only),
    // plus an in-use check: cannot delete while cases still reference it.
    // =====================================================
    public async Task<bool> Handle(DeleteCaseStageCommand request, CancellationToken cancellationToken)
    {
        var stage = await _context.CaseStages.FirstOrDefaultAsync(x => x.StageID == request.StageID, cancellationToken);
        if (stage == null)
        {
            _logger.LogWarning("Delete failed: Case stage not found: {StageID}", request.StageID);
            throw new NotFoundException("Case stage not found.");
        }

        if (stage.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Delete denied: user {UserId} attempted to delete a global/other-firm stage {StageID}", _currentUser.UserID, request.StageID);
            throw new NotFoundException("Case stage not found.");
        }

        int caseCount = await _context.Cases.CountAsync(x => x.StageID == request.StageID, cancellationToken);
        if (caseCount > 0)
        {
            _logger.LogWarning("Delete failed: {Count} case(s) reference stage: {StageID}", caseCount, request.StageID);
            throw new ValidationException(new()
            {
                $"Cannot delete stage. {caseCount} case(s) are currently linked to it. Deactivate it instead."
            });
        }

        _context.CaseStages.Remove(stage);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case stage deleted successfully: {StageID}", request.StageID);

        return true;
    }
}
