namespace LTSFrontend.Core.Enums
{
    /// <summary>
    /// Mirrors LTSBackend.Comman.Enum.OtpPurpose - identifies why an OTP was
    /// issued, so a single "verify OTP" screen/flow can be reused for more
    /// than one purpose while still showing the person the right context.
    /// </summary>
    public enum OtpPurpose
    {
        /// <summary>OTP issued to confirm a brand-new account's email during Register.</summary>
        Registration = 1,

        /// <summary>OTP issued as part of the Forgot Password flow.</summary>
        PasswordReset = 2
    }

    /// <summary>Display helpers for <see cref="OtpPurpose"/>.</summary>
    public static class OtpPurposeExtensions
    {
        /// <summary>Human-readable label shown on the Verify OTP page.</summary>
        public static string ToDisplayText(this OtpPurpose purpose) => purpose switch
        {
            OtpPurpose.Registration => "Verify your account",
            OtpPurpose.PasswordReset => "Verify password reset",
            _ => "Verify code"
        };
    }
}
