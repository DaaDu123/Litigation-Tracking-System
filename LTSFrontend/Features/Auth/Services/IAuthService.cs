using LTSFrontend.Features.Auth.DTOs;

namespace LTSFrontend.Features.Auth.Services
{
    /// <summary>
    /// Client-side gateway to LTSBackend's AuthController.
    /// Every call goes through ApiClient, which unwraps ApiResponse&lt;T&gt;
    /// and throws ApiException on failure (Success = false / non-2xx).
    /// </summary>
    public interface IAuthService
    {
        Task<RegisterResponseDTO> RegisterAsync(RegisterRequest request);
        Task<VerifyOtpResponseDTO> VerifyOtpAsync(VerifyOtpRequest request);
        Task<ResendOtpResponseDTO> ResendOtpAsync(ResendOtpRequest request);
        Task<LoginResponseDTO> LoginAsync(LoginRequest request);

        /// <summary>
        /// Changes the password of the CURRENTLY authenticated user
        /// (POST /api/auth/change-password). Requires the user's current
        /// password as proof of ownership. Unlike ForgotPassword/ResetPassword,
        /// this never touches email/OTP - it's a same-session security action.
        /// </summary>
        Task<bool> ChangePasswordAsync(ChangePasswordRequest request);

        Task<ForgotPasswordResponseDTO> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ResetPasswordResponseDTO> ResetPasswordAsync(ResetPasswordRequest request);
        Task LogoutAsync();
    }
}
