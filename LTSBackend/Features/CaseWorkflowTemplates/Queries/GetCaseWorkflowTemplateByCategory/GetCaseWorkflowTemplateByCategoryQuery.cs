using LTSBackend.Features.CaseWorkflowTemplates.DTOs;
using MediatR;

namespace LTSBackend.Features.CaseWorkflowTemplates.Queries.GetCaseWorkflowTemplateByCategory;


public record GetCaseWorkflowTemplateByCategoryQuery(int CategoryID) : IRequest<CaseWorkflowTemplateDTO?>;
