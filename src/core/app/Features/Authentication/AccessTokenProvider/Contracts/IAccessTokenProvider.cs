using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;

/// <summary>
/// Returns a valid token for DGII or for a receiver, authenticating again when it is missing or about to expire.
/// </summary>
public interface IAccessTokenProvider
{
    Task<Result<string>> GetTokenAsync(string? buyerHost, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="call"/> with a valid token. When the service answers 401 the cached token is
    /// discarded and the call is retried once with a fresh token.
    /// </summary>
    Task<Result<T>> ExecuteAsync<T>(
        string? buyerHost,
        Func<string, CancellationToken, Task<Result<T>>> call,
        CancellationToken cancellationToken = default);
}
