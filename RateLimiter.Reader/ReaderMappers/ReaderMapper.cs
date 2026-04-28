using RateLimiter.Reader.Models.Domain;
using RateLimiter.Reader.Models.DbModel;
using Riok.Mapperly.Abstractions;

namespace RateLimiter.Reader.Mappers;

[Mapper]
public static partial class ReaderMapper
{
    public static partial RateLimit MapToDomain(RateLimitDb document);

    public static partial IReadOnlyCollection<RateLimit> MapToDomain(IEnumerable<RateLimitDb> documents);

    private static string MapId(string? id)
        => id ?? string.Empty;
}