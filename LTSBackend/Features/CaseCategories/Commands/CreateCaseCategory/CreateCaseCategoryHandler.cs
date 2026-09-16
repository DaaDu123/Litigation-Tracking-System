using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Masters;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseCategories.Commands.CreateCaseCategory;

public sealed class CreateCaseCategoryHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<CreateCaseCategoryHandler> _logger) : IRequestHandler<CreateCaseCategoryCommand, int>
{
    // =====================================================
    // HANDLE — creates a new case category
    // Trims input, checks the name is unique among what the caller can
    // see (global categories + their own firm's), then saves it. FirmID is
    // set to the caller's own firm for a FirmAdmin, or left null (global,
    // visible to every firm) for a SuperAdmin.
    // =====================================================
    public async Task<int> Handle(CreateCaseCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating case category: {CategoryName}", request.CategoryName);

        request = request with { CategoryName = request.CategoryName.Trim(), Description = request.Description?.Trim() };

        // NOTE: automatically scoped to global + own firm's categories via the HasQueryFilter on CaseCategory
        bool exists = await _context.CaseCategories.AnyAsync(x => x.CategoryName.ToLower() == request.CategoryName.ToLower(), cancellationToken);
        if (exists)
        {
            _logger.LogWarning("Create failed: Case category already exists: {CategoryName}", request.CategoryName);
            throw new ValidationException(new() { $"Category '{request.CategoryName}' already exists." });
        }

        // SuperAdmin creates a system-wide global category (FirmID null). FirmAdmin creates one scoped to their own firm.
        var category = new CaseCategory
        {
            FirmID = _currentUser.FirmID,
            CategoryName = request.CategoryName,
            Description = request.Description,
            IsActive = request.IsActive
        };

        _context.CaseCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Case category created successfully: {CategoryID}", category.CategoryID);

        return category.CategoryID;
    }
}
