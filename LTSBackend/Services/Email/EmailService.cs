using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace LTSBackend.Services.Email;

public class EmailService(IConfiguration _configuration, ILogger<EmailService> _logger) : IEmailService
{
    public async Task SendOtpEmailAsync(string toEmail, string fullName, string otpCode)
    {
        var textBody = $"Dear {fullName},\r\n\r\n" +
                        $"Your OTP code is: {otpCode}\r\n" +
                        $"This code will expire in 5 minutes.\r\n\r\n" +
                        $"If you didn't request this, please ignore this email.\r\n\r\n" +
                        $"Regards,\r\nLTS System";

        await SendAsync(toEmail, fullName, "LTS - Your OTP Code", textBody);
        _logger.LogInformation("OTP email sent successfully to {Email}", toEmail);
    }

    public async Task SendNotificationEmailAsync(string toEmail, string fullName, string subject, string message)
    {
        var textBody = $"Dear {fullName},\r\n\r\n" +
                        $"{message}\r\n\r\n" +
                        $"You can also view this notification by signing in to LTS.\r\n\r\n" +
                        $"Regards,\r\nLTS System";

        await SendAsync(toEmail, fullName, $"LTS - {subject}", textBody);
        _logger.LogInformation("Notification email sent successfully to {Email}", toEmail);
    }

    public async Task SendContactMessageEmailAsync(string fromName, string fromEmail, string? fromPhone, string message)
    {
        try
        {
            string smtpHost = _configuration["EmailSettings:SmtpHost"]!;
            int smtpPort = Convert.ToInt32(_configuration["EmailSettings:SmtpPort"]);
            string senderEmail = _configuration["EmailSettings:SenderEmail"]!;
            string senderName = _configuration["EmailSettings:SenderName"]!;
            string appPassword = _configuration["EmailSettings:AppPassword"]!;

            var textBody = $"New message from the LTS website contact form:\r\n\r\n" +
                           $"Name: {fromName}\r\n" +
                           $"Email: {fromEmail}\r\n" +
                           (string.IsNullOrWhiteSpace(fromPhone) ? "" : $"Phone: {fromPhone}\r\n") +
                           $"\r\nMessage:\r\n{message}\r\n\r\n" +
                           $"(Reply to this email to respond directly to {fromName}.)";

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(senderName, senderEmail));
            // Replying in the inbox goes straight to the visitor, not back to us.
            email.ReplyTo.Add(new MailboxAddress(fromName, fromEmail));
            // Delivered to the firm's own support inbox (the configured sender).
            email.To.Add(new MailboxAddress(senderName, senderEmail));
            email.Subject = $"LTS Website Contact - {fromName}";
            email.Body = new TextPart("plain") { Text = textBody };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(senderEmail, appPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Contact form email delivered to support inbox from {Email}", fromEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contact form email from {Email}", fromEmail);
            throw; // Re-throw - unlike the acknowledgement email, this one must reach a human.
        }
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string textBody)
    {
        try
        {
            string smtpHost = _configuration["EmailSettings:SmtpHost"]!;
            int smtpPort = Convert.ToInt32(_configuration["EmailSettings:SmtpPort"]);
            string senderEmail = _configuration["EmailSettings:SenderEmail"]!;
            string senderName = _configuration["EmailSettings:SenderName"]!;
            string appPassword = _configuration["EmailSettings:AppPassword"]!;

            _logger.LogInformation("SMTP Configuration - Host: {Host}, Port: {Port}, Sender: {Sender}", smtpHost, smtpPort, senderEmail);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(senderName, senderEmail));
            email.To.Add(new MailboxAddress(toName, toEmail));
            email.Subject = subject;
            email.Body = new TextPart("plain") { Text = textBody };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(senderEmail, appPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email ({Subject}) to {Email}", subject, toEmail);
            throw; // Re-throw to let caller handle
        }
    }
}