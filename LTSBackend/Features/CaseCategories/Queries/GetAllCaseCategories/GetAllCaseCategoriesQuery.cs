using LTSBackend.Features.CaseCategories.DTOs;
using MediatR;

namespace LTSBackend.Features.CaseCategories.Queries.GetAllCaseCategories;

public sealed record GetAllCaseCategoriesQuery(string? SearchText = null, bool ActiveOnly = true) : IRequest<List<CaseCategoryDTO>>;
