using System.Globalization;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Templates;
using Serilog.Templates.Themes;
using OpenTelemetryBuilder = OpenTelemetry.OpenTelemetryBuilder;

namespace FoxholeTrade.WebApp;

public static partial class ApplicationExtensions
{
    private static readonly Dictionary<TemplateThemeStyle, string> _consoleColorTheme = new()
    {
        [TemplateThemeStyle.Text] = "",
        [TemplateThemeStyle.SecondaryText] = "",
        [TemplateThemeStyle.TertiaryText] = "",
        [TemplateThemeStyle.Invalid] = "\u001B[33m",
        [TemplateThemeStyle.Null] = "\u001B[34m",
        [TemplateThemeStyle.Name] = "",
        [TemplateThemeStyle.String] = "\u001B[36m",
        [TemplateThemeStyle.Number] = "\u001B[35m",
        [TemplateThemeStyle.Boolean] = "\u001B[34m",
        [TemplateThemeStyle.Scalar] = "\u001B[32m",
        [TemplateThemeStyle.LevelVerbose] = "",
        [TemplateThemeStyle.LevelDebug] = "\u001B[1m",
        [TemplateThemeStyle.LevelInformation] = "\u001B[36;1m",
        [TemplateThemeStyle.LevelWarning] = "\u001B[33;1m",
        [TemplateThemeStyle.LevelError] = "\u001B[31;1m",
        [TemplateThemeStyle.LevelFatal] = "\u001B[31;1m"
    };

    public static void ConfigureAppSerilog(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog((_, _, serilog) =>
            {
                serilog.ReadFrom.Configuration(builder.Configuration);

                serilog.Enrich.FromLogContext();
                serilog.Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName);
                serilog.Enrich.WithProperty("EnvironmentName", builder.Environment.EnvironmentName);
                serilog.Enrich.WithMachineName();
                serilog.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    .WithDestructurers([new DbUpdateExceptionDestructurer()]));

                var messageFormatter = new ExpressionTemplate("[{@t:HH:mm:ss} {@l:u3} {Coalesce(SourceContext, '<none>')}] {@m}\n{@x}",
                    CultureInfo.InvariantCulture,
                    theme: new TemplateTheme(_consoleColorTheme));

                serilog.WriteTo.Console(messageFormatter);
                serilog.WriteTo.Debug(messageFormatter);
            },
            writeToProviders: true);
    }

    public static void ConfigureAppOpenTelemetry(this WebApplicationBuilder builder)
    {
        OpenTelemetryBuilder openTelemetryBuilder = builder.Services.AddOpenTelemetry();

        openTelemetryBuilder.ConfigureResource(r => r.AddService(builder.Environment.ApplicationName));

        openTelemetryBuilder.WithLogging(_ => { },
            logging =>
            {
                logging.IncludeScopes = true;
                logging.IncludeFormattedMessage = true;
            });

        openTelemetryBuilder.WithTracing(tracing =>
        {
            tracing.AddHttpClientInstrumentation();
            tracing.AddAspNetCoreInstrumentation(instr =>
            {
                // Exclude health check requests from tracing
                instr.Filter = httpContext =>
                {
                    PathString path = httpContext.Request.Path;
                    bool isHealthCheck = path.HasValue && (
                        path.Value.Equals("/health", StringComparison.OrdinalIgnoreCase)
                     || path.Value.Equals("/alive", StringComparison.OrdinalIgnoreCase)
                    );

                    return !isHealthCheck;
                };
            });
        });

        openTelemetryBuilder.WithMetrics(meter =>
        {
            meter.AddAspNetCoreInstrumentation();
            meter.AddHttpClientInstrumentation();
            meter.AddRuntimeInstrumentation();
        });

        if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"])) openTelemetryBuilder.UseAzureMonitor();
    }

    public static void ConfigureAppHealthChecks(this WebApplicationBuilder builder)
    {
        IHealthChecksBuilder healthChecksBuilder = builder.Services.AddHealthChecks();

        healthChecksBuilder.AddCheck("alive", () => HealthCheckResult.Healthy(), ["alive"]);
    }

    public static void MapAppHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = check => check.Tags.Contains("alive") });
    }
}
