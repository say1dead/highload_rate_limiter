using FluentValidation;
using Microsoft.Extensions.Options;
using Npgsql;
using UserService.Domain;
using UserService.Mappers;
using UserService.Options;
using UserService.Repositories;
using UserService.Services;
using UserService.Validators;
using UserService.Interceptors;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DBOptions>(builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddSingleton<NpgsqlDataSource>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<DBOptions>>().Value;
    return NpgsqlDataSource.Create(opts.Postgres);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(redisConnection);
});

builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<UserService.Services.UserService>();
builder.Services.AddSingleton<UserMapper>();
builder.Services.AddSingleton<UserDbMapper>();

builder.Services.AddSingleton<UserCreateValidator>();
builder.Services.AddSingleton<UserUpdateValidator>();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<Auth>();
    options.Interceptors.Add<RateLimit>();
});

var app = builder.Build();

app.MapGrpcService<UserService.Controllers.UserController>();

app.MapGet("/", () => "UserService is running...");

await app.RunAsync("http://*:5002");