namespace LTSFrontend.Features.Masters.DTOs
{
    /// <summary>
    /// Common court-type suggestions shown in the Court form's datalist.
    /// CourtType is a free-text field on the backend, so this only powers
    /// autocomplete/consistency - it is never enforced client or server side.
    /// </summary>
    public static class CourtTypeOptions
    {
        public static readonly string[] Suggestions =
        {
            "Supreme Court",
            "High Court",
            "District & Sessions Court",
            "Civil Court",
            "Family Court",
            "Banking Court",
            "Labour Court",
            "Anti-Terrorism Court",
            "Consumer Court",
            "Tax Tribunal",
            "Arbitration Tribunal",
            "Administrative Tribunal"
        };
    }
}
