using System.Text.RegularExpressions;

namespace LTSBackend.Comman.Validation;









public static class PakistaniFormat
{

    private static readonly Regex LocalPattern = new(@"^03\d{9}$", RegexOptions.Compiled);
    private static readonly Regex IntlPattern = new(@"^923\d{9}$", RegexOptions.Compiled);
    private static readonly Regex CnicPattern = new(@"^\d{13}$", RegexOptions.Compiled);








    public static string? NormalizePhone(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;


        var trimmed = raw.Trim();
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());

        if (LocalPattern.IsMatch(digits))
        {

            return "+92" + digits[1..];
        }

        if (IntlPattern.IsMatch(digits))
        {

            return "+" + digits;
        }


        if (digits.Length == 12 && digits.StartsWith("92") && IntlPattern.IsMatch(digits))
            return "+" + digits;




        return trimmed;
    }


    public static bool IsValidPhone(string? raw)
    {
        var normalized = NormalizePhone(raw);
        return normalized != null && Regex.IsMatch(normalized, @"^\+923\d{9}$");
    }







    public static string? NormalizeCnic(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        return digits;
    }


    public static bool IsValidCnic(string? raw)
    {
        var normalized = NormalizeCnic(raw);
        return normalized != null && CnicPattern.IsMatch(normalized);
    }


    public static string? FormatCnic(string? normalized)
    {
        if (string.IsNullOrWhiteSpace(normalized) || !CnicPattern.IsMatch(normalized))
            return normalized;

        return $"{normalized[..5]}-{normalized[5..12]}-{normalized[12..]}";
    }
}
