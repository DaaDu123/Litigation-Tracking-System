using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LTSFrontend.Shared.Validation
{





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


            return Regex.IsMatch(digits, "^03\\d{9}$") || Regex.IsMatch(digits, "^923\\d{9}$");
        }
    }
}
