using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.UserJoinRequests.DTOs
{





    public class SubmitUserJoinRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a firm.")]
        public int FirmID { get; set; }
    }
}
