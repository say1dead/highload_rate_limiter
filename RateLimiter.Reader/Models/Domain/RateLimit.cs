namespace RateLimiter.Reader.Models.Domain;

public class RateLimit
{
    public string Id { get; set; } = string.Empty;

    public string Route { get; set; }
    public int RequestsPerMinute { get; set; }
}