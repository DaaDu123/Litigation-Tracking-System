using LTSBackend.Comman.Responses;
using LTSBackend.Features.Auth.ChangePassword;
using LTSBackend.Features.Auth.ForgotPassword;
using LTSBackend.Features.Auth.Login;
using LTSBackend.Features.Auth.Logout;
using LTSBackend.Features.Auth.RefreshToken;
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

    // =====================================================
    // VERIFY OTP — Anonymous
    // Confirms the 6-digit one-time code sent to the user's email during
    // registration (or another OTP-based flow) and marks the account as
    // verified so it can log in.
    // SECURITY: rate limited ("auth-critical", see Program.cs) — this is
    // the endpoint that brute-forces a 6-digit OTP; without a limit an
    // attacker gets unlimited guesses inside the 5-minute expiry window.
    // =====================================================
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-critical")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
    {
        _logger.LogInformation("OTP verification attempt for email: {Email}", command.Email);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<VerifyOtpResponseDTO>.SuccessResponse(result, result.Message));
    }

    // =====================================================
    // RESEND OTP — Anonymous
    // Issues a fresh OTP (e.g. because the previous one expired or the
    // email never arrived) for the same verification flow as Register.
    // SECURITY: rate limited ("auth-moderate", see Program.cs) — prevents
    // this endpoint being used as an email-bombing vector.
    // =====================================================
    [HttpPost("resend-otp")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-moderate")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpCommand command)
    {
        _logger.LogInformation("Resend OTP request for email: {Email}", command.Email);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<ResendOtpResponseDTO>.SuccessResponse(result, result.Message));
    }

    // =====================================================
    // LOGIN — Anonymous
    // Validates email/password and, on success, issues the access token
    // (plus sets the refresh-token cookie) that every other authenticated
    // endpoint relies on.
    // SECURITY: rate limited ("auth-critical", see Program.cs) — the
    // per-account lockout in LoginHandler doesn't stop an attacker trying
    // many different email addresses from one IP; this closes that gap.
    // =====================================================
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-critical")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        _logger.LogInformation("Login attempt for email: {Email}", command.Email);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<LoginResponseDTO>.SuccessResponse(result, "Login successful!"));
    }

    // =====================================================
    // LOGOUT — Any authenticated user
    // Invalidates the caller's current refresh token/session so the
    // access/refresh token pair can no longer be used to get new tokens.
    // =====================================================
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("Logout request");
        var result = await _mediator.Send(new LogoutCommand());
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Logout successful!"));
    }

    // =====================================================
    // REFRESH TOKEN — Anonymous (relies on the refresh-token cookie)
    // Exchanges a still-valid refresh token (sent as an HttpOnly cookie,
    // not in the body) for a new short-lived access token, so the user
    // stays logged in without re-entering credentials.
    // SECURITY: rate limited ("auth-critical", see Program.cs) — caps how
    // fast a stolen/guessed refresh token cookie can be replayed.
    // =====================================================
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-critical")]
    public async Task<IActionResult> RefreshToken()
    {
        _logger.LogInformation("Token refresh request");
        var result = await _mediator.Send(new RefreshTokenCommand());
        return Ok(ApiResponse<RefreshTokenResponseDTO>.SuccessResponse(
            result,
            "Access token refreshed successfully."));
    }

    // =====================================================
    // CHANGE PASSWORD — Any authenticated user
    // Lets an already-logged-in user change their own password by
    // supplying their current password plus a new one. The acting user's
    // ID always comes from their own JWT claim, never from the request
    // body, so a user can only ever change their own password here.
    // =====================================================
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

    // =====================================================
    // FORGOT PASSWORD — Anonymous
    // Starts the "I forgot my password" flow: if the email exists, an OTP
    // is emailed to it. Always returns the same generic response whether
    // or not the email is registered (see ForgotPasswordHandler) so the
    // endpoint can't be used to check which emails have accounts. Also
    // doubles as the "resend OTP" call for this specific flow.
    // SECURITY: rate limited ("auth-moderate", see Program.cs) — without a
    // rate limit an attacker could still email-bomb a target address.
    // =====================================================
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-moderate")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        _logger.LogInformation("Forgot password request for email: {Email}", command.Email);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<ForgotPasswordResponseDTO>.SuccessResponse(result, result.Message));
    }

    // =====================================================
    // RESET PASSWORD — Anonymous
    // Completes the forgot-password flow: verifies the OTP sent by
    // ForgotPassword and, if valid, sets the new password.
    // SECURITY: rate limited ("auth-critical", see Program.cs) — same
    // brute-force concern as VerifyOtp: a 6-digit code is checked here, so
    // unlimited guesses inside the 5-minute expiry must be blocked.
    // =====================================================
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
