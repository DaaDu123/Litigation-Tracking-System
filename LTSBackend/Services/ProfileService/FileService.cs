using LTSBackend.Comman.Exceptions;
using LTSBackend.Services.VirusScan;
using Microsoft.Extensions.Configuration;

namespace LTSBackend.Services.ProfileService;

public class FileService(IWebHostEnvironment _environment, IVirusScanService _virusScanService, IConfiguration _configuration, ILogger<FileService> _logger) : IFileService
{
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".msi", ".bat", ".cmd", ".sh", ".ps1", ".psm1",
        ".php", ".php3", ".php4", ".php5", ".phtml",
        ".js", ".mjs", ".vbs", ".jar", ".jse", ".wsf", ".wsh",
        ".html", ".htm", ".svg", ".swf", ".scr", ".com", ".cpl", ".apk"
    };



    private string GetPublicRoot()
    {
        var configuredRoot = _configuration["FileStorage:PublicRoot"];
        var root = string.IsNullOrWhiteSpace(configuredRoot)
            ? (_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"))
            : configuredRoot;
        return Path.GetFullPath(root);
    }

    public Task<string> SaveFileAsync(IFormFile file, string folderName)
    {
        string publicRoot = GetPublicRoot();
        return SaveFileInternalAsync(file, folderName, publicRoot, isPublic: true);
    }


    public void DeleteFile(string? relativePath)
    {
        string publicRoot = GetPublicRoot();
        DeleteFileInternal(relativePath, publicRoot, isPublic: true);
    }


    public async Task<byte[]> ReadSecureFileAsync(string relativePath)
    {
        string fullPath = ResolveSecurePath(relativePath);

        if (!File.Exists(fullPath))
        {
            _logger.LogError("Secure file not found on disk: {FullPath}", fullPath);
            throw new FileNotFoundException("File not found on server.", relativePath);
        }

        return await File.ReadAllBytesAsync(fullPath);
    }


    public void DeleteSecureFile(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            return;
        }

        try
        {
            string fullPath = ResolveSecurePath(relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Secure file deleted successfully: {FilePath}", relativePath);
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to delete secure file: {FilePath}", relativePath);
        }
    }


    private async Task<string> SaveFileInternalAsync(IFormFile file, string folderName, string root, bool isPublic)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Attempted to save null or empty file");
            return string.Empty;
        }

        string ext = Path.GetExtension(file.FileName);

        if (BlockedExtensions.Contains(ext))
        {
            _logger.LogWarning("Rejected upload with disallowed extension: {Extension} (original name: {FileName})", ext, file.FileName);
            throw new InvalidOperationException($"File type '{ext}' is not permitted for upload.");
        }

        using (var scanStream = file.OpenReadStream())
        {
            var scanResult = await _virusScanService.ScanAsync(scanStream, file.FileName);

            if (!scanResult.IsClean)
            {
                if (scanResult.ThreatName != null)
                {
                    _logger.LogWarning("Upload rejected - malware detected: {FileName} ({Threat})", file.FileName, scanResult.ThreatName);
                    throw new ValidationException([$"This file was rejected because it appears to contain malware ({scanResult.ThreatName}). Please scan it locally and try a clean copy."]);
                }

                bool failClosed = _configuration.GetValue("VirusScan:FailClosed", true);
                if (failClosed)
                {
                    _logger.LogError("Upload rejected - virus scan could not be completed for {FileName}: {Error}", file.FileName, scanResult.Error);
                    throw new ValidationException(["File upload is temporarily unavailable (virus scanner unreachable). Please try again shortly or contact your administrator."]);
                }

                _logger.LogWarning("Proceeding with upload of {FileName} DESPITE a failed virus scan - VirusScan:FailClosed=false. Error: {Error}", file.FileName, scanResult.Error);
            }
        }

        string uploadsFolder = isPublic
            ? Path.Combine(root, "uploads", folderName)
            : Path.Combine(root, folderName);

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
            _logger.LogInformation("Created uploads folder: {Folder}", uploadsFolder);
        }

        string uniqueFileName = $"{Guid.NewGuid()}{ext}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            _logger.LogInformation("File saved successfully: {FileName} (secure={IsSecure})", uniqueFileName, !isPublic);

            return isPublic ? $"/uploads/{folderName}/{uniqueFileName}" : $"{folderName}/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file: {FileName}", file.FileName);
            throw;
        }
    }


    private void DeleteFileInternal(string? relativePath, string root, bool isPublic)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            return;
        }

        try
        {
            string fullPath = Path.Combine(root, relativePath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully: {FilePath}", relativePath);
            }
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to delete file: {FilePath}", relativePath);
        }
    }











    private static string BuildCaseDocumentFolder(int firmId, long caseId)
        => $"Firm/{firmId}/Case/{caseId}/Documents";

    public Task<string> SaveCaseDocumentAsync(IFormFile file, int firmId, long caseId)
    {
        string secureRoot = Path.Combine(_environment.ContentRootPath, "SecureStorage");
        string folder = BuildCaseDocumentFolder(firmId, caseId);
        return SaveFileInternalAsync(file, folder, secureRoot, isPublic: false);
    }

    public async Task<byte[]> ReadCaseDocumentAsync(string relativePath, int firmId, long caseId)
    {
        ValidateCaseDocumentOwnership(relativePath, firmId, caseId);
        return await ReadSecureFileAsync(relativePath);
    }

    public void DeleteCaseDocument(string? relativePath, int firmId, long caseId)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            return;
        }

        ValidateCaseDocumentOwnership(relativePath, firmId, caseId);
        DeleteSecureFile(relativePath);
    }














    private void ValidateCaseDocumentOwnership(string relativePath, int firmId, long caseId)
    {
        string expectedFolder = BuildCaseDocumentFolder(firmId, caseId);
        string normalizedPath = relativePath.Replace('\\', '/').TrimStart('/');

        if (normalizedPath.StartsWith(expectedFolder + "/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!normalizedPath.StartsWith("Firm/", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "Legacy (pre-isolation) document path served without Firm/Case validation: {RelativePath}", relativePath);
            return;
        }

        _logger.LogWarning(
            "Blocked cross-tenant/cross-case document access attempt: path {RelativePath} does not belong to Firm {FirmId} / Case {CaseId}",
            relativePath, firmId, caseId);
        throw new UnauthorizedAccessException("This document does not belong to the specified firm or case.");
    }


    private string ResolveSecurePath(string relativePath)
    {
        string secureRoot = Path.Combine(_environment.ContentRootPath, "SecureStorage");
        string fullPath = Path.GetFullPath(Path.Combine(secureRoot, relativePath.TrimStart('/', '\\')));
        string normalizedRoot = Path.GetFullPath(secureRoot);

        if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Rejected secure file path outside SecureStorage root: {RelativePath}", relativePath);
            throw new UnauthorizedAccessException("Invalid file path.");
        }

        return fullPath;
    }
}
