namespace RateLimiter.Reader.Options;

public class MongoDbOption
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string CollectionName { get; set; } = "rate_limits";
}