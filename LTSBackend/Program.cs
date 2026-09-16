using LTSBackend.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Each Add* call below is one self-contained concern, in Extensions/ - see
// that folder for what each one actually configures.
builder
    .AddAppLogging()
    .AddAppDatabase()
    .AddAppJwtAuthentication()
    .AddAppCors()
    .AddAppForwardedHeaders();

builder.Services
    .AddAppMediatR()
    .AddApplicationServices()
    .AddAppBackgroundServices()
    .AddAppAuthorizationPolicies()
    .AddAppRateLimiting()
    .AddAppHealthChecks()
    .AddAppControllers()
    .AddAppSwagger();

var app = builder.Build();

// In Docker, SQL Server may start after the API container. Wait for SQL Server,
// create the application database if needed, and apply EF Core migrations before
// hosted background services start querying the database.
await app.MigrateAppDatabaseAsync();

app.UseAppMiddlewarePipeline();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Litigation Tracking System API starting in {Environment}", app.Environment.EnvironmentName);

app.Run();
