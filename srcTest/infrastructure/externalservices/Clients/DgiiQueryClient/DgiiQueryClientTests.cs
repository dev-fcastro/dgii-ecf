using System.Net;
using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;
using DgiiEcf.Domain.Tracking.TrackStatus;
using DgiiEcf.ExternalServices.Tests.Support;

namespace DgiiEcf.ExternalServices.Tests.Clients.DgiiQueryClient;

public sealed class DgiiQueryClientTests
{
    private const string DirectoryEntry =
        """{"nombre":"DGII","rnc":"131880681","urlRecepcion":"https://ecf.dgii.gov.do/testecf/emisorreceptor","urlAceptacion":"https://ecf.dgii.gov.do/testecf/emisorreceptor","urlOpcional":"https://ecf.dgii.gov.do/Testecf/autenticacion"}""";

    private static ExternalServices.Clients.DgiiQueryClient.DgiiQueryClient Create(StubHttpMessageHandler handler, DgiiEnvironment environment = DgiiEnvironment.Test) =>
        new(ClientFactory.Transport(handler), ClientFactory.Options(environment));

    [Fact]
    public async Task GetCustomerDirectoryAsync_InTest_UsesListadoAndKeepsTheArray()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, $"[{DirectoryEntry}]");

        var result = await Create(handler).GetCustomerDirectoryAsync("131880681", "t", CancellationToken.None);

        Assert.Equal("131880681", Assert.Single(result.Value).Rnc);
        Assert.Equal(
            "https://ecf.dgii.gov.do/TesteCF/consultadirectorio/api/consultas/listado?rnc=131880681",
            handler.Requests.Single().Uri.ToString());
    }

    [Fact]
    public async Task GetCustomerDirectoryAsync_InProduction_WrapsTheSingleEntry()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, DirectoryEntry);

        var result = await Create(handler, DgiiEnvironment.Production).GetCustomerDirectoryAsync("131880681", "t", CancellationToken.None);

        Assert.Equal("https://ecf.dgii.gov.do/testecf/emisorreceptor", Assert.Single(result.Value).UrlRecepcion);
        Assert.Equal(
            "https://ecf.dgii.gov.do/eCF/consultadirectorio/api/consultas/obtenerdirectorioporrnc?rnc=131880681",
            handler.Requests.Single().Uri.ToString());
    }

    [Fact]
    public async Task GetCustomerDirectoryAsync_WhenBodyIsNull_ReturnsEmpty()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "null");

        var result = await Create(handler).GetCustomerDirectoryAsync("131880681", "t", CancellationToken.None);

        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetTrackStatusAsync_MapsTheStatus()
    {
        var handler = new StubHttpMessageHandler(
            HttpStatusCode.OK,
            """{"trackId":"t-1","codigo":1,"estado":"Aceptado","rnc":"130862346","encf":"E310000000001","secuenciaUtilizada":true,"fechaRecepcion":"1/1/2026","mensajes":[{"valor":"","codigo":0}]}""");

        var result = await Create(handler).GetTrackStatusAsync("t-1", "tok", CancellationToken.None);

        Assert.Equal(TrackStatus.Accepted, result.Value.Status);
        Assert.Equal("https://ecf.dgii.gov.do/TesteCF/consultaresultado/api/Consultas/Estado?trackId=t-1", handler.Requests.Single().Uri.ToString());
        Assert.Equal("Bearer tok", handler.Requests.Single().Authorization);
    }

    [Fact]
    public async Task InquiryStatusAsync_OmitsMissingParameters()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"codigo":1,"estado":"Aceptado","montoTotal":"180000.00"}""");

        var result = await Create(handler).InquiryStatusAsync("130862346", "E320000000001", null, "BucMq7", "t", CancellationToken.None);

        Assert.Equal(180000.00m, result.Value.MontoTotal);
        Assert.Equal(
            "https://ecf.dgii.gov.do/TesteCF/consultaestado/api/Consultas/Estado?rncEmisor=130862346&ncfElectronico=E320000000001&codigoSeguridad=BucMq7",
            handler.Requests.Single().Uri.ToString());
    }

    [Fact]
    public async Task GetSummaryInvoiceInquiryAsync_UsesTheFcHost()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"rnc":"1","encf":"E32","secuenciaUtilizada":true,"codigo":1,"estado":"Aceptado"}""");

        await Create(handler, DgiiEnvironment.Production).GetSummaryInvoiceInquiryAsync("130862346", "E320000000001", "m+tPLr", "t", CancellationToken.None);

        Assert.Equal(
            "https://fc.dgii.gov.do/eCF/consultarfce/api/Consultas/Consulta?rnc_emisor=130862346&encf=E320000000001&cod_seguridad_eCF=m%2BtPLr",
            handler.Requests.Single().Uri.ToString());
    }
}
