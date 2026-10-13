using System.ComponentModel.DataAnnotations;

namespace LTSFrontend.Shared.Validation
{





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
