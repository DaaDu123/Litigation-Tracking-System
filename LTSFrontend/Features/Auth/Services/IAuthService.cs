using LTSFrontend.Features.Auth.DTOs;

namespace LTSFrontend.Features.Auth.Services
{





    public interface IAuthService
    {
        Task<RegisterResponseDTO> RegisterAsync(RegisterRequest request);
        Task<VerifyOtpResponseDTO> VerifyOtpAsync(VerifyOtpRequest request);
        Task<ResendOtpResponseDTO> ResendOtpAsync(ResendOtpRequest request);
        Task<LoginResponseDTO> LoginAsync(LoginRequest request);







        Task<bool> ChangePasswordAsync(ChangePasswordRequest request);

        Task<ForgotPasswordResponseDTO> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ResetPasswordResponseDTO> ResetPasswordAsync(ResetPasswordRequest request);
        Task LogoutAsync();
    }
}
