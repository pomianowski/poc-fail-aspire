using Microsoft.EntityFrameworkCore;

namespace AspirePoc.App;

public class DatabaseTesterBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<DatabaseTesterBackgroundService> logger
) : IHostedService
{
    private const int DefaultDelay = 5;

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(FireAndForget, cancellationToken);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task FireAndForget()
    {
        try
        {
            using CancellationTokenSource cts = new();

            logger.LogWarning("Waiting for {Delay} seconds", DefaultDelay);

            await Task.Delay(TimeSpan.FromSeconds(DefaultDelay), cts.Token); // wait for db

            using IServiceScope scope = serviceProvider.CreateScope();
            IServiceProvider services = scope.ServiceProvider;
            AppDbContext context = services.GetRequiredService<AppDbContext>();

            await context.Database.MigrateAsync(cancellationToken: cts.Token);

            // Add a new item
            Item newItem = new() { Name = "Sample Item" };
            context.Items.Add(newItem);
            await context.SaveChangesAsync(cts.Token);

            // Read the item back
            List<Item> items = await context
                .Items.OrderBy(x => x.Id)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync(cancellationToken: cts.Token);

            logger.LogInformation("Retrieved {Count} items", items.Count);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Postgres failed.");

            throw;
        }
    }
}
