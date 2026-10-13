using MediatR;

namespace LTSBackend.Features.ContactMessages.Commands.SubmitContactMessage;




public record SubmitContactMessageCommand(string Name, string Email, string? Phone, string Message) : IRequest<bool>;
