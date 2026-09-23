using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Contracts.IJwtTokenService;

/// <summary>
/// Issues and validates RS256 JWTs signed with the taxpayer certificate (receiver authentication).
/// </summary>
public interface IJwtTokenService
{
    Result<string> Issue(IReadOnlyDictionary<string, object> claims, TimeSpan lifetime);

    /// <summary>
    /// Validates the signature and algorithm. Expired tokens are returned as valid with
    /// <see cref="JwtValidation.IsExpired"/> set so the caller decides.
    /// </summary>
    Result<JwtValidation> Validate(string token);
}

public sealed record JwtValidation(
    IReadOnlyDictionary<string, string> Claims,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? ExpiresAt,
    bool IsExpired);
