using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

namespace FoxholeTrade.Data;

public static class ApplicationExtensions
{
    public static void ConfigureAppDbContext(this WebApplicationBuilder builder)
    {
        string? connectionString = builder.Configuration.GetConnectionString("Db");
        if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("Connection string for database is not set");

        builder.Services.AddDbContext<AppDbContext>(db =>
        {
            if (builder.Environment.IsDevelopment())
            {
                db.EnableDetailedErrors();
                db.EnableSensitiveDataLogging();
                db.ConfigureWarnings(warn => warn.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
            }

            db.UseSqlServer(connectionString);
        });

        builder.Services.AddHostedService<ApplyDatabaseMigrationsHostedService>();
        
        builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
        builder.Services.ConfigureOpenTelemetryTracerProvider(trace => trace.AddEntityFrameworkCoreInstrumentation());
    }
}
