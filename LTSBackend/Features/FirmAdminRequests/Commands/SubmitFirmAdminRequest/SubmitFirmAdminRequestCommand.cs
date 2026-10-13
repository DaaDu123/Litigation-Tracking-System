using MediatR;
using LTSBackend.Comman.Behaviors;

namespace LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequest;





public record SubmitFirmAdminRequestCommand(string Email, string Password) : IRequest<int>, IAllowIncompleteProfile;
