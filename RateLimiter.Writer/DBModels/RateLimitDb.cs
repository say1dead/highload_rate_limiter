using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RateLimiter.Writer.DbModels;

[BsonIgnoreExtraElements]
public class RateLimitDb
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("route")]
    public string Route { get; set; } = string.Empty;

    [BsonElement("requests_per_minute")]
    public int RequestsPerMinute { get; set; }
}