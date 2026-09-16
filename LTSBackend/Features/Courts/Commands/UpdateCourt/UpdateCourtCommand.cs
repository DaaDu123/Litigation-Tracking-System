using MediatR;

namespace LTSBackend.Features.Courts.Commands.UpdateCourt;

public sealed record UpdateCourtCommand(int CourtID, string CourtName, string? CourtType, string? Jurisdiction, string? Address, bool IsActive = true) : IRequest<bool>;
