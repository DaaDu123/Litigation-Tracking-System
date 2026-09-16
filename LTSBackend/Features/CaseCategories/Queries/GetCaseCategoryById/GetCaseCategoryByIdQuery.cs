using LTSBackend.Features.CaseCategories.DTOs;
using MediatR;

namespace LTSBackend.Features.CaseCategories.Queries.GetCaseCategoryById;

public sealed record GetCaseCategoryByIdQuery(int CategoryID) : IRequest<CaseCategoryDTO>;
