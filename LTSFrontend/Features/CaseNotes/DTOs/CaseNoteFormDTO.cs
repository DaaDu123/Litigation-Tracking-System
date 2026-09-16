namespace LTSFrontend.Features.CaseNotes.DTOs
{
    /// <summary>
    /// Shared form model used for both create and update of a case note.
    /// NoteID stays 0 when creating a new note.
    /// </summary>
    public class CaseNoteFormDTO
    {
        public long NoteID { get; set; }
        public long CaseID { get; set; }
        public string NoteType { get; set; } = "General";
        public string Notes { get; set; } = string.Empty;

        public static readonly string[] NoteTypes = { "Internal", "Confidential", "General" };
    }
}
