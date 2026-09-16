using MediatR;

namespace LTSBackend.Features.Courts.Commands.DeleteCourt;

public sealed record DeleteCourtCommand(int CourtID) : IRequest<bool>;
