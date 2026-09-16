using FluentValidation;

namespace LTSBackend.Features.CaseNotes.Commands.UpdateNote
{
    public class UpdateCaseNoteValidator : AbstractValidator<UpdateCaseNoteCommand>
    {
        private static readonly string[] ValidTypes = { "Internal", "Confidential", "General" };

        // Requires a valid NoteID plus the same NoteType/text rules as
        // CreateCaseNoteValidator.
        public UpdateCaseNoteValidator()
        {
            RuleFor(x => x.Note).NotNull();
            RuleFor(x => x.Note.NoteID).GreaterThan(0);
            RuleFor(x => x.Note.NoteType).NotEmpty().Must(t => ValidTypes.Contains(t));
            RuleFor(x => x.Note.Notes).NotEmpty().MaximumLength(4000);
        }
    }
}
