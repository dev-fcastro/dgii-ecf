using DgiiEcf.Application.Common.Contracts.IAccessTokenStore;
using DgiiEcf.Application.Common.Contracts.IDgiiAuthenticationClient;
using DgiiEcf.Application.Common.Contracts.IXmlDocumentSigner;
using DgiiEcf.Application.Common.Responses.AccessToken;
using DgiiEcf.Application.Features.Authentication.Authenticate.Contracts;
using DgiiEcf.Application.Features.Authentication.Common;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Authentication.Authenticate;

/// <summary>
/// Seed flow: request the seed, sign it as <c>SemillaModel</c>, exchange it for a token and keep the token.
/// </summary>
public sealed class AuthenticateHandler : IAuthenticateHandler
{
    private const string SeedRootElement = "SemillaModel";

    private readonly IDgiiAuthenticationClient _authenticationClient;
    private readonly IXmlDocumentSigner _signer;
    private readonly IAccessTokenStore _tokenStore;
    private readonly TimeProvider _timeProvider;

    public AuthenticateHandler(
        IDgiiAuthenticationClient authenticationClient,
        IXmlDocumentSigner signer,
        IAccessTokenStore tokenStore,
        TimeProvider timeProvider)
    {
        _authenticationClient = authenticationClient;
        _signer = signer;
        _tokenStore = tokenStore;
        _timeProvider = timeProvider;
    }

    public async Task<Result<AccessToken>> HandleAsync(AuthenticateCommand command, CancellationToken cancellationToken = default)
    {
        var seed = await _authenticationClient.GetSeedAsync(command.BuyerHost, cancellationToken);
        if (seed.IsFailure)
        {
            return seed.Error;
        }

        if (string.IsNullOrWhiteSpace(seed.Value))
        {
            return AuthenticateErrors.EmptySeed;
        }

        var signedSeed = _signer.Sign(seed.Value, SeedRootElement);
        if (signedSeed.IsFailure)
        {
            return signedSeed.Error;
        }

        var token = await _authenticationClient.ValidateSeedAsync(signedSeed.Value, command.BuyerHost, cancellationToken);
        if (token.IsFailure)
        {
            return token.Error;
        }

        if (string.IsNullOrWhiteSpace(token.Value.Token))
        {
            return AuthenticateErrors.EmptyToken;
        }

        var expiresAt = token.Value.ResolveExpiration(_timeProvider.GetUtcNow());
        _tokenStore.Set(AccessTokenAudiences.For(command.BuyerHost), new StoredAccessToken(token.Value.Token, expiresAt));

        return token.Value;
    }
}
