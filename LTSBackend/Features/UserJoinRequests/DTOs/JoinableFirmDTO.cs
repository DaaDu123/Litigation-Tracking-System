namespace LTSBackend.Features.UserJoinRequests.DTOs;

/// <summary>
/// Deliberately minimal (no address/contact/internal details) - this is
/// returned from an [AllowAnonymous] endpoint so someone filling out the
/// public join-request form can pick their firm from a dropdown without
/// needing the firm code by heart.
/// </summary>
public class JoinableFirmDTO
{
    public int FirmID { get; set; }
    public string FirmName { get; set; } = string.Empty;
    public string FirmCode { get; set; } = string.Empty;
}
