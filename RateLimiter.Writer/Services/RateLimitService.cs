using FluentValidation;
using RateLimiter.Writer.Repository;
using RateLimiter.Writer.Domain;
using Writer.Services;
using Writer.Repositories;
using RateLimiter.Writer.Exceptions;

namespace RateLimiter.Writer.Services;

public class RateLimitService : IRateLimitService
{
    private readonly IRateLimitRepository _repository;

    private readonly IValidator<RateLimit> _validator;

    public RateLimitService(IRateLimitRepository repository, IValidator<RateLimit> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<RateLimit> CreateAsync(RateLimit limit, CancellationToken ct)
    {
        _validator.ValidateAndThrow(limit);

        var existing = await _repository.GetByRouteAsync(limit.Route, ct);

        if (existing is not null)
        {
             throw new AlreadyExistsException("rate limit for route already exists");
        }

        return await _repository.CreateAsync(limit, ct);
    }

    public async  Task<RateLimit?> GetByRouteAsync(string route, CancellationToken ct)
    {
        var limit = await _repository.GetByRouteAsync(route, ct);
        if (limit is null)
            throw new NotFoundException("rate limit for route not found");

        return limit;
    }

    public async Task<RateLimit?> UpdateAsync(RateLimit limit, CancellationToken ct)
    {
        _validator.ValidateAndThrow(limit);

        var updated = await _repository.UpdateAsync(limit, ct);
        if (updated is null)
            throw new NotFoundException("rate limit for route not found");

        return updated;
    }

    public async Task<bool> DeleteAsync(string route, CancellationToken ct)
    {
        var deleted = await _repository.DeleteAsync(route, ct);
        if (!deleted)
            throw new NotFoundException("rate limit for route not found");

        return true;
    }
}