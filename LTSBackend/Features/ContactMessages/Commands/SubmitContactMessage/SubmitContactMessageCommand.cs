using MediatR;

namespace LTSBackend.Features.ContactMessages.Commands.SubmitContactMessage;

// Anonymous "Contact Us" submission from the public website. Not persisted
// to the database — it's delivered straight to the firm's support inbox by
// email, the same way OTP / notification emails already go out.
public record SubmitContactMessageCommand(string Name, string Email, string? Phone, string Message) : IRequest<bool>;
