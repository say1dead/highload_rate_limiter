using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using StackExchange.Redis;

namespace UserService.Interceptors;

public class RateLimit : Interceptor
{
    private readonly IConnectionMultiplexer _redis;

    public RateLimit(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var userIdHeader = context.RequestHeaders.FirstOrDefault(h => h.Key == "user_id");
        if (userIdHeader == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "missing user_id header"));
        }

        var userId = userIdHeader.Value;
        var endpoint = context.Method.Split('/').Last().ToLower();
        var key = $"blocked:{userId}:{endpoint}";

        var db = _redis.GetDatabase();

        if (await db.KeyExistsAsync(key))
        {
            throw new RpcException(new Status(StatusCode.ResourceExhausted,
                $"rate blocked for endpoint {endpoint}"));
        }

        return await continuation(request, context);
    }

}

