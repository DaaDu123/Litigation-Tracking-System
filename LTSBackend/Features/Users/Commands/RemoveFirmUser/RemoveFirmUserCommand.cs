using MediatR;

namespace LTSBackend.Features.Users.Commands.RemoveFirmUser;

public record RemoveFirmUserCommand(int UserID, string Reason) : IRequest<bool>;
