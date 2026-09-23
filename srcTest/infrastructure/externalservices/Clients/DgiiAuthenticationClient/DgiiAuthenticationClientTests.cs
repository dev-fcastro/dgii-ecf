using System.Net;
using DgiiEcf.Application.Common.Errors.DgiiApiError;
using DgiiEcf.ExternalServices.Http.DgiiHttpTransport;
using DgiiEcf.ExternalServices.Tests.Support;

namespace DgiiEcf.ExternalServices.Tests.Clients.DgiiAuthenticationClient;

public sealed class DgiiAuthenticationClientTests
{
    private const string DelegationError = "El RNC 00112233445 del certificado no está delegado para realizar transaciones.";

    private static ExternalServices.Clients.DgiiAuthenticationClient.DgiiAuthenticationClient Create(StubHttpMessageHandler handler) =>
        new(ClientFactory.Transport(handler), ClientFactory.Options());

    [Fact]
    public async Task GetSeedAsync_ReturnsTheSeedXml()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "<SemillaModel><valor>x</valor></SemillaModel>", "application/xml");

        var result = await Create(handler).GetSeedAsync(null, CancellationToken.None);

        Assert.Equal("<SemillaModel><valor>x</valor></SemillaModel>", result.Value);
        Assert.Equal("https://ecf.dgii.gov.do/TesteCF/Autenticacion/api/Autenticacion/Semilla", handler.Requests.Single().Uri.ToString());
    }

    [Fact]
    public async Task GetSeedAsync_WithBuyerHost_UsesTheReceiverResource()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "<SemillaModel />", "application/xml");

        await Create(handler).GetSeedAsync("https://ecf.dgii.gov.do/Testecf/autenticacion", CancellationToken.None);

        Assert.Equal("https://ecf.dgii.gov.do/Testecf/autenticacion/fe/autenticacion/api/semilla", handler.Requests.Single().Uri.ToString());
    }

    [Fact]
    public async Task ValidateSeedAsync_ReturnsTheToken()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"token":"abc","expira":"2026-08-17T13:00:00Z","expedido":"2026-08-17T12:00:00Z"}""");

        var result = await Create(handler).ValidateSeedAsync("<SemillaModel />", null, CancellationToken.None);

        Assert.Equal("abc", result.Value.Token);
        var request = handler.Requests.Single();
        Assert.Equal("https://ecf.dgii.gov.do/TesteCF/autenticacion/api/Autenticacion/ValidarSemilla", request.Uri.ToString());
        Assert.Contains("filename=\"signed.xml\"", request.Body);
        Assert.Null(request.Authorization);
    }

    [Fact]
    public async Task ValidateSeedAsync_WhenDgiiAnswersAString_ReportsTheDgiiText()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.BadRequest, DelegationError, "text/plain");

        var result = await Create(handler).ValidateSeedAsync("<SemillaModel />", null, CancellationToken.None);

        var error = Assert.IsType<DgiiApiError>(result.Error);
        Assert.Equal(DelegationError, error.Message);
        Assert.Equal(400, error.Status);
        Assert.Equal(DelegationError, error.RawBody);
        Assert.Equal("/TesteCF/autenticacion/api/Autenticacion/ValidarSemilla", error.Resource);
    }

    [Fact]
    public async Task GetSeedAsync_WhenServerFails_ReportsTheDgiiText()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.InternalServerError, "Servicio de autenticación no disponible", "text/plain");

        var result = await Create(handler).GetSeedAsync(null, CancellationToken.None);

        Assert.Equal("Servicio de autenticación no disponible", result.Error.Message);
    }

    [Fact]
    public async Task ValidateSeedAsync_WhenBodyIsEmpty_FallsBackToTheStatus()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(string.Empty) });

        var result = await Create(handler).ValidateSeedAsync("<SemillaModel />", null, CancellationToken.None);

        Assert.StartsWith("DGII request failed with status code 400", result.Error.Message);
    }

    [Fact]
    public async Task GetSeedAsync_WhenUnauthorizedWithoutBody_UsesTheCredentialsHint()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(string.Empty) });

        var result = await Create(handler).GetSeedAsync(null, CancellationToken.None);

        Assert.Equal(DgiiHttpTransport.UnauthorizedFallback, result.Error.Message);
    }

    [Fact]
    public async Task GetSeedAsync_WhenUnauthorizedWithBody_PrefersTheDgiiText()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.Unauthorized, "Token vencido", "text/plain");

        var result = await Create(handler).GetSeedAsync(null, CancellationToken.None);

        Assert.Equal("Token vencido", result.Error.Message);
    }

    [Fact]
    public async Task GetSeedAsync_WhenTimingOut_ReturnsTimeoutError()
    {
        var handler = new StubHttpMessageHandler(_ => throw new TaskCanceledException("timeout"));

        var result = await Create(handler).GetSeedAsync(null, CancellationToken.None);

        var error = Assert.IsType<DgiiApiError>(result.Error);
        Assert.Equal(DgiiApiError.TimeoutCodeValue, error.Code);
        Assert.Equal("ECONNABORTED", error.TransportCode);
        Assert.Null(error.Status);
    }
}
