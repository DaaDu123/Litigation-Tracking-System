using Microsoft.OpenApi;
using System.Reflection;

namespace LTSBackend.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddAppControllers(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

        return services;
    }

    public static IServiceCollection AddAppSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Litigation Tracking System API",
                Version = "v1.0.0",
                Description = "Complete Case Management System with Role-Based Access Control, Document Management, and Audit Logging",
                TermsOfService = new Uri("https://example.com/terms"),
                Contact = new OpenApiContact { Name = "LTS Support Team", Email = "support@lts.local" },
                License = new OpenApiLicense { Name = "MIT License" }
            });

            c.EnableAnnotations();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme.\r\n\r\nEnter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"",
                Name = "Authorization",
                In = ParameterLocation.Header
            });

            c.AddSecurityRequirement(document => new()
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            c.OrderActionsBy(x => x.HttpMethod);
        });

        return services;
    }
}