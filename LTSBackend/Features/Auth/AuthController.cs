using LTSBackend.Comman.Responses;
using LTSBackend.Features.Auth.ChangePassword;
using LTSBackend.Features.Auth.ForgotPassword;
using LTSBackend.Features.Auth.Login;
using LTSBackend.Features.Auth.Logout;
using LTSBackend.Features.Auth.RefreshToken;
using LTSBackend.Features.Auth.Register;
using LTSBackend.Features.Auth.ResendOtp;
using LTSBackend.Features.Auth.ResetPassword;
using LTSBackend.Features.Auth.VerifyOtp;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace LTSBackend.Features.Auth;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }









    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-moderate")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", command.Email);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<RegisterResponseDTO>.SuccessResponse(result, result.Message));
    }










    [HttpPost("verify-otp")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-critical")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
    {
        _logger.LogInformation("OTP verification attempt for email: {Email}", command.Email);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<VerifyOtpResponseDTO>.SuccessResponse(result, result.Message));
    }








    [HttpPost("resend-otp")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-moderate")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpCommand command)
    {
        _logger.LogInformation("Resend OTP request for email: {Email}", command.Email);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<ResendOtpResponseDTO>.SuccessResponse(result, result.Message));
    }










    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-critical")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        _logger.LogInformation("Login attempt for email: {Email}", command.Email);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<LoginResponseDTO>.SuccessResponse(result, "Login successful!"));
    }






    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("Logout request");
        var result = await _mediator.Send(new LogoutCommand());
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Logout successful!"));
    }












    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        _logger.LogInformation("Token refresh request");
        var result = await _mediator.Send(new RefreshTokenCommand());
        return Ok(ApiResponse<RefreshTokenResponseDTO>.SuccessResponse(
            result,
            "Access token refreshed successfully."));
    }








    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        _logger.LogInformation("Change password request");

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<bool>.FailureResponse(
                "Invalid or missing user identity."));
        }

        var commandWithUserId = command with { UserID = userId };
        var result = await _mediator.Send(commandWithUserId);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Password changed successfully!"));
    }











    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-moderate")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        _logger.LogInformation("Forgot password request for email: {Email}", command.Email);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<ForgotPasswordResponseDTO>.SuccessResponse(result, result.Message));
    }









    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-critical")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        _logger.LogInformation("Password reset request received (via OTP) for email: {Email}", command.Email);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<ResetPasswordResponseDTO>.SuccessResponse(result, result.Message));
    }
}
