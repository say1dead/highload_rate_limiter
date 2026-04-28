using Riok.Mapperly.Abstractions;
using RateLimiter.Writer.Domain;
using RateLimiter.Writer.DbModels;

namespace RateLimiter.Writer.Mappers;

[Mapper]
public partial class RateLimitDbMapper
{
    public partial RateLimitDb ToDb(RateLimit domain);
    public partial RateLimit ToDomain(RateLimitDb db);
}
