using MediatR;

namespace LTSBackend.Features.CaseCategories.Commands.DeleteCaseCategory;

public sealed record DeleteCaseCategoryCommand(int CategoryID) : IRequest<bool>;
