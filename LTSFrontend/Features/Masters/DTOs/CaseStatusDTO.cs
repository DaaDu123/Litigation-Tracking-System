namespace LTSFrontend.Features.Masters.DTOs
{

    public class CaseStatusDTO
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int SequenceNo { get; set; }
        public string ColorCode { get; set; } = string.Empty;
        public bool IsClosed { get; set; }
        public bool IsActive { get; set; }


        public bool IsGlobal { get; set; }
    }
}
