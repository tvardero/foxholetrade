using FoxholeTrade.WebApp.Components;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ConfigureTelemetry(builder);
ConfigureServices(builder);

WebApplication app = builder.Build();

ConfigureMiddlewares(app);

app.Run();

static void ConfigureTelemetry(WebApplicationBuilder builder)
{
    IHealthChecksBuilder healthChecksBuilder = builder.Services.AddHealthChecks();
    healthChecksBuilder.AddCheck("self", () => HealthCheckResult.Healthy(), ["alive"]);
}

static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();
}

static void ConfigureMiddlewares(WebApplication app)
{
    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error", createScopeForErrors: true);

    app.UseAntiforgery();

    app.MapHealthChecks("/health");
    app.MapHealthChecks("/alive", new HealthCheckOptions() { Predicate = c => c.Tags.Contains("alive") });

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();
}
