using RateLimiter.Reader.Models.DbModel;
using MongoDB.Driver;
using RateLimiter.Reader.Models.Domain;

namespace RateLimiter.Reader.Repository;

public interface IReaderRepository
{
    Task<IEnumerable<RateLimit>> GetLimitsBatchAsync(int batchSize, int skip);
    IAsyncEnumerable<RateLimitChange> WatchLimitsAsync();
}