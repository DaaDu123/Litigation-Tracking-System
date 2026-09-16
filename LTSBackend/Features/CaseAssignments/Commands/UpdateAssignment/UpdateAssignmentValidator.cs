using FluentValidation;

namespace LTSBackend.Features.CaseAssignments.Commands.UpdateAssignment
{
    public class UpdateAssignmentValidator : AbstractValidator<UpdateAssignmentCommand>
    {
        private static readonly string[] ValidTypes = { "Legal Officer", "Supervisor", "Lawyer", "External Counsel" };

        // Requires a valid AssignmentID and a recognised AssignmentType.
        public UpdateAssignmentValidator()
        {
            RuleFor(x => x.Assignment).NotNull();
            RuleFor(x => x.Assignment.AssignmentID).GreaterThan(0);
            RuleFor(x => x.Assignment.AssignmentType).NotEmpty().Must(t => ValidTypes.Contains(t));
        }
    }
}
