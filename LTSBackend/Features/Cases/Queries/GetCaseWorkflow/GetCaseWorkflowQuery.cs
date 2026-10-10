using LTSBackend.Features.Cases.DTOs;
using MediatR;

namespace LTSBackend.Features.Cases.Queries.GetCaseWorkflow;

public record GetCaseWorkflowQuery(long CaseID) : IRequest<CaseWorkflowDTO>;
