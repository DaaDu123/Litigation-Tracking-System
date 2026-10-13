namespace LTSFrontend.Features.Masters.DTOs
{

    public class CaseCategoryDTO
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }




        public bool IsGlobal { get; set; }
    }
}
