using System.IO.Compression;
using System.Text;
using LTSBackend.Comman.Exceptions;
using LTSBackend.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Features.Firms.Queries.ExportFirmData;

public class ExportFirmDataQueryHandler(AppDbContext _context, ILogger<ExportFirmDataQueryHandler> _logger) : IRequestHandler<ExportFirmDataQuery, byte[]>
{
    // =====================================================
    // HANDLE — builds a downloadable zip of a firm's data (users/cases/parties CSVs)
    // Used as part of the firm-offboarding flow: export the firm's data
    // before blocking/removing its workspace.
    // =====================================================
    public async Task<byte[]> Handle(ExportFirmDataQuery request, CancellationToken cancellationToken)
    {
        var firm = await _context.Firms.AsNoTracking().FirstOrDefaultAsync(f => f.FirmID == request.FirmID, cancellationToken);
        if (firm == null)
            throw new NotFoundException("Firm not found.");

        var users = await _context.Users.AsNoTracking()
            .Where(u => u.FirmID == request.FirmID)
            .Select(u => new { u.UserID, u.EmployeeNo, u.FullName, u.Email, u.Phone, u.Department, u.Designation, u.IsActive, u.CreatedAt })
            .ToListAsync(cancellationToken);

        var cases = await _context.Cases.AsNoTracking()
            .Where(c => c.FirmID == request.FirmID)
            .Select(c => new { c.CaseID, c.CaseNumber, c.InternalReferenceNo, c.CaseTitle, c.Priority, c.FilingDate, c.IsClosed, c.IsArchived })
            .ToListAsync(cancellationToken);

        var parties = await _context.CaseParties.AsNoTracking()
            .Where(p => p.Case.FirmID == request.FirmID)
            .Select(p => new { p.PartyID, p.CaseID, p.PartyType, p.PartyName, p.Organization })
            .ToListAsync(cancellationToken);

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            await WriteCsvEntryAsync(archive, "users.csv", users.Cast<object>().ToList());
            await WriteCsvEntryAsync(archive, "cases.csv", cases.Cast<object>().ToList());
            await WriteCsvEntryAsync(archive, "case_parties.csv", parties.Cast<object>().ToList());
        }

        _logger.LogInformation("Exported data for firm {FirmID} ({FirmName}): {UserCount} users, {CaseCount} cases",
            firm.FirmID, firm.FirmName, users.Count, cases.Count);

        return memoryStream.ToArray();
    }

    // Writes one CSV file entry into the zip archive from a list of
    // anonymous-typed rows, using reflection to derive the header row from
    // the first row's properties.
    private static async Task WriteCsvEntryAsync(ZipArchive archive, string fileName, List<object> rows)
    {
        var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
        await using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);

        if (rows.Count == 0)
        {
            await writer.WriteLineAsync("(no data)");
            return;
        }

        var properties = rows[0].GetType().GetProperties();
        await writer.WriteLineAsync(string.Join(",", properties.Select(p => p.Name)));

        foreach (var row in rows)
        {
            var values = properties.Select(p => CsvEscape(p.GetValue(row)?.ToString() ?? string.Empty));
            await writer.WriteLineAsync(string.Join(",", values));
        }
    }

    private static readonly char[] FormulaTriggerChars = { '=', '+', '-', '@', '\t', '\r' };

    private static string CsvEscape(string value)
    {
        if (value.Length > 0 && FormulaTriggerChars.Contains(value[0]))
        {
            value = "'" + value;
        }

        return value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }
}
