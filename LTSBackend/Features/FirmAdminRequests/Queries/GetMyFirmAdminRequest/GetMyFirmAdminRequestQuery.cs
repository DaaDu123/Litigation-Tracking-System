using LTSBackend.Features.FirmAdminRequests.DTOs;
using MediatR;

namespace LTSBackend.Features.FirmAdminRequests.Queries.GetMyFirmAdminRequest;

// The caller's own latest Firm Admin request (any status), submitted from
// their own account (see SubmitFirmAdminRequestFromAccountCommand), for
// the "Create Firm" modal's pending/rejected state. Returns null if
// they've never submitted one this way. Mirrors GetMyJoinRequestQuery one
// tier up.
public record GetMyFirmAdminRequestQuery : IRequest<FirmAdminRequestDTO?>;
