using LTSBackend.Services.Email;
using MediatR;

namespace LTSBackend.Features.ContactMessages.Commands.SubmitContactMessage;

public class SubmitContactMessageCommandHandler(IEmailService _emailService, ILogger<SubmitContactMessageCommandHandler> _logger) : IRequestHandler<SubmitContactMessageCommand, bool>
{
    // =====================================================
    // HANDLE — anonymous "Contact Us" submission from the public website.
    // 1) Delivers the message to the firm's support inbox over SMTP (Gmail,
    //    same relay used for OTP/notification emails) with Reply-To set to
    //    the visitor, so replying in the inbox goes straight back to them.
    // 2) Best-effort acknowledgement email back to the visitor confirming
    //    receipt - failure here does not fail the whole request, since the
    //    important part (step 1) already succeeded.
    // =====================================================
    public async Task<bool> Handle(SubmitContactMessageCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var email = request.Email.Trim();
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        var message = request.Message.Trim();

        await _emailService.SendContactMessageEmailAsync(name, email, phone, message);

        _logger.LogInformation("Contact form message received from {Email}", email);

        try
        {
            await _emailService.SendNotificationEmailAsync(email,name,"We've received your message","Thanks for reaching out to the Litigation Tracking System team. " + "We've received your message and will get back to you shortly.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send contact-form acknowledgement email to {Email}", email);
        }

        return true;
    }
}
