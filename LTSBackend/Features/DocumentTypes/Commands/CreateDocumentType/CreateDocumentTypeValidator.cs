using FluentValidation;

namespace LTSBackend.Features.DocumentTypes.Commands.CreateDocumentType;

public class CreateDocumentTypeValidator : AbstractValidator<CreateDocumentTypeCommand>
{


    public CreateDocumentTypeValidator()
    {
        RuleFor(x => x.TypeName)
            .NotEmpty().WithMessage("Type name is required.")
            .MaximumLength(160).WithMessage("Type name cannot exceed 160 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
