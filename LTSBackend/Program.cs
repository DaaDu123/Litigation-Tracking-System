using LTSBackend.Extensions;

var builder = WebApplication.CreateBuilder(args);



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




await app.MigrateAppDatabaseAsync();

app.UseAppMiddlewarePipeline();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Litigation Tracking System API starting in {Environment}", app.Environment.EnvironmentName);

app.Run();
