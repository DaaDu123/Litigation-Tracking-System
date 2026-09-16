using FluentValidation;

namespace LTSBackend.Features.CaseAssignments.Commands.AssignCase
{
    public class AssignCaseValidator : AbstractValidator<AssignCaseCommand>
    {
        private static readonly string[] ValidTypes = { "Legal Officer", "Supervisor", "Lawyer", "External Counsel" };

        // Requires a valid Case/User ID and a recognised AssignmentType
        // (Legal Officer, Supervisor, Lawyer, or External Counsel).
        public AssignCaseValidator()
        {
            RuleFor(x => x.Assignment).NotNull();
            RuleFor(x => x.Assignment.CaseID).GreaterThan(0);
            RuleFor(x => x.Assignment.UserID).GreaterThan(0);
            RuleFor(x => x.Assignment.AssignmentType)
                .NotEmpty()
                .Must(t => ValidTypes.Contains(t))
                .WithMessage("AssignmentType must be Legal Officer, Supervisor, Lawyer, or External Counsel");
        }
    }
}
