using System.Net;
using DgiiEcf.Application.Common.Errors.DgiiApiError;
using DgiiEcf.ExternalServices.Tests.Support;

namespace DgiiEcf.ExternalServices.Tests.Clients.DgiiReceptionClient;

public sealed class DgiiReceptionClientTests
{
    private const string SignedXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ECF><Signature /></ECF>";

    [Fact]
    public async Task SendElectronicDocumentAsync_PostsMultipartWithXmlFieldAndContentLength()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"trackId":"abc-123"}""");
        var client = new ExternalServices.Clients.DgiiReceptionClient.DgiiReceptionClient(ClientFactory.Transport(handler), ClientFactory.Options());

        var result = await client.SendElectronicDocumentAsync(SignedXml, "101672919E310000000001.xml", "token-1", null, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error.Message);
        Assert.Equal("abc-123", result.Value.TrackId);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://ecf.dgii.gov.do/TesteCF/recepcion/api/FacturasElectronicas", request.Uri.ToString());
        Assert.Equal("Bearer token-1", request.Authorization);
        Assert.StartsWith("multipart/form-data; boundary=", request.ContentType);
        Assert.True(request.ContentLength > 0);
        Assert.Contains("Content-Disposition: form-data; name=\"xml\"; filename=\"101672919E310000000001.xml\"", request.Body);
        Assert.Contains("Content-Type: application/xml", request.Body);
        Assert.Contains(SignedXml, request.Body);
    }

    [Fact]
    public async Task SendElectronicDocumentAsync_WithBuyerHost_PostsToTheReceiver()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "{}");
        var client = new ExternalServices.Clients.DgiiReceptionClient.DgiiReceptionClient(ClientFactory.Transport(handler), ClientFactory.Options());

        await client.SendElectronicDocumentAsync(SignedXml, "f.xml", "t", "https://ecf.dgii.gov.do/testecf/emisorreceptor/", CancellationToken.None);

        Assert.Equal("https://ecf.dgii.gov.do/testecf/emisorreceptor/fe/recepcion/api/ecf", handler.Requests.Single().Uri.ToString());
    }

    [Fact]
    public async Task SendSummaryAsync_PostsToTheFcHost()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"codigo":1,"estado":"Aceptado","encf":"E320000000001","secuenciaUtilizada":true}""");
        var client = new ExternalServices.Clients.DgiiReceptionClient.DgiiReceptionClient(ClientFactory.Transport(handler), ClientFactory.Options());

        var result = await client.SendSummaryAsync(SignedXml, "f.xml", "t", CancellationToken.None);

        Assert.Equal("Aceptado", result.Value.Estado);
        Assert.Equal("https://fc.dgii.gov.do/TesteCF/recepcionfc/api/recepcion/ecf", handler.Requests.Single().Uri.ToString());
    }

    [Fact]
    public async Task SendElectronicDocumentAsync_WhenDgiiReturnsMensajes_ExposesThem()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.BadRequest, """{"mensajes":[{"valor":"e-NCF duplicado","codigo":3}]}""");
        var client = new ExternalServices.Clients.DgiiReceptionClient.DgiiReceptionClient(ClientFactory.Transport(handler), ClientFactory.Options());

        var result = await client.SendElectronicDocumentAsync(SignedXml, "f.xml", "t", null, CancellationToken.None);

        var error = Assert.IsType<DgiiApiError>(result.Error);
        Assert.Equal("e-NCF duplicado", error.Message);
        Assert.Equal(400, error.Status);
        Assert.Equal("dgii.http_400", error.Code);
        var message = Assert.Single(error.Messages!);
        Assert.Equal(3, message.Codigo);
    }

    [Fact]
    public async Task VoidEncfAsync_PostsToAnularRango()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"rnc":"1","codigo":"1","nombre":"x","mensajes":["ok"]}""");
        var client = new ExternalServices.Clients.DgiiReceptionClient.DgiiReceptionClient(ClientFactory.Transport(handler), ClientFactory.Options(Domain.Common.Environment.DgiiEnvironment.DgiiEnvironment.Production));

        var result = await client.VoidEncfAsync(SignedXml, "f.xml", "t", CancellationToken.None);

        Assert.Equal("ok", Assert.Single(result.Value.Mensajes!));
        Assert.Equal("https://ecf.dgii.gov.do/eCF/anulacionrangos/api/operaciones/anularrango", handler.Requests.Single().Uri.ToString());
    }
}
