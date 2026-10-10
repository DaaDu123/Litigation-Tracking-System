using LTSBackend.Features.CaseWorkflowTemplates.DTOs;
using MediatR;

namespace LTSBackend.Features.CaseWorkflowTemplates.Queries.GetCaseWorkflowTemplateByCategory;

/// <summary>Resolves the ACTIVE template that applies to a category for the caller (own firm's first, else the global one). Null if none is configured.</summary>
public record GetCaseWorkflowTemplateByCategoryQuery(int CategoryID) : IRequest<CaseWorkflowTemplateDTO?>;
