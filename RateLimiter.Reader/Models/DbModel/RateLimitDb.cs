using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RateLimiter.Reader.Models.DbModel;

public class RateLimitDb
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("route")]
    public string Route { get; set; } = string.Empty;

    [BsonElement("requests_per_minute")]
    public int RequestsPerMinute { get; set; }

    [BsonElement("lastupdated")]
    public DateTime? LastUpdated { get; set; }
}