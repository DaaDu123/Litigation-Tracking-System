using FluentValidation;

namespace LTSBackend.Features.CaseParties.Commands.UpdateCaseParty
{
    public class UpdateCasePartyValidator : AbstractValidator<UpdateCasePartyCommand>
    {
        private static readonly string[] ValidPartyTypes =
        {
            "Plaintiff", "Defendant", "Petitioner", "Respondent", "Applicant", "Respondent Department"
        };

        // Requires a valid PartyID plus the same PartyType/name/email rules as
        // CreateCasePartyValidator.
        public UpdateCasePartyValidator()
        {
            RuleFor(x => x.Party).NotNull();
            RuleFor(x => x.Party.PartyID).GreaterThan(0);
            RuleFor(x => x.Party.PartyType)
                .NotEmpty()
                .Must(t => ValidPartyTypes.Contains(t))
                .WithMessage("Invalid party type");
            RuleFor(x => x.Party.PartyName).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Party.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Party.Email));
        }
    }
}
