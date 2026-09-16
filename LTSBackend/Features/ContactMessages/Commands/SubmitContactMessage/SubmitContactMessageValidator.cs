using FluentValidation;

namespace LTSBackend.Features.ContactMessages.Commands.SubmitContactMessage;

public class SubmitContactMessageValidator : AbstractValidator<SubmitContactMessageCommand>
{
    public SubmitContactMessageValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.Phone));
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}
