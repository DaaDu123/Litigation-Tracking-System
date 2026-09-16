using LTSBackend.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LTSBackend.Extensions;

public static class DatabaseExtensions
{
    public static WebApplicationBuilder AddAppDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."),
                sql =>
                {
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
        });

        return builder;
    }

    /// <summary>
    /// Waits for SQL Server to become available, creates the configured database
    /// when it does not exist, and applies all pending EF Core migrations.
    /// This is intended for containerized deployments where SQL Server and the
    /// API start at roughly the same time.
    /// </summary>
    public static async Task MigrateAppDatabaseAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 30;
        var logger = app.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseInitialization");

        var connectionString = app.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var sqlConnectionString = new SqlConnectionStringBuilder(connectionString);
        var databaseName = sqlConnectionString.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("The SQL Server connection string must specify a database name.");

        // Connect to master first. If the application database does not exist,
        // connecting directly to it can produce SQL Server error 4060.
        var masterConnectionString = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        }.ConnectionString;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await using var masterConnection = new SqlConnection(masterConnectionString);
                await masterConnection.OpenAsync(cancellationToken);

                logger.LogInformation("SQL Server is ready. Ensuring database {DatabaseName} exists.", databaseName);

                // Database names cannot be SQL parameters, so safely escape the
                // identifier before putting it in CREATE DATABASE.
                var escapedDatabaseName = databaseName.Replace("]", "]]", StringComparison.Ordinal);
                await using var command = masterConnection.CreateCommand();
                command.CommandText = $"IF DB_ID(@databaseName) IS NULL CREATE DATABASE [{escapedDatabaseName}];";
                command.Parameters.AddWithValue("@databaseName", databaseName);
                await command.ExecuteNonQueryAsync(cancellationToken);

                // Now connect through the normal AppDbContext connection and
                // apply the migrations included in the application image.
                await using var scope = app.Services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                logger.LogInformation("Applying pending EF Core migrations to {DatabaseName}.", databaseName);
                await db.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Database {DatabaseName} is initialized successfully.", databaseName);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (SqlException ex) when (attempt < maxAttempts)
            {
                var delaySeconds = Math.Min(attempt * 2, 10);
                logger.LogWarning(
                    ex,
                    "SQL Server is not ready yet (attempt {Attempt}/{MaxAttempts}). Retrying in {DelaySeconds}s.",
                    attempt,
                    maxAttempts,
                    delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"SQL Server did not become available after {maxAttempts} attempts.");
    }
}
