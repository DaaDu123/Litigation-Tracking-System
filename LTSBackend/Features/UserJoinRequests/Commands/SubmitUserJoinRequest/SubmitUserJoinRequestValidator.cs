using FluentValidation;
using LTSBackend.Comman.Enum;

namespace LTSBackend.Features.UserJoinRequests.Commands.SubmitUserJoinRequest;

public class SubmitUserJoinRequestValidator : AbstractValidator<SubmitUserJoinRequestCommand>
{
    // Same shape as CreateUserCommandValidator (name/email/password
    // complexity/phone/department), plus a required FirmID (the firm
    // the requester picked from the public firm list) and a
    // RequestedRoleID restricted to the four joinable, firm-level roles -
    // FirmAdmin and SuperAdmin are never requestable through this form.
    public SubmitUserJoinRequestValidator()
    {
        RuleFor(x => x.FirmID)
            .GreaterThan(0)
            .WithMessage("Please select the firm you want to join.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[!@#$%^&*(),.?"":{}|<>_\-+=\[\]\\/;'~`]").WithMessage("Password must contain at least one symbol (!@#$%^&* etc.).");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.")
            .Matches(@"^\+?[0-9\-\(\)\s]*$").WithMessage("Phone format is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.RequestedRoleID)
            .Must(RoleHierarchy.IsJoinableRole)
            .WithMessage("Invalid role. You may request Partner, Associate Lawyer, Moharrir, or Intern/Paralegal only - " +
                         "Firm Admin access has its own request form, and Super Admin cannot be requested.");
    }
}
