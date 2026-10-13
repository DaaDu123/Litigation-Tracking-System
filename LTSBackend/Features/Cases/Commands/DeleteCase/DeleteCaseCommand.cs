using MediatR;

namespace LTSBackend.Features.Cases.Commands.DeleteCase;





public record DeleteCaseCommand(long CaseID, string ConfirmCaseNumber, string Reason) : IRequest<bool>;
