using LTSFrontend.Core.Http;
using LTSFrontend.Features.Auth.DTOs;

namespace LTSFrontend.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApiClient _api;

        public AuthService(ApiClient api)
        {
            _api = api;
        }

        public async Task<RegisterResponseDTO> RegisterAsync(RegisterRequest request)
        {
            // ConfirmPassword is a client-only field (used for validation);
            // the backend RegisterCommand doesn't have it, so we don't send it.
            var payload = new
            {
                request.FullName,
                request.Email,
                request.Password,
                request.Phone,
                request.Department,
                request.FirmCode
            };

            var result = await _api.PostAsync<RegisterResponseDTO>(ApiEndpoints.Auth.Register, payload);
            return result!;
        }

        public async Task<VerifyOtpResponseDTO> VerifyOtpAsync(VerifyOtpRequest request)
        {
            var result = await _api.PostAsync<VerifyOtpResponseDTO>(ApiEndpoints.Auth.VerifyOtp, request);
            return result!;
        }

        public async Task<ResendOtpResponseDTO> ResendOtpAsync(ResendOtpRequest request)
        {
            var result = await _api.PostAsync<ResendOtpResponseDTO>(ApiEndpoints.Auth.ResendOtp, request);
            return result!;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequest request)
        {
            var result = await _api.PostAsync<LoginResponseDTO>(ApiEndpoints.Auth.Login, request);
            return result!;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            // ConfirmNewPassword is a client-only field used for the match
            // check in ChangePasswordRequest.Validate(); the backend
            // ChangePasswordCommand only has OldPassword/NewPassword (UserID
            // is populated server-side from the JWT, never sent by us).
            var payload = new
            {
                request.OldPassword,
                request.NewPassword
            };

            var result = await _api.PostAsync<bool>(ApiEndpoints.Auth.ChangePassword, payload);
            return result;
        }

        public async Task<ForgotPasswordResponseDTO> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var result = await _api.PostAsync<ForgotPasswordResponseDTO>(ApiEndpoints.Auth.ForgotPassword, request);
            return result!;
        }

        public async Task<ResetPasswordResponseDTO> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var payload = new
            {
                request.Email,
                request.OtpCode,
                request.NewPassword
            };

            var result = await _api.PostAsync<ResetPasswordResponseDTO>(ApiEndpoints.Auth.ResetPassword, payload);
            return result!;
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _api.PostAsync<bool>(ApiEndpoints.Auth.Logout);
            }
            catch
            {
                // Best-effort logout fallback
            }
        }
    }
}