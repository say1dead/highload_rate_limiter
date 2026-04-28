using Riok.Mapperly.Abstractions;
using RateLimiter.Writer.Domain;
using RateLimiter.Writer.DbModels;
using RateLimiter.Writer.Protos;

namespace RateLimiter.Writer.Mappers;

[Mapper]
public partial class RateLimitMapper
{
    public partial RateLimitDb ToDb(RateLimit domain);

    public partial RateLimit ToDomain(RateLimitDb db);

    [MapperIgnoreTarget(nameof(RateLimit.Id))]
    public partial RateLimit ToDomain(CreateLimitRequest request);

    [MapperIgnoreTarget(nameof(RateLimit.Id))]
    public partial RateLimit ToDomain(UpdateLimitRequest request);
    
    public partial Limit ToProto(RateLimit domain);
}