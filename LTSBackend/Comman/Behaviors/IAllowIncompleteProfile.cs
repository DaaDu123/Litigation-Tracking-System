namespace LTSBackend.Comman.Behaviors;

/// <summary>
/// Marker interface for the small, explicit allowlist of MediatR
/// requests that a user with IsProfileCompleted == false is still allowed
/// to run (auth basics + the profile-completion commands/queries
/// themselves). Everything else is blocked by default by
/// ProfileCompletionBehavior - this is a deliberate allowlist, not a
/// denylist, so a new command added later is BLOCKED until someone
/// explicitly opts it in, which is the safe default for this gate.
/// </summary>
public interface IAllowIncompleteProfile
{
}
