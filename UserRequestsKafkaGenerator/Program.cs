using System.Text.Json;
using UserRequestsKafkaGenerator.Services;

var config = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText("appsettings.json"));
var kafkaCfg = config.GetProperty("kafka");

var bootstrap = kafkaCfg.GetProperty("bootstrapServers").GetString()!;
var topic = kafkaCfg.GetProperty("topic").GetString()!;
var acks = kafkaCfg.TryGetProperty("acks", out var ackVal) ? ackVal.GetString() : "all";

await using var producer = new KafkaProducer(bootstrap, topic, acks);
await using var scheduler = new WorkScheduler(producer);

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var item1 = scheduler.Add(123, "GetUserById", 10);
var item2 = scheduler.Add(321, "GetUserById", 20);

Console.WriteLine("Demo started:");
Console.WriteLine(item1);
Console.WriteLine(item2);

_ = Task.Run(async () =>
{
    await Task.Delay(10000);
    scheduler.Update(item2.Id, 25, "UpdateUser");
    Console.WriteLine($"[AutoDemo] Updated {item2.Id} -> rpm=25, endpoint=UpdateUser");
});

var loop = new CommandLoop(scheduler);
await loop.RunAsync(cts.Token);

Console.WriteLine("Kafka Messages Generator");