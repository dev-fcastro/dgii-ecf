using System.Text.Json;
using Extractor = DgiiEcf.ExternalServices.Http.DgiiErrorMessageExtractor.DgiiErrorMessageExtractor;

namespace DgiiEcf.ExternalServices.Tests.Http.DgiiErrorMessageExtractor;

public sealed class DgiiErrorMessageExtractorTests
{
    private const string DelegationError = "El RNC 00112233445 del certificado no está delegado para realizar transaciones.";

    [Fact]
    public void Extract_BareString_ReturnsIt()
    {
        Assert.Equal(DelegationError, Extractor.Extract(DelegationError));
    }

    [Fact]
    public void Extract_JsonEncodedString_UnwrapsIt()
    {
        Assert.Equal(DelegationError, Extractor.Extract(JsonSerializer.Serialize(DelegationError)));
    }

    [Fact]
    public void Extract_JsonBodyAsString_ReadsMensaje()
    {
        Assert.Equal(DelegationError, Extractor.Extract(JsonSerializer.Serialize(new { mensaje = DelegationError })));
    }

    [Fact]
    public void Extract_Mensajes_JoinsThem()
    {
        var body = """{"mensajes":[{"valor":"Fecha de emisión inválida","codigo":2},{"valor":"e-NCF duplicado","codigo":3}]}""";

        Assert.Equal("Fecha de emisión inválida | e-NCF duplicado", Extractor.Extract(body));
    }

    [Fact]
    public void Extract_MensajesAsStrings_ReturnsThem()
    {
        Assert.Equal("Rango no disponible", Extractor.Extract("""{"mensajes":["Rango no disponible"]}"""));
    }

    [Theory]
    [InlineData("""{"Message":"Token expirado"}""", "Token expirado")]
    [InlineData("""{"error":"invalid_grant"}""", "invalid_grant")]
    [InlineData("""{"title":"One or more validation errors occurred.","errors":{"xml":["El campo xml es requerido."]}}""", "One or more validation errors occurred.")]
    [InlineData("""{"errors":{"xml":["El campo xml es requerido."]}}""", "xml: El campo xml es requerido.")]
    public void Extract_AspNetShapes_ReturnsTheDescription(string body, string expected)
    {
        Assert.Equal(expected, Extractor.Extract(body));
    }

    [Fact]
    public void Extract_HtmlErrorPage_ReturnsItsText()
    {
        const string html = "<html><head><title>500</title></head><body><h1>Error del servidor</h1></body></html>";

        Assert.Equal("500 Error del servidor", Extractor.Extract(html));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("{}")]
    public void Extract_NothingToReport_ReturnsNull(string? body)
    {
        Assert.Null(Extractor.Extract(body));
    }

    [Fact]
    public void Extract_LongBody_IsTruncated()
    {
        var message = Extractor.Extract(new string('x', 1500));

        Assert.Equal(1001, message!.Length);
        Assert.EndsWith("…", message);
    }

    [Fact]
    public void ReadMessages_KeepsValorAndCodigo()
    {
        var messages = Extractor.ReadMessages("""{"mensajes":[{"valor":"e-NCF vencido","codigo":4}]}""");

        var message = Assert.Single(messages!);
        Assert.Equal("e-NCF vencido", message.Valor);
        Assert.Equal(4, message.Codigo);
    }
}
