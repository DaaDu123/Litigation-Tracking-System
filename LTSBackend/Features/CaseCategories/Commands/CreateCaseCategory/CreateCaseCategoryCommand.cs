using MediatR;

namespace LTSBackend.Features.CaseCategories.Commands.CreateCaseCategory;

public sealed record CreateCaseCategoryCommand(string CategoryName, string? Description, bool IsActive = true) : IRequest<int>;
