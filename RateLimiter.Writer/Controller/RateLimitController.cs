using Grpc.Core;
using RateLimiter.Writer.Protos;
using Writer.Services;
using FluentValidation;
using RateLimiter.Writer.Mappers;
using RateLimiter.Writer.Exceptions;

namespace RateLimiter.Writer.Controller;

public class RateLimitController : RateLimiterWriter.RateLimiterWriterBase
{
    private readonly IRateLimitService _service;

    private readonly RateLimitProtoMapper _mapper;

    public RateLimitController(IRateLimitService service, RateLimitProtoMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public override async Task<CreateLimitResponse> CreateLimit(CreateLimitRequest request, ServerCallContext context)
    {
        try
        {
            var domain = _mapper.ToDomain(request);
            var created = await _service.CreateAsync(domain, context.CancellationToken);
            return new CreateLimitResponse { Limit = _mapper.ToProto(created) };
        }
        catch (FluentValidation.ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (AlreadyExistsException ex)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
    }

    public override async Task<GetLimitByRouteResponse> GetLimitByRoute(GetLimitByRouteRequets requets, ServerCallContext context)
    {
        try
        {
            var limit = await _service.GetByRouteAsync(requets.Route, context.CancellationToken);

            return new GetLimitByRouteResponse { Limit = _mapper.ToProto(limit) };
        }
        catch (NotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        
    }

    public override async Task<UpdateLimitResponse> UpdateLimit(UpdateLimitRequest request, ServerCallContext context)
    {
        try
        {
            var domain = _mapper.ToDomain(request);
            var updated = await _service.UpdateAsync(domain, context.CancellationToken);

            return new UpdateLimitResponse { Limit = _mapper.ToProto(updated) };
        }
        catch (FluentValidation.ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (NotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<DeleteLimitResponse> DeleteLimit(DeleteLimitRequest request, ServerCallContext context)
    {
        try
        {
            var deleted = await _service.DeleteAsync(request.Route, context.CancellationToken);
            return new DeleteLimitResponse { Success = true };
        }
        catch (NotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }
}