using MatchIQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatchIQ.API.BackgroundServices;

public class DailyJobsService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyJobsService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public DailyJobsService(IServiceScopeFactory scopeFactory, ILogger<DailyJobsService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunJobsAsync(stoppingToken);

        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunJobsAsync(stoppingToken);
        }
    }

    private async Task RunJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            var expiredOffers = await context.Database
                .ExecuteSqlRawAsync("SELECT expire_stale_offers()", cancellationToken);

            _logger.LogInformation("expire_stale_offers ejecutado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar expire_stale_offers.");
        }

        try
        {
            await context.Database
                .ExecuteSqlRawAsync("SELECT expire_stale_submissions()", cancellationToken);

            _logger.LogInformation("expire_stale_submissions ejecutado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar expire_stale_submissions.");
        }
    }
}
