namespace LTSBackend.Services.Email
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string fullName, string otpCode);






        Task SendNotificationEmailAsync(string toEmail, string fullName, string subject, string message);






        Task SendContactMessageEmailAsync(string fromName, string fromEmail, string? fromPhone, string message);
    }
}