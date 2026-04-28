namespace RateLimiter.Writer.Domain;

public class RateLimit
{
    public string? Id { get; set; }

    public string Route { get; set; } = string.Empty;

    public int RequestsPerMinute { get; set; }
}