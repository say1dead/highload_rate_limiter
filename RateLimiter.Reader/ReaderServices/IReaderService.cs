using RateLimiter.Reader.Models.Domain;

namespace RateLimiter.Reader.Services;

public interface IReaderService
{
    Task LoadInitialLimitsAsync();

    void StartWatchingChangesAsync(CancellationToken cancellationToken);

    IReadOnlyDictionary<string, RateLimit> GetAllLimits();

    RateLimit? GetLimitByRoute(string route);

    void UpdateLimit(RateLimit limit);
    void RemoveLimit(string route);

    Task StopAsync();

    Task ProcessRequestAsync(int userId, string endpoint);
}