namespace LTSFrontend.Features.Masters.DTOs
{

    public class DepartmentDTO
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? DepartmentCode { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }


        public bool IsGlobal { get; set; }
    }
}
