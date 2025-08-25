using FoxholeTrade.WebApp.Components;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ConfigureTelemetry(builder);
ConfigureServices(builder);

WebApplication app = builder.Build();

ConfigureMiddlewares(app);

app.Run();

static void ConfigureTelemetry(WebApplicationBuilder builder) { }

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

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();
}
