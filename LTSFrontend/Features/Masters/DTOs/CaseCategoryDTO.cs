namespace LTSFrontend.Features.Masters.DTOs
{
    /// <summary>Mirrors LTSBackend.Features.CaseCategories.DTOs.CaseCategoryDTO</summary>
    public class CaseCategoryDTO
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        /// <summary>True = system-wide category shared across all firms. Only SuperAdmin
        /// can edit/deactivate/delete these; a FirmAdmin can use it on cases but the
        /// edit/delete actions must be hidden for it in the UI.</summary>
        public bool IsGlobal { get; set; }
    }
}
