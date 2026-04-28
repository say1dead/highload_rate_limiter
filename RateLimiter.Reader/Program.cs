using RateLimiter.Reader.Repository;
using RateLimiter.Reader.Services;
using RateLimiter.Reader.HostedServices;
using RateLimiter.Reader.Controllers;
using RateLimiter.Reader.Options;
using RateLimiter.Reader.Cache;
using RateLimiter.Reader.ReaderKafka;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbOption>(
    builder.Configuration.GetSection("Mongo"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbOption>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDbOption>>().Value;
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.Configure<RedisOption>(
    builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisSettings = sp.GetRequiredService<IOptions<RedisOption>>().Value;
    return ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
});
builder.Services.AddSingleton<RedisCache>();

builder.Services.AddHostedService<LimitHostedService>();

builder.Services.Configure<KafkaOption>(
    builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<KafkaConsumer>();
builder.Services.AddHostedService<KafkaListener>();

builder.Services.AddSingleton<IReaderRepository, ReaderRepository>();
builder.Services.AddSingleton<IReaderService, ReaderService>();

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<ReaderController>();
app.MapGet("/", () => "RateLimiter Reader Service is running");

await app.RunAsync("http://*:5000");
