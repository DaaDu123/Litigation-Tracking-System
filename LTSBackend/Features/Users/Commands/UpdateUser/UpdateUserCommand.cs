using MediatR;
namespace LTSBackend.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    int UserID,
    string FullName,
    string Email,
    string? Phone,
    string? Department,
    int? RoleID,
    bool IsActive,
    IFormFile? ProfileImage
) : IRequest<bool>
{
    public int ActingUserID { get; init; }
}