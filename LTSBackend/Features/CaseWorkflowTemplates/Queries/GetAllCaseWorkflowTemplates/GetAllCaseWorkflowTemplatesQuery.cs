using LTSBackend.Features.CaseWorkflowTemplates.DTOs;
using MediatR;

namespace LTSBackend.Features.CaseWorkflowTemplates.Queries.GetAllCaseWorkflowTemplates;

public record GetAllCaseWorkflowTemplatesQuery(bool ActiveOnly) : IRequest<List<CaseWorkflowTemplateDTO>>;
