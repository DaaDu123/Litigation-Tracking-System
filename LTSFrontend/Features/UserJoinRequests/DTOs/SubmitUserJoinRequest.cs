using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Features.UserJoinRequests.DTOs
{
    /// <summary>
    /// Mirrors LTSBackend.Features.UserJoinRequests.Commands.SubmitUserJoinRequest.SubmitUserJoinRequestCommand -
    /// sent by an already-registered, already-profile-completed Firm User
    /// from their own dashboard. No personal details here anymore.
    /// </summary>
    public class SubmitUserJoinRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a firm.")]
        public int FirmID { get; set; }
    }
}
