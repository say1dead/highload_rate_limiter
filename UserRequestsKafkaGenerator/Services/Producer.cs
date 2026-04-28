using Confluent.Kafka;
using System.Text.Json;
using UserRequestsKafkaGenerator.Models;

namespace UserRequestsKafkaGenerator.Services;


public sealed class KafkaProducer : IAsyncDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly JsonSerializerOptions _json;

    private static Acks ParseAcks(string? acks)
    {
        return acks?.ToLowerInvariant() switch
        {
            "0" or "none" => Acks.None,
            "1" or "leader" => Acks.Leader,
            "all" or "-1" => Acks.All,
            _ => Acks.All
        };
    }

    public KafkaProducer(string bootstrapServers, string topic, string? acks = "all")
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = ParseAcks(acks)
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
        _topic = topic;

        _json = new JsonSerializerOptions { WriteIndented = false };
    }

    public async Task SendAsync(Event evt, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(evt, _json);
        var key = evt.UserId.ToString();

        try
        {
            await _producer.ProduceAsync(_topic, new Message<string, string>
            {
                Key = key,
                Value = payload
            }, ct);
        }
        catch (ProduceException<string, string> ex)
        {
            Console.Error.WriteLine($"Kafka produce error: {ex.Error.Reason}");
        }
    }

    public ValueTask DisposeAsync()
    {
        _producer.Flush();
        _producer.Dispose();
        return ValueTask.CompletedTask;
    }
}
