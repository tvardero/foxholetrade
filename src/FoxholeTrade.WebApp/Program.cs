using FoxholeTrade.Data;
using FoxholeTrade.WebApp;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.ConfigureAppSerilog();
builder.ConfigureAppOpenTelemetry();
builder.ConfigureAppHealthChecks();
builder.ConfigureAppDbContext();
builder.ConfigureAppAuth();

builder.Services.AddRazorPages();

WebApplication app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
else app.UseExceptionHandler("/Error");

app.UseRouting();

app.MapAppHealthChecks();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
