using MediatR;

namespace LTSBackend.Features.CaseCategories.Commands.UpdateCaseCategory;

public sealed record UpdateCaseCategoryCommand(int CategoryID, string CategoryName, string? Description, bool IsActive = true) : IRequest<bool>;
