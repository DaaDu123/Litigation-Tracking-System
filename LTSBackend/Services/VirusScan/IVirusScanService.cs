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








public interface IVirusScanService
{
    Task<VirusScanResult> ScanAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default);
}
