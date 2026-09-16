using MediatR;
namespace LTSBackend.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string FullName,
    string Email,
    string Password,
    string? Phone,
    string? Department,
    // Never sent by the frontend on create — there is no Role field in
    // the Add User form at all. Always defaults to UserRole.InternParalegal
    // in the handler. Only meaningful on UpdateUserCommand (Edit User),
    // where a Firm Admin changes an existing user's role.
    int? RoleID,
    IFormFile? ProfileImage
) : IRequest<int>
{
    public int ActingUserID { get; init; }   // ✅ set from controller via ClaimTypes.NameIdentifier
}