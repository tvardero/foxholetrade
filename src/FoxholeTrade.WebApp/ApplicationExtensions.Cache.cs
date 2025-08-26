using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ZiggyCreatures.Caching.Fusion;

namespace FoxholeTrade.WebApp;

public static partial class ApplicationExtensions
{
    public static void ConfigureAppFusionCache(this WebApplicationBuilder builder)
    {
        bool useFactoryTimeouts = !builder.Environment.IsDevelopment();

        // see docs https://github.com/ZiggyCreatures/FusionCache/blob/main/docs/StepByStep.md
        IFusionCacheBuilder fusion = builder.Services.AddFusionCache()
            .AsHybridCache()
            .WithCacheKeyPrefix($"{builder.Environment.ApplicationName}:")
            .WithOptions(options =>
            {
                options.DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(30);
                options.AutoRecoveryMaxRetryCount = 20;

                // configure log levels of events (not filters!)
                options.FactorySyntheticTimeoutsLogLevel = LogLevel.Information;
                options.DistributedCacheSyntheticTimeoutsLogLevel = LogLevel.Information;
                options.BackplaneSyntheticTimeoutsLogLevel = LogLevel.Information;
            })
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                // cache entry duration
                Duration = TimeSpan.FromMinutes(1),
                JitterMaxDuration = TimeSpan.FromSeconds(2),

                // entry factory failsafe
                IsFailSafeEnabled = true,
                FailSafeMaxDuration = TimeSpan.FromHours(2),
                FailSafeThrottleDuration = TimeSpan.FromSeconds(30),

                // entry factory soft and hard timeouts
                FactorySoftTimeout = useFactoryTimeouts ? TimeSpan.FromMilliseconds(100) : Timeout.InfiniteTimeSpan,
                FactoryHardTimeout = useFactoryTimeouts ? TimeSpan.FromMilliseconds(1500) : Timeout.InfiniteTimeSpan,
                AllowTimedOutFactoryBackgroundCompletion = false,

                // distributed cache soft and hard timeouts
                DistributedCacheSoftTimeout = TimeSpan.FromSeconds(1),
                DistributedCacheHardTimeout = TimeSpan.FromSeconds(2),
                AllowBackgroundDistributedCacheOperations = false,
            })
            .WithSystemTextJsonSerializer(new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve });
        // .TryWithRegisteredBackplane()
        // .TryWithRegisteredDistributedCache();

        builder.Services.ConfigureOpenTelemetryMeterProvider(m => m.AddFusionCacheInstrumentation());
        builder.Services.ConfigureOpenTelemetryTracerProvider(t => t.AddFusionCacheInstrumentation());
    }
}
