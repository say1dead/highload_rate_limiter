using MongoDB.Driver;
using RateLimiter.Reader.Models.DbModel;
using RateLimiter.Reader.Models.Domain;

namespace RateLimiter.Reader.Mappers;

public static class ChangeMapper
{
    public static RateLimitChange MapToDomain(ChangeStreamDocument<RateLimitDb> change)
    {

        return change.OperationType switch
        {
            ChangeStreamOperationType.Insert => new RateLimitChange
            {
                Type = ChangeType.Insert,
                Limit = ReaderMapper.MapToDomain(change.FullDocument)
            },

            ChangeStreamOperationType.Update => new RateLimitChange
            {
                Type = ChangeType.Update,
                Limit = ReaderMapper.MapToDomain(change.FullDocument)
            },

            ChangeStreamOperationType.Delete => new RateLimitChange
            {
                Type = ChangeType.Delete,
                Id = change.DocumentKey["_id"].ToString()
            },

            _ => new RateLimitChange { Type = ChangeType.Delete }
        };
    }
}
