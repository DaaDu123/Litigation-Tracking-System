using FluentValidation;

namespace LTSBackend.Features.DocumentTypes.Commands.UpdateDocumentType;

public class UpdateDocumentTypeValidator : AbstractValidator<UpdateDocumentTypeCommand>
{
    // Requires a valid DocumentTypeID plus the same name/description rules
    // as CreateDocumentTypeValidator.
    public UpdateDocumentTypeValidator()
    {
        RuleFor(x => x.DocumentTypeID).GreaterThan(0).WithMessage("Valid document type ID is required.");

        RuleFor(x => x.TypeName)
            .NotEmpty().WithMessage("Type name is required.")
            .MaximumLength(160).WithMessage("Type name cannot exceed 160 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
