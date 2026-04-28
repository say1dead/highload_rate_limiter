using StackExchange.Redis;

namespace RateLimiter.Reader.Cache;

public class RedisCache
{
    private readonly IDatabase _db;

    public RedisCache(IConnectionMultiplexer connection)
    {
        _db = connection.GetDatabase();
    }

    public async Task SetBlockAsync(int userId, string endpoint)
    {
        string blockKey = $"blocked:{userId}:{endpoint}";
        await _db.StringSetAsync(blockKey, "1", TimeSpan.FromMinutes(5));
    }

    public async Task<bool> IsBlockedAsync(int userId, string endpoint)
    {
        string blockKey = $"blocked:{userId}:{endpoint}";
        return await _db.KeyExistsAsync(blockKey);
    }
}