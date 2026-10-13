using LTSFrontend;
using LTSFrontend.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();




if (!builder.Environment.IsDevelopment())
{
    var keyPath = builder.Configuration["DataProtection:KeysPath"]
        ?? "/var/aspnet/DataProtection-Keys";
    Directory.CreateDirectory(keyPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
        .SetApplicationName("LTSFrontend");
}


builder.Services.AddLtsFrontendServices(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();


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



app.MapRazorComponents<App>().AddInteractiveServerRenderMode().Add(endpointBuilder => endpointBuilder.Metadata.Add(new AllowAnonymousAttribute()));

app.Run();
