using LTSBackend.Services.Email;
using MediatR;

namespace LTSBackend.Features.ContactMessages.Commands.SubmitContactMessage;

public class SubmitContactMessageCommandHandler(IEmailService _emailService, ILogger<SubmitContactMessageCommandHandler> _logger) : IRequestHandler<SubmitContactMessageCommand, bool>
{









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
