using RateLimiter.Reader.Models.DbModel;
using MongoDB.Driver;
using RateLimiter.Reader.Models.Domain;
using RateLimiter.Reader.Mappers;

namespace RateLimiter.Reader.Repository;

public class ReaderRepository : IReaderRepository
{
    private readonly IMongoCollection<RateLimitDb> _collection;

    public ReaderRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<RateLimitDb>("rate_limits");
    }

    public async Task<IEnumerable<RateLimit>> GetLimitsBatchAsync(int batchSize, int skip)
    {
        var options = new FindOptions<RateLimitDb>
        {
            BatchSize = batchSize,
            Skip = skip,
            Limit = batchSize
        };

        using var cursor = await _collection.FindAsync(FilterDefinition<RateLimitDb>.Empty, options);
        var dbList = await cursor.ToListAsync();

        return ReaderMapper.MapToDomain(dbList);
    }


    public async IAsyncEnumerable<RateLimitChange> WatchLimitsAsync()
    {
        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
        };

        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<RateLimitDb>>()
            .Match(change => change.OperationType == ChangeStreamOperationType.Insert ||
                           change.OperationType == ChangeStreamOperationType.Update ||
                           change.OperationType == ChangeStreamOperationType.Delete);

        using var cursor = await _collection.WatchAsync(pipeline, options);

        while (await cursor.MoveNextAsync())
        {
            foreach (var dbChange in cursor.Current)
            {
                var change = ChangeMapper.MapToDomain(dbChange);
                yield return change;
            }
        }
    }
}