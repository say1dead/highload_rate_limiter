using System.Text.Json.Serialization;

namespace RateLimiter.Reader.Models.Domain;

public class RequestEvent
{
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = string.Empty;
}
