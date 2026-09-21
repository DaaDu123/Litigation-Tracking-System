using System.Text.RegularExpressions;

namespace LTSBackend.Comman.Validation;

/// <summary>
/// Single source of truth for Pakistani mobile-number and CNIC
/// normalization/validation. Every write path (registration, profile
/// completion, profile update) and every uniqueness check MUST go through
/// NormalizePhone/NormalizeCnic before touching the database, so that
/// "03001234567" and "+923001234567", or "35202-1234567-1" and
/// "3520212345671", are always treated as the same value.
/// </summary>
public static class PakistaniFormat
{
    // 03XXXXXXXXX (11 digits, starts 03) or +923XXXXXXXXX / 923XXXXXXXXX.
    private static readonly Regex LocalPattern = new(@"^03\d{9}$", RegexOptions.Compiled);
    private static readonly Regex IntlPattern = new(@"^923\d{9}$", RegexOptions.Compiled);
    private static readonly Regex CnicPattern = new(@"^\d{13}$", RegexOptions.Compiled);

    /// <summary>
    /// Normalizes any accepted Pakistani mobile format to the canonical
    /// "+923XXXXXXXXX" form. Returns null if the input is null/blank, or
    /// the original trimmed input (unchanged) if it doesn't match any
    /// recognized pattern - callers should treat that as invalid via
    /// <see cref="IsValidPhone"/> before persisting, not silently accept it.
    /// </summary>
    public static string? NormalizePhone(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        // Strip everything except digits and a leading '+'.
        var trimmed = raw.Trim();
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());

        if (LocalPattern.IsMatch(digits))
        {
            // 03XXXXXXXXX -> +923XXXXXXXXX
            return "+92" + digits[1..];
        }

        if (IntlPattern.IsMatch(digits))
        {
            // 923XXXXXXXXX -> +923XXXXXXXXX
            return "+" + digits;
        }

        // Already canonical +923XXXXXXXXX (12 digits after the '+')
        if (digits.Length == 12 && digits.StartsWith("92") && IntlPattern.IsMatch(digits))
            return "+" + digits;

        // Doesn't match a known Pakistani pattern - return as-is so the
        // caller's IsValidPhone check (run separately) fails loudly
        // instead of silently normalizing garbage into the database.
        return trimmed;
    }

    /// <summary>True only for a value that NormalizePhone would turn into a canonical +923XXXXXXXXX number.</summary>
    public static bool IsValidPhone(string? raw)
    {
        var normalized = NormalizePhone(raw);
        return normalized != null && Regex.IsMatch(normalized, @"^\+923\d{9}$");
    }

    /// <summary>
    /// Normalizes a CNIC (with or without dashes) to 13 raw digits.
    /// Returns null if input is null/blank, or the trimmed input unchanged
    /// if it isn't 13 digits once dashes/spaces are stripped - callers
    /// must validate with <see cref="IsValidCnic"/> before persisting.
    /// </summary>
    public static string? NormalizeCnic(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        return digits;
    }

    /// <summary>True only for a value that normalizes to exactly 13 digits.</summary>
    public static bool IsValidCnic(string? raw)
    {
        var normalized = NormalizeCnic(raw);
        return normalized != null && CnicPattern.IsMatch(normalized);
    }

    /// <summary>Formats a normalized 13-digit CNIC as "XXXXX-XXXXXXX-X" for display. Returns the input unchanged if it isn't 13 digits.</summary>
    public static string? FormatCnic(string? normalized)
    {
        if (string.IsNullOrWhiteSpace(normalized) || !CnicPattern.IsMatch(normalized))
            return normalized;

        return $"{normalized[..5]}-{normalized[5..12]}-{normalized[12..]}";
    }
}
