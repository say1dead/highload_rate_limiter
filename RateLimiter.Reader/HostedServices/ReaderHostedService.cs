using RateLimiter.Reader.Services;

namespace RateLimiter.Reader.HostedServices;

public class LimitHostedService : IHostedService
{
    private readonly IReaderService _readerService;
    private readonly ILogger<LimitHostedService> _logger;

    public LimitHostedService(IReaderService readerService, ILogger<LimitHostedService> logger)
    {
        _readerService = readerService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Limit Hosted Service...");

        try
        {
            await _readerService.LoadInitialLimitsAsync();
            _readerService.StartWatchingChangesAsync(cancellationToken);

            _logger.LogInformation("Limit Hosted Service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting Limit Hosted Service");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Limit Hosted Service...");

        await _readerService.StopAsync();
    }
}
