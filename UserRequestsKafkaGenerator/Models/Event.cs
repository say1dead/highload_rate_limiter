using System.Text.Json.Serialization;

namespace UserRequestsKafkaGenerator.Models;

public sealed class Event
{
    [JsonPropertyName("user_id")]
    public int UserId { get; init; }

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; init; } = "";
}
