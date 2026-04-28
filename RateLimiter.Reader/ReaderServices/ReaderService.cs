using RateLimiter.Reader.Models.Domain;
using RateLimiter.Reader.Models.DbModel;
using RateLimiter.Reader.Repository;
using RateLimiter.Reader.Mappers;
using RateLimiter.Reader.Cache;
using MongoDB.Driver;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;

namespace RateLimiter.Reader.Services;

public class ReaderService : IReaderService
{
    private readonly ConcurrentDictionary<string, RateLimit> _limits;
    private readonly ConcurrentDictionary<string, List<DateTime>> _requests;
    private readonly IReaderRepository _repository;
    private readonly ILogger<ReaderService> _logger;
    private readonly RedisCache _redisCache;
    private readonly ConcurrentDictionary<string, DateTime> _loggedBlocks = new();
    private Task? _watchingTask;

    public ReaderService(
        IReaderRepository repository,
        ILogger<ReaderService> logger,
        RedisCache redisCache)
    {
        _limits = new ConcurrentDictionary<string, RateLimit>();
        _requests = new ConcurrentDictionary<string, List<DateTime>>();
        _repository = repository;
        _logger = logger;
        _redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
    }

    public async Task LoadInitialLimitsAsync()
    {
        const int batchSize = 1000;
        int skip = 0;
        bool hasMoreData = true;

        while (hasMoreData)
        {
            var batch = await _repository.GetLimitsBatchAsync(batchSize, skip);
            int count = 0;

            foreach (var limit in batch)
            {
                _limits[limit.Route] = limit;
                count++;
            }

            hasMoreData = count == batchSize;
            skip += batchSize;
        }

        _logger.LogInformation("Finished loading initial limits. Total: {TotalCount}", _limits.Count);
    }

    public void StartWatchingChangesAsync(CancellationToken cancellationToken)
    {
        _watchingTask = Task.Run(() => WatchChangesAsync(cancellationToken), cancellationToken);
        _logger.LogInformation("Started watching for MongoDB changes");
    }

    private async Task WatchChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var change in _repository.WatchLimitsAsync())
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                ProcessChange(change);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cancelled watching");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in change watching loop");
        }
    }

    private void ProcessChange(RateLimitChange change)
    {
        switch (change.Type)
        {
            case ChangeType.Insert:
            case ChangeType.Update:
                if (change.Limit != null)
                {
                    _limits[change.Limit.Route] = change.Limit;
                    _logger.LogInformation("{Change} limit for route {Route}, RPM: {RPM}",
                        change.Type, change.Limit.Route, change.Limit.RequestsPerMinute);
                }
                break;

            case ChangeType.Delete:
                if (!string.IsNullOrEmpty(change.Id))
                {
                    var routeToRemove = _limits.FirstOrDefault(x => x.Value.Id == change.Id).Key;
                    if (!string.IsNullOrEmpty(routeToRemove))
                    {
                        _limits.TryRemove(routeToRemove, out _);
                        _logger.LogInformation("Deleted limit for route: {Route}", routeToRemove);
                    }
                }
                break;
        }
    }

    public IReadOnlyDictionary<string, RateLimit> GetAllLimits()
        => new ReadOnlyDictionary<string, RateLimit>(_limits);

    public RateLimit? GetLimitByRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            return null;

        return _limits.FirstOrDefault(x =>
            string.Equals(x.Key, route, StringComparison.OrdinalIgnoreCase)).Value;
    }

    public void UpdateLimit(RateLimit limit) => _limits[limit.Route] = limit;

    public void RemoveLimit(string route) => _limits.TryRemove(route, out _);

    public async Task ProcessRequestAsync(int userId, string endpoint)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return;

            endpoint = endpoint.Trim().ToLower().Replace("/", "_");

            if (await _redisCache.IsBlockedAsync(userId, endpoint))
            {
                string blockKey = $"{userId}:{endpoint}";
                if (!_loggedBlocks.TryGetValue(blockKey, out var expiry) || expiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("User {UserId} is blocked for endpoint {Endpoint}", userId, endpoint);
                    _loggedBlocks[blockKey] = DateTime.UtcNow.AddMinutes(5);
                }
                return;
            }

            var limit = GetLimitByRoute(endpoint);
            if (limit == null)
            {
                return;
            }

            string key = $"{userId}:{endpoint}";
            var now = DateTime.UtcNow;
            var list = _requests.GetOrAdd(key, _ => new List<DateTime>());

            lock (list)
            {
                list.Add(now);
                list.RemoveAll(t => (now - t).TotalSeconds > 60);
            }

            int count = list.Count;

            if (count > limit.RequestsPerMinute)
            {
                _logger.LogWarning("User {UserId} exceeded limit for {Endpoint} ({Count}/{Limit})",
                    userId, endpoint, count, limit.RequestsPerMinute);

                await _redisCache.SetBlockAsync(userId, endpoint);
                _requests.TryRemove(key, out _);
            }
            else
            {
                _logger.LogInformation("User {UserId} -> {Endpoint}: {Count}/{Limit}",
                    userId, endpoint, count, limit.RequestsPerMinute);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing request for user {UserId}, endpoint {Endpoint}", userId, endpoint);
        }
    }

    public async Task StopAsync()
    {
        if (_watchingTask != null)
            await _watchingTask;

        _logger.LogInformation("Limit service stopped");
    }
}
