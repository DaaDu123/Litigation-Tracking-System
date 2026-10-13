namespace LTSFrontend.Core.Enums
{





    public enum OtpPurpose
    {

        Registration = 1,


        PasswordReset = 2
    }


    public static class OtpPurposeExtensions
    {

        public static string ToDisplayText(this OtpPurpose purpose) => purpose switch
        {
            OtpPurpose.Registration => "Verify your account",
            OtpPurpose.PasswordReset => "Verify password reset",
            _ => "Verify code"
        };
    }
}
