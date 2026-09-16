using MediatR;

namespace LTSBackend.Features.Firms.Queries.ExportFirmData;

/// <summary>
/// Roles SRS "Data Custody & Export": produces a downloadable snapshot
/// of everything belonging to one firm (users + cases + parties) as a
/// zip of CSV files, so the firm's data can be handed back to them if
/// they leave the platform.
/// </summary>
public record ExportFirmDataQuery(int FirmID) : IRequest<byte[]>;
