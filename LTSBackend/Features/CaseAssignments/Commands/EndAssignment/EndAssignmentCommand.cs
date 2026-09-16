using MediatR;

namespace LTSBackend.Features.CaseAssignments.Commands.EndAssignment
{
    public class EndAssignmentCommand : IRequest<bool>
    {
        public long AssignmentID { get; set; }
        public string? Remarks { get; set; }
    }
}
