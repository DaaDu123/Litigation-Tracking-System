using LTSBackend.Data;
using LTSBackend.Services.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace LTSBackend.Extensions;

public static class JwtAuthenticationExtensions
{
    public static WebApplicationBuilder AddAppJwtAuthentication(this WebApplicationBuilder builder)
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var jwtSecret = jwtSettings["SecretKey"];

        if (string.IsNullOrWhiteSpace(jwtSecret))
            throw new InvalidOperationException("JwtSettings:SecretKey is missing. Check appsettings.json");

        builder.Services.Configure<JwtSettings>(jwtSettings);

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ValidateLiveSessionAsync
                };
            });

        return builder;
    }

    // A signed, unexpired JWT can still belong to a session that should no
    // longer work - account deactivated, firm blocked, or password/logout
    // rotated the security stamp since the token was issued. This runs on
    // every authenticated request to catch those cases immediately instead
    // of waiting for the token to expire naturally.
    private static async Task ValidateLiveSessionAsync(TokenValidatedContext context)
    {
        var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            context.Fail("Token is missing a valid user identifier.");
            return;
        }

        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        var record = await dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(u => u.UserID == userId)
            .Select(u => new
            {
                u.IsActive,
                u.IsDeleted,
                u.SecurityStamp,
                FirmBlocked = u.Firm != null && u.Firm.IsBlocked,
                FirmDeleted = u.Firm != null && u.Firm.IsDeleted
            })
            .FirstOrDefaultAsync();

        if (record == null || record.IsDeleted || !record.IsActive)
        {
            context.Fail("This account is no longer active.");
            return;
        }

        if (record.FirmBlocked || record.FirmDeleted)
        {
            context.Fail("This firm's workspace has been suspended.");
            return;
        }

        var tokenStamp = context.Principal?.FindFirstValue("SecurityStamp");
        if (string.IsNullOrEmpty(tokenStamp) || tokenStamp != record.SecurityStamp)
        {
            context.Fail("Session has been invalidated. Please log in again.");
        }
    }
}
