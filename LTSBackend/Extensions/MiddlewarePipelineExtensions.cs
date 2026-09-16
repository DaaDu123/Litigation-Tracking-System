using LTSBackend.Comman.Middleware;

namespace LTSBackend.Extensions;

public static class MiddlewarePipelineExtensions
{
    // Kept as a single method, not split further - the whole point of this
    // block is that the call order matters (exception handling first,
    // auth before rate limiting before authorization, etc.), so scattering
    // it across files would make that ordering harder to see, not easier.
    public static WebApplication UseAppMiddlewarePipeline(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<AuditMiddleware>();
        app.UseForwardedHeaders();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(c => c.RouteTemplate = "swagger/{documentName}/swagger.json");
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LTS API v1.0.0");
                c.RoutePrefix = "";
                c.DefaultModelsExpandDepth(2);
                c.DefaultModelExpandDepth(2);
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            });
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "Production");

        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            await next();
        });

        app.UseAuthentication();

        // Runs after authentication (so partitioning could later move to
        // per-user, not just per-IP) and before authorization (so a
        // rate-limited request never reaches a handler).
        app.UseRateLimiter();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}
