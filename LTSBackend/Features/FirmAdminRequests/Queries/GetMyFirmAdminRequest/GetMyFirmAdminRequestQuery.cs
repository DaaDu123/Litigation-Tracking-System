using LTSBackend.Features.FirmAdminRequests.DTOs;
using MediatR;

namespace LTSBackend.Features.FirmAdminRequests.Queries.GetMyFirmAdminRequest;






public record GetMyFirmAdminRequestQuery : IRequest<FirmAdminRequestDTO?>;
