using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoxholeTrade.Data;

public class ApplyDatabaseMigrationsHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ApplyDatabaseMigrationsHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!db.Database.IsRelational())
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
            return;
        }

        if (db.Database.HasPendingModelChanges())
            throw new InvalidOperationException("Code-first changes detected without created migration. Create missing migration with dotnet-ef.");

        await db.Database.MigrateAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
