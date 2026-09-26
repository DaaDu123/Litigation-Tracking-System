using MediatR;

namespace LTSBackend.Features.FirmAdminRequests.Commands.SubmitFirmAdminRequestFromAccount;

// "Create Firm" from an already-registered, already-logged-in user's own
// dashboard (sidebar). Unlike SubmitFirmAdminRequestCommand (the
// anonymous/public flow, which collects a fresh Email + Password because
// the account doesn't exist yet), this one collects NOTHING - the
// requester already has an account, so their existing email/password are
// simply reused in place once a SuperAdmin approves (see
// ApproveFirmAdminRequestCommandHandler's UserID-linked branch). No
// re-registration with the same email is ever required.
public record SubmitFirmAdminRequestFromAccountCommand : IRequest<int>;
