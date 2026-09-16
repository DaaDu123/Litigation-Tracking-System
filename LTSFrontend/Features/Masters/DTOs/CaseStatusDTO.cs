namespace LTSFrontend.Features.Masters.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.CaseStatuses.DTOs.CaseStatusDTO</summary>
    public class CaseStatusDTO
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int SequenceNo { get; set; }
        public string ColorCode { get; set; } = string.Empty;
        public bool IsClosed { get; set; }
        public bool IsActive { get; set; }

        /// <summary>True = system-wide record shared across all firms. Only SuperAdmin can edit/deactivate/delete these.</summary>
        public bool IsGlobal { get; set; }
    }
}
