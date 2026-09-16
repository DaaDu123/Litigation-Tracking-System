using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LTSBackend.Data;

/// <summary>
/// Design-time factory for EF Core CLI/PMC tools ("dotnet ef migrations add",
/// "dotnet ef database update", Package Manager Console's Add-Migration).
///
/// Without this, "dotnet ef" tries to build the FULL application host
/// (everything registered in Program.cs - authentication, authorization
/// policies, MediatR, hosted services, etc.) just to pull a DbContextOptions
/// out of it, and any DI misconfiguration anywhere in that graph (as
/// happened with PermissionPolicyProvider's missing DefaultAuthorizationPolicyProvider
/// registration) breaks migrations even though it has nothing to do with
/// the database. Implementing IDesignTimeDbContextFactory gives EF Core
/// tools a minimal, self-contained way to build just the DbContext -
/// reading the connection string directly from appsettings.json (and
/// appsettings.{ASPNETCORE_ENVIRONMENT}.json if present) - so migration
/// commands work reliably regardless of the rest of the app's DI graph.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
            //.AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. Set it in appsettings.json " +
                "(or appsettings.Development.json) before running EF Core migration commands.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        });

        return new AppDbContext(optionsBuilder.Options);
    }
}
