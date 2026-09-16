using MediatR;
using LTSBackend.Features.CaseAssignments.DTOs;

namespace LTSBackend.Features.CaseAssignments.Commands.AssignCase
{
    public class AssignCaseCommand : IRequest<long>
    {
        public AssignCaseDTO Assignment { get; set; } = null!;
    }
}
