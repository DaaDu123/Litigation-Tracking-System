using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LTSFrontend.Shared.Validation
{
    /// <summary>
    /// Client-side mirror of LTSBackend.Comman.Validation.PakistaniFormat.IsValidPhone -
    /// this is a UX convenience only. The backend re-validates and
    /// normalizes independently and is the authoritative check.
    /// </summary>
    public class PakistaniPhoneAttribute : ValidationAttribute
    {
        public bool AllowEmpty { get; set; }

        public PakistaniPhoneAttribute()
        {
            ErrorMessage = "Enter a valid Pakistani mobile number (e.g. 03001234567 or +923001234567).";
        }

        public override bool IsValid(object? value)
        {
            var raw = value as string;

            if (string.IsNullOrWhiteSpace(raw))
                return AllowEmpty;

            var digits = new string(raw.Where(char.IsDigit).ToArray());

            // 03XXXXXXXXX (11 digits) or 923XXXXXXXXX / +923XXXXXXXXX (12 digits after country code)
            return Regex.IsMatch(digits, "^03\\d{9}$") || Regex.IsMatch(digits, "^923\\d{9}$");
        }
    }
}
