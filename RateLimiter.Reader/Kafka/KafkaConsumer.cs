using Confluent.Kafka;
using System.Text.Json;
using RateLimiter.Reader.Models.Domain;
using RateLimiter.Reader.Services;
using Microsoft.Extensions.Options;
using RateLimiter.Reader.Options;

namespace RateLimiter.Reader.ReaderKafka;

public class KafkaConsumer
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IReaderService _readerService;
    private readonly string _topic;
    private readonly ILogger<KafkaConsumer> _logger;

    public KafkaConsumer(
        IOptions<KafkaOption> kafkaOption,
        IReaderService readerService,
        ILogger<KafkaConsumer> logger)
    {
        _readerService = readerService;
        _logger = logger;
        _topic = kafkaOption.Value.Topic;

        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaOption.Value.BootstrapServers,
            GroupId = kafkaOption.Value.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }

    public async void Consume(CancellationToken token)
    {
        _consumer.Subscribe(_topic);
        _logger.LogInformation("Kafka consumer subscribed to topic: {Topic}", _topic);

        while (!token.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(token);
                var evt = JsonSerializer.Deserialize<RequestEvent>(result.Message.Value);
                if (evt == null) continue;

                await _readerService.ProcessRequestAsync(evt.UserId, evt.Endpoint);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consume error");
            }
        }

        _consumer.Close();
    }
}