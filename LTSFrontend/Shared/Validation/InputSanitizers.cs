using System.Linq;

namespace LTSFrontend.Shared.Validation
{
    /// <summary>
    /// Keystroke-level sanitizers for phone/CNIC inputs, wired up via
    /// @bind:get/@bind:set (or @bind-Value:get/@bind-Value:set) on the
    /// relevant &lt;input&gt;/&lt;InputText&gt; elements. These strip anything
    /// that isn't a digit (letters, symbols, spaces, pasted junk, etc.) as
    /// the user types, so invalid characters never make it into the field.
    /// This is a UX guard only - PakistaniPhoneAttribute / PakistaniCnicAttribute
    /// and the backend remain the authoritative validation.
    /// </summary>
    public static class InputSanitizers
    {
        /// <summary>
        /// Keeps only digits, plus a single leading '+' when the user is
        /// entering a country code (e.g. "+923001234567"). Caps at 12
        /// digits (923XXXXXXXXX) so a runaway paste can't blow up the field.
        /// </summary>
        public static string SanitizePhone(string? raw)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            var hasLeadingPlus = raw.TrimStart().StartsWith("+");
            var digits = new string(raw.Where(char.IsDigit).ToArray());

            const int maxDigits = 12; // e.g. 92 300 1234567
            if (digits.Length > maxDigits)
                digits = digits[..maxDigits];

            return hasLeadingPlus ? "+" + digits : digits;
        }

        /// <summary>
        /// Keeps only digits (up to the 13 that make up a CNIC) and
        /// auto-inserts the standard 5-7-1 dashes as the user types, so the
        /// field always reads like 35202-1234567-1 even though only digit
        /// keystrokes are ever accepted.
        /// </summary>
        public static string SanitizeCnic(string? raw)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            var digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.Length > 13)
                digits = digits[..13];

            return digits.Length switch
            {
                <= 5 => digits,
                <= 12 => $"{digits[..5]}-{digits[5..]}",
                _ => $"{digits[..5]}-{digits[5..12]}-{digits[12..]}"
            };
        }
    }
}
