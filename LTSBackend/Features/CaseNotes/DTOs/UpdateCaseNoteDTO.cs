namespace LTSBackend.Features.CaseNotes.DTOs
{
    public class UpdateCaseNoteDTO
    {
        public long NoteID { get; set; }
        public string NoteType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
