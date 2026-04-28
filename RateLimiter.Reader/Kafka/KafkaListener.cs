namespace RateLimiter.Reader.ReaderKafka;

public class KafkaListener : IHostedService
{
    private readonly KafkaConsumer _consumer;
    private CancellationTokenSource? _cts;
    private Task? _consumeTask;

    public KafkaListener(KafkaConsumer consumer)
    {
        _consumer = consumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new CancellationTokenSource();
        _consumeTask = Task.Factory.StartNew(
            () => _consumer.Consume(_cts.Token),
            TaskCreationOptions.LongRunning);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        return Task.CompletedTask;
    }
}
