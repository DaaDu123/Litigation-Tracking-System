namespace LTSBackend.Features.Cases.Commands.UpdateCaseStatus
{
    public class UpdateCaseStatusRequest
    {
        public int NewStatusID { get; set; }
        public string? Remarks { get; set; }
    }
}
