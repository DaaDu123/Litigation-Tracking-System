using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Shared.Validation
{
    /// <summary>
    /// Client-side mirror of LTSBackend.Comman.Validation.PakistaniFormat.IsValidCnic -
    /// this is a UX convenience only. The backend re-validates and
    /// normalizes independently and is the authoritative check.
    /// </summary>
    public class PakistaniCnicAttribute : ValidationAttribute
    {
        public PakistaniCnicAttribute()
        {
            ErrorMessage = "Enter a valid CNIC (format: 35202-1234567-1).";
        }

        public override bool IsValid(object? value)
        {
            var raw = value as string;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            var digits = new string(raw.Where(char.IsDigit).ToArray());
            return digits.Length == 13;
        }
    }
}
