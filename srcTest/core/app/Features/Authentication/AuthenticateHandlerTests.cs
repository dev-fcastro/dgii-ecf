using DgiiEcf.Application.Common.Contracts.IAccessTokenStore;
using DgiiEcf.Application.Common.Errors.DgiiApiError;
using DgiiEcf.Application.Common.Responses.AccessToken;
using DgiiEcf.Application.Common.Responses.InvoiceResponse;
using DgiiEcf.Application.Features.Authentication.AccessTokenProvider;
using DgiiEcf.Application.Features.Authentication.Authenticate;
using DgiiEcf.Application.Features.Reception.Common;
using DgiiEcf.Application.Features.Reception.SendElectronicDocument;
using DgiiEcf.Application.Tests.Support;

namespace DgiiEcf.Application.Tests.Features.Authentication;

public sealed class AuthenticateHandlerTests
{
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero));
    private readonly AuthenticationClientSpy _client = new();
    private readonly SignerSpy _signer = new();
    private readonly AccessTokenStoreSpy _store = new();

    private AuthenticateHandler CreateHandler() => new(_client, _signer, _store, _time);

    [Fact]
    public async Task HandleAsync_SignsTheSeedAsSemillaModelAndStoresTheToken()
    {
        var result = await CreateHandler().HandleAsync(new AuthenticateCommand());

        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.Equal("SemillaModel", Assert.Single(_signer.Calls).Root);
        Assert.Equal("signed:SemillaModel", Assert.Single(_client.Validations).SignedSeed);
        var stored = _store.Tokens["dgii"];
        Assert.Equal("token-1", stored.Token);
        Assert.Equal(new DateTimeOffset(2026, 8, 17, 13, 0, 0, TimeSpan.Zero), stored.ExpiresAt);
    }

    [Fact]
    public async Task HandleAsync_WithBuyerHost_StoresTheTokenUnderTheHost()
    {
        await CreateHandler().HandleAsync(new AuthenticateCommand("https://Receptor.example.com/"));

        Assert.Equal("https://Receptor.example.com/", Assert.Single(_client.SeedRequests));
        Assert.True(_store.Tokens.ContainsKey("https://receptor.example.com"));
    }

    [Fact]
    public async Task HandleAsync_WhenSeedFails_DoesNotSignNorStore()
    {
        _client.Seed = new DgiiApiError("dgii.http_500", "Servicio no disponible", Status: 500);

        var result = await CreateHandler().HandleAsync(new AuthenticateCommand());

        Assert.Equal("Servicio no disponible", result.Error.Message);
        Assert.Empty(_signer.Calls);
        Assert.Empty(_store.Tokens);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsEmpty_ReturnsEmptyToken()
    {
        _client.Token = new AccessToken(string.Empty, null, null);

        var result = await CreateHandler().HandleAsync(new AuthenticateCommand());

        Assert.Equal(AuthenticateErrors.EmptyToken, result.Error);
        Assert.Empty(_store.Tokens);
    }

    [Fact]
    public async Task AccessTokenProvider_WithValidCachedToken_DoesNotAuthenticate()
    {
        _store.Set("dgii", new StoredAccessToken("cached", _time.GetUtcNow().AddMinutes(30)));
        var provider = new AccessTokenProvider(_store, CreateHandler(), _time);

        var token = await provider.GetTokenAsync(null);

        Assert.Equal("cached", token.Value);
        Assert.Empty(_client.SeedRequests);
    }

    [Fact]
    public async Task AccessTokenProvider_WithTokenAboutToExpire_Authenticates()
    {
        _store.Set("dgii", new StoredAccessToken("old", _time.GetUtcNow().AddSeconds(30)));
        var provider = new AccessTokenProvider(_store, CreateHandler(), _time);

        var token = await provider.GetTokenAsync(null);

        Assert.Equal("token-1", token.Value);
    }

    [Fact]
    public async Task SendElectronicDocument_WhenDgiiAnswers401_RenewsTheTokenAndRetriesOnce()
    {
        _store.Set("dgii", new StoredAccessToken("stale", _time.GetUtcNow().AddMinutes(30)));
        var reception = new ReceptionClientSpy();
        reception.InvoiceResponses.Enqueue(new DgiiApiError("dgii.http_401", "Token vencido", Status: 401));
        reception.InvoiceResponses.Enqueue(new InvoiceResponse("track-9", null, null));
        var handler = new SendElectronicDocumentHandler(new AccessTokenProvider(_store, CreateHandler(), _time), reception);

        var result = await handler.HandleAsync(new SendElectronicDocumentCommand(
            "<ECF><RNCEmisor>101672919</RNCEmisor><eNCF>E310000000001</eNCF><SignatureValue>x</SignatureValue></ECF>"));

        Assert.Equal("track-9", result.Value.TrackId);
        Assert.Equal(["stale", "token-1"], reception.Sent.Select(sent => sent.Token));
        Assert.Equal("101672919E310000000001.xml", reception.Sent[0].FileName);
        Assert.Contains("dgii", _store.Removed);
    }

    [Fact]
    public async Task SendElectronicDocument_UnsignedDocument_IsRejectedBeforeCallingDgii()
    {
        var reception = new ReceptionClientSpy();
        var handler = new SendElectronicDocumentHandler(new AccessTokenProvider(_store, CreateHandler(), _time), reception);

        var result = await handler.HandleAsync(new SendElectronicDocumentCommand("<ECF><eNCF>E310000000001</eNCF></ECF>"));

        Assert.Equal(ReceptionErrors.UnsignedDocument, result.Error);
        Assert.Empty(reception.Sent);
        Assert.Empty(_client.SeedRequests);
    }
}
