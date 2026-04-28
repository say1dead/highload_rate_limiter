using MongoDB.Bson.Serialization.Attributes;

namespace RateLimiter.Reader.Models.Domain;

public class RateLimitChange
{
    public ChangeType Type { get; set; }

    public RateLimit? Limit { get; set; }

    public string? Id { get; set; }
}

public enum ChangeType
{
    Insert,
    Update,
    Delete
}
