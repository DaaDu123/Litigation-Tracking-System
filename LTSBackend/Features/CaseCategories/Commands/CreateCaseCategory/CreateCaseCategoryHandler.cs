using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using LTSBackend.Models.Masters;
using LTSBackend.Services.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.CaseCategories.Commands.CreateCaseCategory;

public sealed class CreateCaseCategoryHandler(AppDbContext _context, ICurrentUserService _currentUser, ILogger<CreateCaseCategoryHandler> _logger) : IRequestHandler<CreateCaseCategoryCommand, int>
{







    public async Task<int> Handle(CreateCaseCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating case category: {CategoryName}", request.CategoryName);

        request = request with { CategoryName = request.CategoryName.Trim(), Description = request.Description?.Trim() };


        bool exists = await _context.CaseCategories.AnyAsync(x => x.CategoryName.ToLower() == request.CategoryName.ToLower(), cancellationToken);
        if (exists)
        {
            _logger.LogWarning("Create failed: Case category already exists: {CategoryName}", request.CategoryName);
            throw new ValidationException(new() { $"Category '{request.CategoryName}' already exists." });
        }


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
