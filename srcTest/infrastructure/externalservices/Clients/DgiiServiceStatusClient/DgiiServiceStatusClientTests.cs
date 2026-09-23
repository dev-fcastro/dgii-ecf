using System.Net;
using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;
using DgiiEcf.ExternalServices.Tests.Support;

namespace DgiiEcf.ExternalServices.Tests.Clients.DgiiServiceStatusClient;

public sealed class DgiiServiceStatusClientTests
{
    [Fact]
    public async Task VerifyStatusAsync_SendsTheApiKeyAndEnvironmentCode()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"estado":"Disponible"}""");
        var client = new ExternalServices.Clients.DgiiServiceStatusClient.DgiiServiceStatusClient(
            ClientFactory.Transport(handler),
            ClientFactory.Options(DgiiEnvironment.Certification));

        var result = await client.VerifyStatusAsync("api-key", CancellationToken.None);

        Assert.Equal("Disponible", result.Value.Estado);
        var request = handler.Requests.Single();
        Assert.Equal("api-key", request.Authorization);
        Assert.Equal("https://statusecf.dgii.gov.do/api/estatusservicios/verificarestado?ambiente=3", request.Uri.ToString());
    }

    [Fact]
    public async Task GetServicesStatusAsync_ReturnsEveryService()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            """[{"servicio":"Autenticación","estatus":"Disponible","ambiente":"TesteCF"},{"servicio":"Recepción","estatus":"No Disponible","ambiente":"TesteCF"}]""");
        var client = new ExternalServices.Clients.DgiiServiceStatusClient.DgiiServiceStatusClient(ClientFactory.Transport(handler), ClientFactory.Options());

        var result = await client.GetServicesStatusAsync("api-key", CancellationToken.None);

        Assert.Equal(2, result.Value.Count);
        Assert.Equal("https://statusecf.dgii.gov.do/api/estatusservicios/obtenerestatus", handler.Requests.Single().Uri.ToString());
    }
}
