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
    // in the handler. A role can only ever be changed afterwards via
    // ChangeUserRoleCommand (PUT /api/users/{id}/role) — there is no
    // general "edit user" endpoint that could also change name/contact
    // info/photo alongside it (see UsersController for why).
    int? RoleID,
    IFormFile? ProfileImage
) : IRequest<int>
{
    public int ActingUserID { get; init; }   // ✅ set from controller via ClaimTypes.NameIdentifier
}