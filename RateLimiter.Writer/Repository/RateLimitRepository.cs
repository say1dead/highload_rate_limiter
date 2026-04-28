using RateLimiter.Writer.Domain;
using RateLimiter.Writer.DbModels;
using RateLimiter.Writer.Mappers;
using MongoDB.Driver;
using Writer.Repositories;
using RateLimiter.Writer.Exceptions;

namespace RateLimiter.Writer.Repository;

public class RateLimitRepository : IRateLimitRepository
{
    private readonly IMongoCollection<RateLimitDb> _collection;
    private readonly RateLimitDbMapper _mapper;

    public RateLimitRepository(IMongoCollection<RateLimitDb> collection, RateLimitDbMapper mapper)
    {
        _collection = collection;
        _mapper = mapper;
    }

    private IMongoCollection<RateLimitDb> GetCollection() => _collection;

    public async Task EnsureIndexesAsync(CancellationToken ct)
    {
        var coll = GetCollection();
        var keys = Builders<RateLimitDb>.IndexKeys.Ascending(x => x.Route);
        var options = new CreateIndexOptions { Unique = true, Name = "unique_indexes_rate_limits_route" };
        await coll.Indexes.CreateOneAsync(new CreateIndexModel<RateLimitDb>(keys, options), cancellationToken: ct);
    }

    public async Task<RateLimit?> CreateAsync(RateLimit limit, CancellationToken ct)
    {
        var coll = GetCollection();
        var db = _mapper.ToDb(limit);

        try
        {
            await coll.InsertOneAsync(db, cancellationToken: ct);
            return _mapper.ToDomain(db);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new AlreadyExistsException("limit with this route already exists", ex);
        }
    }

    public async Task<RateLimit?> GetByRouteAsync(string route, CancellationToken ct)
    {
        var coll = GetCollection();
        var filter = Builders<RateLimitDb>.Filter.Eq(x => x.Route, route);

        var dbEntity = await coll.Find(filter).FirstOrDefaultAsync(ct);
        return dbEntity is null ? null : _mapper.ToDomain(dbEntity);
    }

    public Task<RateLimit?> UpdateAsync(RateLimit limit, CancellationToken ct)
    {
        var coll = GetCollection();
        var filter = Builders<RateLimitDb>.Filter.Eq(x => x.Route, limit.Route);
        var update = Builders<RateLimitDb>.Update
            .Set(x => x.RequestsPerMinute, limit.RequestsPerMinute);

        var opts = new FindOneAndUpdateOptions<RateLimitDb>
        {
            ReturnDocument = ReturnDocument.After
        };

        return coll.FindOneAndUpdateAsync(filter, update, opts, ct)
            .ContinueWith(t => t.Result is null ? null : _mapper.ToDomain(t.Result), ct);
    }

    public async Task<bool> DeleteAsync(string route, CancellationToken ct)
    {
        var coll = GetCollection();
        var filter = Builders<RateLimitDb>.Filter.Eq(x => x.Route, route);
        var result = await coll.DeleteOneAsync(filter, ct);
        return result.DeletedCount > 0;
    }
}
