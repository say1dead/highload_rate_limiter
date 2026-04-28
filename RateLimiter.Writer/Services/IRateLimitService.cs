using RateLimiter.Writer.Domain;
using RateLimiter.Writer.Protos;

namespace Writer.Services;

public interface IRateLimitService
{
    Task<RateLimit> CreateAsync(RateLimit limit, CancellationToken ct);

    Task<RateLimit?> GetByRouteAsync(string route, CancellationToken ct);

    Task<RateLimit?> UpdateAsync(RateLimit limit, CancellationToken ct);

    Task<bool> DeleteAsync(string route, CancellationToken ct);
}
