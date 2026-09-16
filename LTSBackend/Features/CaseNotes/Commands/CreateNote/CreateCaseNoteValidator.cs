using FluentValidation;

namespace LTSBackend.Features.CaseNotes.Commands.CreateNote
{
    public class CreateCaseNoteValidator : AbstractValidator<CreateCaseNoteCommand>
    {
        private static readonly string[] ValidTypes = { "Internal", "Confidential", "General" };

        // Requires a valid CaseID, a recognised NoteType (Internal, Confidential,
        // or General), and non-empty note text (max 4000 chars).
        public CreateCaseNoteValidator()
        {
            RuleFor(x => x.Note).NotNull();
            RuleFor(x => x.Note.CaseID).GreaterThan(0);
            RuleFor(x => x.Note.NoteType)
                .NotEmpty()
                .Must(t => ValidTypes.Contains(t))
                .WithMessage("NoteType must be Internal, Confidential, or General");
            RuleFor(x => x.Note.Notes)
                .NotEmpty().WithMessage("Note text is required")
                .MaximumLength(4000);
        }
    }
}
