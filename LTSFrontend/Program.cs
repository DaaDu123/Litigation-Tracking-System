using LTSFrontend;
using LTSFrontend.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configure Forwarded Headers for Nginx / Cloudflare
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ProtectedLocalStorage uses ASP.NET Core Data Protection. Keep its key ring
// stable in production so stored sessions remain readable after a container
// restart or replacement.
if (!builder.Environment.IsDevelopment())
{
    var keyPath = builder.Configuration["DataProtection:KeysPath"]
        ?? "/var/aspnet/DataProtection-Keys";
    Directory.CreateDirectory(keyPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
        .SetApplicationName("LTSFrontend");
}

// LTS auth + API client + feature services
builder.Services.AddLtsFrontendServices(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
// This endpoint serves the interactive shell with prerendering disabled.
// Route and role checks run in AuthorizeRouteView after ProtectedLocalStorage
// restores the browser session; backend APIs independently enforce access.
app.MapRazorComponents<App>().AddInteractiveServerRenderMode().Add(endpointBuilder => endpointBuilder.Metadata.Add(new AllowAnonymousAttribute()));

app.Run();
