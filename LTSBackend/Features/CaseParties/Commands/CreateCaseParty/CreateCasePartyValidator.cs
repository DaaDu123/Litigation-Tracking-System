using FluentValidation;

namespace LTSBackend.Features.CaseParties.Commands.CreateCaseParty
{
    public class CreateCasePartyValidator : AbstractValidator<CreateCasePartyCommand>
    {
        private static readonly string[] ValidPartyTypes =
        {
            "Plaintiff", "Defendant", "Petitioner", "Respondent", "Applicant", "Respondent Department"
        };

        // Requires a valid CaseID, a recognised PartyType (Plaintiff, Defendant,
        // Petitioner, Respondent, Applicant, or Respondent Department), a
        // non-empty party name, and a well-formed email if one is supplied.
        public CreateCasePartyValidator()
        {
            RuleFor(x => x.Party).NotNull();

            RuleFor(x => x.Party.CaseID)
                .GreaterThan(0).WithMessage("Case ID must be greater than 0");

            RuleFor(x => x.Party.PartyType)
                .NotEmpty().WithMessage("Party type is required")
                .Must(t => ValidPartyTypes.Contains(t))
                .WithMessage("Party type must be one of: " + "Plaintiff, Defendant, Petitioner, Respondent, Applicant, Respondent Department");

            RuleFor(x => x.Party.PartyName)
                .NotEmpty().WithMessage("Party name is required")
                .MaximumLength(300).WithMessage("Party name cannot exceed 300 characters");

            RuleFor(x => x.Party.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrEmpty(x.Party.Email));

            RuleFor(x => x.Party.CNIC)
                .MaximumLength(20);

            RuleFor(x => x.Party.ContactNo)
                .MaximumLength(30);
        }
    }
}
