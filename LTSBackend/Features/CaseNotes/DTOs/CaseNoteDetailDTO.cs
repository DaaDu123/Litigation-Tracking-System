namespace LTSBackend.Features.CaseNotes.DTOs
{
    public class CaseNoteDetailDTO
    {
        public long NoteID { get; set; }
        public long CaseID { get; set; }
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string NoteType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
