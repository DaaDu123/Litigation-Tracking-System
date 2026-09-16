namespace LTSBackend.Services.Email
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string fullName, string otpCode);

        /// <summary>
        /// Sends an in-app Notification's content out as an email too, so a
        /// user gets the alert both on their dashboard AND in their inbox
        /// (e.g. deadline alerts, hearing reminders, case assignments).
        /// </summary>
        Task SendNotificationEmailAsync(string toEmail, string fullName, string subject, string message);

        /// <summary>
        /// Delivers a public "Contact Us" form submission to the firm's own
        /// support inbox (EmailSettings:SenderEmail), with Reply-To set to
        /// the visitor so replying in the inbox goes straight back to them.
        /// </summary>
        Task SendContactMessageEmailAsync(string fromName, string fromEmail, string? fromPhone, string message);
    }
}