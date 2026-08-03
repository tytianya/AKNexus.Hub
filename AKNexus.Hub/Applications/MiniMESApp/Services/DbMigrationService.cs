using MiniMESApp.Data;
using Microsoft.EntityFrameworkCore;

namespace MiniMESApp.Services;

public class DbMigrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DbMigrationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DasAppDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
