namespace LTSBackend.Features.CaseNotes.DTOs
{
    public class CreateCaseNoteDTO
    {
        public long CaseID { get; set; }
        public string NoteType { get; set; } = "General";
        public string Notes { get; set; } = string.Empty;
    }
}
