using MediatR;

namespace LTSBackend.Features.Courts.Commands.CreateCourt;

public sealed record CreateCourtCommand(string CourtName,string? CourtType,string? Jurisdiction,string? Address,bool IsActive = true ) : IRequest<int>;
