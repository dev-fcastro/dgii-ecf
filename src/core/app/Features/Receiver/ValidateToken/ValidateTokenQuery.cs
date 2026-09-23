namespace DgiiEcf.Application.Features.Receiver.ValidateToken;

/// <summary>
/// Validates a token previously issued by ValidateSignedSeed (send it as <c>Authorization: Bearer ...</c>).
/// </summary>
public sealed record ValidateTokenQuery(string Token);

/// <summary>
/// Decoded receiver token. An expired token is still returned with <see cref="IsExpired"/> set.
/// </summary>
public sealed record ReceiverTokenInfo(string? Valor, string? Timestamp, DateTimeOffset? IssuedAt, DateTimeOffset? ExpiresAt, bool IsExpired);
