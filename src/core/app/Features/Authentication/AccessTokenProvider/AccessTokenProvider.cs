using DgiiEcf.Application.Common.Contracts.IAccessTokenStore;
using DgiiEcf.Application.Common.Errors.DgiiApiError;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider.Contracts;
using DgiiEcf.Application.Features.Authentication.Authenticate;
using DgiiEcf.Application.Features.Authentication.Authenticate.Contracts;
using DgiiEcf.Application.Features.Authentication.Common;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Authentication.AccessTokenProvider;

public sealed class AccessTokenProvider : IAccessTokenProvider
{
    private const int Unauthorized = 401;
    private static readonly TimeSpan RenewalMargin = TimeSpan.FromMinutes(1);

    private readonly IAccessTokenStore _tokenStore;
    private readonly IAuthenticateHandler _authenticateHandler;
    private readonly TimeProvider _timeProvider;

    public AccessTokenProvider(IAccessTokenStore tokenStore, IAuthenticateHandler authenticateHandler, TimeProvider timeProvider)
    {
        _tokenStore = tokenStore;
        _authenticateHandler = authenticateHandler;
        _timeProvider = timeProvider;
    }

    public async Task<Result<string>> GetTokenAsync(string? buyerHost, CancellationToken cancellationToken = default)
    {
        var stored = _tokenStore.Get(AccessTokenAudiences.For(buyerHost));
        if (stored is not null && stored.ExpiresAt - RenewalMargin > _timeProvider.GetUtcNow())
        {
            return stored.Token;
        }

        var authenticated = await _authenticateHandler.HandleAsync(new AuthenticateCommand(buyerHost), cancellationToken);
        return authenticated.IsSuccess ? authenticated.Value.Token : authenticated.Error;
    }

    public async Task<Result<T>> ExecuteAsync<T>(
        string? buyerHost,
        Func<string, CancellationToken, Task<Result<T>>> call,
        CancellationToken cancellationToken = default)
    {
        var token = await GetTokenAsync(buyerHost, cancellationToken);
        if (token.IsFailure)
        {
            return token.Error;
        }

        var result = await call(token.Value, cancellationToken);
        if (result.IsSuccess || result.Error is not DgiiApiError { Status: Unauthorized })
        {
            return result;
        }

        _tokenStore.Remove(AccessTokenAudiences.For(buyerHost));
        var renewed = await GetTokenAsync(buyerHost, cancellationToken);
        return renewed.IsFailure ? renewed.Error : await call(renewed.Value, cancellationToken);
    }
}
