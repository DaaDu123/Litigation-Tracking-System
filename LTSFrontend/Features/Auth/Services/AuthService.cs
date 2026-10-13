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


            var payload = new
            {
                request.Email,
                request.Password
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

            }
        }
    }
}