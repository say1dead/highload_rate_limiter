using FluentValidation;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RateLimiter.Writer.Controller;
using RateLimiter.Writer.Domain;
using RateLimiter.Writer.DbModels;
using RateLimiter.Writer.Mappers;
using RateLimiter.Writer.Options;
using RateLimiter.Writer.Repository;
using RateLimiter.Writer.Services;
using RateLimiter.Writer.Validators;
using Writer.Repositories;
using Writer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("Mongo"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
    return new MongoClient(options.ConnectionString);
});

builder.Services.AddSingleton<RateLimitDbMapper>();
builder.Services.AddSingleton<RateLimitProtoMapper>();

builder.Services.AddSingleton<IMongoCollection<RateLimitDb>>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MongoOptions>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(options.DatabaseName);
    return database.GetCollection<RateLimitDb>(options.CollectionName);
});

builder.Services.AddSingleton<IRateLimitRepository, RateLimitRepository>();

builder.Services.AddSingleton<IRateLimitService, RateLimitService>();

builder.Services.AddSingleton<IValidator<RateLimit>, RateLimitValidator>();

builder.Services.AddGrpc();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IRateLimitRepository>();
    await repo.EnsureIndexesAsync(app.Lifetime.ApplicationStopping);
}

app.MapGrpcService<RateLimitController>();

await app.RunAsync("http://*:5001");
