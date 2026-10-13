using MediatR;

namespace LTSBackend.Features.Firms.Queries.ExportFirmData;







public record ExportFirmDataQuery(int FirmID) : IRequest<byte[]>;
