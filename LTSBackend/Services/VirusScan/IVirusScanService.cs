namespace LTSBackend.Services.VirusScan;

public record VirusScanResult(bool IsClean, string? ThreatName, string? Error)
{
    public static VirusScanResult Clean()
    {
        return new(true, null, null);
    }

    public static VirusScanResult Infected(string threatName)
    {
        return new(false, threatName, null);
    }

    public static VirusScanResult ScanFailed(string error)
    {
        return new(false, null, error);
    }
}

/// <summary>
/// Scans uploaded file bytes for malware before they ever touch disk.
/// Every real upload path in this codebase (profile pictures, case
/// documents - see FileService.SaveFileInternalAsync) routes through
/// this, so there is exactly one place to reason about "did we scan
/// this file" rather than one check per feature.
/// </summary>
public interface IVirusScanService
{
    Task<VirusScanResult> ScanAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default);
}
