using RateLimiter.Writer.Domain;

namespace Writer.Repositories;

public interface IRateLimitRepository
{
    Task EnsureIndexesAsync(CancellationToken ct);

    Task<RateLimit?> CreateAsync(RateLimit limit, CancellationToken ct);

    Task<RateLimit?> GetByRouteAsync(string route, CancellationToken ct);

    Task<RateLimit?> UpdateAsync(RateLimit limit, CancellationToken ct);

    Task<bool> DeleteAsync(string route, CancellationToken ct);
}
