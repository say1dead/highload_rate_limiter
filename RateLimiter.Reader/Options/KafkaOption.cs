namespace RateLimiter.Reader.Options;

public class KafkaOption
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = "reader-group";
    public string Topic { get; set; } = "user-events";
}