namespace LTSFrontend.Features.SuperAdminRequests.DTOs
{











    public class SuperAdminRequestDTO
    {
        public int RequestID { get; set; }


        public string Status { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}
