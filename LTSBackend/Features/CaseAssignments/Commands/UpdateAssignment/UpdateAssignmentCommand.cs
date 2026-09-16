using MediatR;
using LTSBackend.Features.CaseAssignments.DTOs;

namespace LTSBackend.Features.CaseAssignments.Commands.UpdateAssignment
{
    public class UpdateAssignmentCommand : IRequest<bool>
    {
        public UpdateAssignmentDTO Assignment { get; set; } = null!;
    }
}
