namespace LTSFrontend.Features.CaseNotes.DTOs
{




    public class CaseNoteFormDTO
    {
        public long NoteID { get; set; }
        public long CaseID { get; set; }
        public string NoteType { get; set; } = "General";
        public string Notes { get; set; } = string.Empty;

        public static readonly string[] NoteTypes = { "Internal", "Confidential", "General" };
    }
}
