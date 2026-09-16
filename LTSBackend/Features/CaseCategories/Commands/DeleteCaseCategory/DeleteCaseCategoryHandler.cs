using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseCategories.Commands.DeleteCaseCategory;

public sealed class DeleteCaseCategoryHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<DeleteCaseCategoryHandler> _logger) : IRequestHandler<DeleteCaseCategoryCommand, bool>
{
    // =====================================================
    // HANDLE — removes a case category the firm no longer needs
    // Same ownership check as Update (own firm's custom case category only),
    // plus an in-use check: cannot delete while cases still reference it.
    // =====================================================
    public async Task<bool> Handle(DeleteCaseCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.CaseCategories.FirstOrDefaultAsync(x => x.CategoryID == request.CategoryID, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Delete failed: Case category not found: {CategoryID}", request.CategoryID);
            throw new NotFoundException("Case category not found.");
        }

        // Ownership check: a FirmAdmin may delete only their OWN firm's custom category - never a global one.
        if (category.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Delete denied: user {UserId} attempted to delete a global/other-firm category {CategoryID}", _currentUser.UserID, request.CategoryID);
            throw new NotFoundException("Case category not found.");
        }

        int caseCount = await _context.Cases.CountAsync(x => x.CategoryID == request.CategoryID, cancellationToken);
        if (caseCount > 0)
        {
            _logger.LogWarning("Delete failed: {Count} case(s) reference category: {CategoryID}", caseCount, request.CategoryID);
            throw new ValidationException(new()
            {
                $"Cannot delete category. {caseCount} case(s) are currently linked to it. Deactivate it instead."
            });
        }

        _context.CaseCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case category deleted successfully: {CategoryID}", request.CategoryID);

        return true;
    }
}
