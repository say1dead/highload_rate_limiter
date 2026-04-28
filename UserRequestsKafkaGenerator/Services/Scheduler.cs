using System.Collections.Concurrent;
using UserRequestsKafkaGenerator.Models;

namespace UserRequestsKafkaGenerator.Services;

public sealed class WorkScheduler : IAsyncDisposable
{
    private readonly KafkaProducer _producer;
    private readonly ConcurrentDictionary<Guid, (WorkItem item, CancellationTokenSource cts)> _tasks = new();

    public WorkScheduler(KafkaProducer producer)
    {
        _producer = producer;
    }

    public WorkItem Add(int userId, string endpoint, int rpm)
    {
        var item = new WorkItem(userId, endpoint, rpm);
        var cts = new CancellationTokenSource();

        if (!_tasks.TryAdd(item.Id, (item, cts)))
            throw new InvalidOperationException("Failed to add work item.");

        _ = RunLoopAsync(item, cts.Token);
        return item;
    }

    public bool Update(Guid id, int? rpm = null, string? endpoint = null)
    {
        if (!_tasks.TryGetValue(id, out var entry)) return false;
        if (rpm.HasValue) entry.item.Rpm = Math.Max(0, rpm.Value);
        if (!string.IsNullOrWhiteSpace(endpoint)) entry.item.Endpoint = endpoint;
        return true;
    }

    public bool Remove(Guid id)
    {
        if (_tasks.TryRemove(id, out var entry))
        {
            entry.cts.Cancel();
            entry.cts.Dispose();
            return true;
        }
        return false;
    }

    public IEnumerable<WorkItem> List() => _tasks.Values.Select(v => v.item);

    private async Task RunLoopAsync(WorkItem item, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var rpm = Math.Max(0, item.Rpm);
            if (rpm == 0)
            {
                await Task.Delay(1000, ct);
                continue;
            }

            var delayMs = (int)Math.Max(1, 60000.0 / rpm);
            var evt = new Event { UserId = item.UserId, Endpoint = item.Endpoint };

            await _producer.SendAsync(evt, ct);
            await Task.Delay(delayMs, ct);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var (_, v) in _tasks)
        {
            v.cts.Cancel();
            v.cts.Dispose();
        }
        _tasks.Clear();
        await _producer.DisposeAsync();
    }
}
