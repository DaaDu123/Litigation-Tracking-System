using MediatR;
namespace LTSBackend.Features.Profile.Commands;

public record UpdateMyProfileCommand(string FullName, string? Phone, string? Department, IFormFile? ProfileImage) : IRequest<bool>
{
    public int UserID { get; init; }
}