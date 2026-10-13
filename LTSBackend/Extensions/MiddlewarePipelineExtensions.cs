using LTSBackend.Comman.Middleware;
using Microsoft.Extensions.FileProviders;

namespace LTSBackend.Extensions;

public static class MiddlewarePipelineExtensions
{




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



        var configuredPublicRoot = app.Configuration["FileStorage:PublicRoot"];
        if (!string.IsNullOrWhiteSpace(configuredPublicRoot))
        {
            var publicRoot = Path.GetFullPath(configuredPublicRoot);
            var uploadsRoot = Path.Combine(publicRoot, "uploads");
            Directory.CreateDirectory(uploadsRoot);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsRoot),
                RequestPath = "/uploads"
            });
        }

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




        app.UseRateLimiter();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}
