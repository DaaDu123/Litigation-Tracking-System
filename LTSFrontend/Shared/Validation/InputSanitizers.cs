using System.Linq;

namespace LTSFrontend.Shared.Validation
{









    public static class InputSanitizers
    {





        public static string SanitizePhone(string? raw)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            var hasLeadingPlus = raw.TrimStart().StartsWith("+");
            var digits = new string(raw.Where(char.IsDigit).ToArray());

            const int maxDigits = 12;
            if (digits.Length > maxDigits)
                digits = digits[..maxDigits];

            return hasLeadingPlus ? "+" + digits : digits;
        }







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
