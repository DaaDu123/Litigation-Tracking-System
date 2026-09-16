using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseCategories.Commands.UpdateCaseCategory;

public sealed class UpdateCaseCategoryHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<UpdateCaseCategoryHandler> _logger) : IRequestHandler<UpdateCaseCategoryCommand, bool>
{
    // =====================================================
    // HANDLE — edits an existing case category
    // Ownership check: a FirmAdmin may only edit their OWN firm's custom
    // case category — never a global one or another firm's (returns
    // NotFound rather than Forbidden, so its existence isn't disclosed).
    // Also re-checks name uniqueness before saving.
    // =====================================================
    public async Task<bool> Handle(UpdateCaseCategoryCommand request, CancellationToken cancellationToken)
    {
        request = request with { CategoryName = request.CategoryName.Trim(), Description = request.Description?.Trim() };

        var category = await _context.CaseCategories.FirstOrDefaultAsync(x => x.CategoryID == request.CategoryID, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Update failed: Case category not found: {CategoryID}", request.CategoryID);
            throw new NotFoundException("Case category not found.");
        }

        // Ownership check: a FirmAdmin may edit only their OWN firm's custom category - never a global one.
        if (category.FirmID != _currentUser.FirmID)
        {
            _logger.LogWarning("Update denied: user {UserId} attempted to edit a global/other-firm category {CategoryID}", _currentUser.UserID, request.CategoryID);
            throw new NotFoundException("Case category not found.");
        }

        bool nameExists = await _context.CaseCategories.AnyAsync(x => x.CategoryID != request.CategoryID && x.CategoryName.ToLower() == request.CategoryName.ToLower(), cancellationToken);
        if (nameExists)
        {
            _logger.LogWarning("Update failed: Case category name already exists: {CategoryName}", request.CategoryName);
            throw new ValidationException(new() { $"Category '{request.CategoryName}' already exists." });
        }

        category.CategoryName = request.CategoryName;
        category.Description = request.Description;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case category updated successfully: {CategoryID}", request.CategoryID);

        return true;
    }
}
