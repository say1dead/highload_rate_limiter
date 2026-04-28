using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace UserService.Interceptors;

public class Auth : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        if (context == null)
            throw new RpcException(new Status(StatusCode.Internal, "ServerCallContext is null"));

        var userIdHeader = context.RequestHeaders.FirstOrDefault(h => h.Key == "user_id");

        if (userIdHeader == null || string.IsNullOrWhiteSpace(userIdHeader.Value))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "missing user_id header"));

        return await continuation(request, context);
    }

}
