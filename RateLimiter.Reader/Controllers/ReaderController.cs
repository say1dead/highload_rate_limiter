using Grpc.Core;
using RateLimiter.Reader.Grpc;
using RateLimiter.Reader.Services;

namespace RateLimiter.Reader.Controllers;

public class ReaderController : Grpc.Reader.ReaderBase
{
    private readonly IReaderService _limitService;
    private readonly ILogger<ReaderController> _logger;

    public ReaderController(IReaderService limitService, ILogger<ReaderController> logger)
    {
        _limitService = limitService;
        _logger = logger;
    }

    public override Task<GetAllLimitsResponse> GetAllLimits(GetAllLimitsRequest request, ServerCallContext context)
    {
        _logger.LogDebug("GetAllLimits method called");
        
        try
        {
            var limits = _limitService.GetAllLimits();
            var response = new GetAllLimitsResponse();

            foreach (var limit in limits.Values)
            {
                response.Limits.Add(new Grpc.RateLimit
                {
                    Route = limit.Route,
                    RequestsPerMinute = limit.RequestsPerMinute,
                });
            }

            _logger.LogDebug("Returning {Count} limits", response.Limits.Count);
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllLimits method");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}