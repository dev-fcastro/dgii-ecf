using System.Xml;
using DgiiEcf.DependencyInjection.DgiiEcfDependencyInjection;
using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;
using DgiiEcf.ExternalServices.Http.DgiiHttpTransport;
using DgiiEcf.Facades.DgiiEcfClient;
using DgiiEcf.Facades.EcfReceiver;
using DgiiEcf.Tests.Support;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DgiiEcf.Tests.DependencyInjection;

public sealed class DgiiEcfDependencyInjectionTests
{
    private const string Seed =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?><SemillaModel xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><valor>abc</valor><fecha>2026-08-17T08:00:00-04:00</fecha></SemillaModel>";

    private static ServiceProvider BuildProvider(StubHttpMessageHandler handler)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DgiiEcf:Environment"] = "Certification",
                ["DgiiEcf:Certificate:CertificatePath"] = TestEnvironment.SamplePath("SAMPLE_CERT.p12"),
                ["DgiiEcf:Certificate:CertificatePassword"] = TestEnvironment.SampleCertificatePassword,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddDgiiEcf(configuration);
        services.AddHttpClient<DgiiHttpTransport>().ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    [Fact]
    public async Task SendElectronicDocumentAsync_AuthenticatesOnceAndSendsWithTheBearerToken()
    {
        var handler = new StubHttpMessageHandler((request, _) => request.RequestUri!.AbsolutePath switch
        {
            "/CerteCF/Autenticacion/api/Autenticacion/Semilla" => StubHttpMessageHandler.Xml(Seed),
            "/CerteCF/autenticacion/api/Autenticacion/ValidarSemilla" =>
                StubHttpMessageHandler.Json("""{"token":"dgii-token","expira":"2999-01-01T00:00:00Z","expedido":"2026-01-01T00:00:00Z"}"""),
            "/CerteCF/recepcion/api/FacturasElectronicas" => StubHttpMessageHandler.Json("""{"trackId":"track-1"}"""),
            _ => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound),
        });
        using var provider = BuildProvider(handler);
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IDgiiEcfClient>();

        var signed = client.Sign("<ECF><Encabezado><Emisor><RNCEmisor>101672919</RNCEmisor></Emisor><IdDoc><eNCF>E310000000001</eNCF></IdDoc></Encabezado></ECF>", "ECF");
        var first = await client.SendElectronicDocumentAsync(signed.Value);
        var second = await client.SendElectronicDocumentAsync(signed.Value);

        Assert.True(first.IsSuccess, first.Error.Message);
        Assert.Equal("track-1", second.Value.TrackId);
        Assert.Equal(4, handler.Requests.Count);

        var validation = handler.Requests[1];
        Assert.Contains("<DigestValue>", validation.Body);
        Assert.Contains("filename=\"signed.xml\"", validation.Body);

        var send = handler.Requests[2];
        Assert.Equal("Bearer dgii-token", send.Authorization);
        Assert.Contains("filename=\"101672919E310000000001.xml\"", send.Body);
        Assert.Equal("Bearer dgii-token", handler.Requests[3].Authorization);
    }

    [Fact]
    public void Receiver_SignedSeedFromAnIssuer_GetsATokenItCanValidate()
    {
        using var provider = BuildProvider(new StubHttpMessageHandler((_, _) => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)));
        using var scope = provider.CreateScope();
        var receiver = scope.ServiceProvider.GetRequiredService<IEcfReceiver>();
        var issuer = scope.ServiceProvider.GetRequiredService<IDgiiEcfClient>();

        var signedSeed = issuer.Sign(receiver.GenerateSeed(), "SemillaModel").Value;
        var token = receiver.ValidateSignedSeed(signedSeed);
        var validation = receiver.ValidateToken(token.Value.Token);

        Assert.True(token.IsSuccess, token.Error.Message);
        Assert.False(validation.Value.IsExpired);
        Assert.False(string.IsNullOrEmpty(validation.Value.Valor));
    }

    [Fact]
    public void Receiver_TamperedSignedSeed_IsRejected()
    {
        using var provider = BuildProvider(new StubHttpMessageHandler((_, _) => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)));
        using var scope = provider.CreateScope();
        var receiver = scope.ServiceProvider.GetRequiredService<IEcfReceiver>();
        var signedSeed = scope.ServiceProvider.GetRequiredService<IDgiiEcfClient>().Sign(receiver.GenerateSeed(), "SemillaModel").Value;

        var document = new XmlDocument();
        document.LoadXml(signedSeed);
        document.GetElementsByTagName("valor")[0]!.InnerText = "otro-valor";

        Assert.Equal("receiver.invalid_seed_signature", receiver.ValidateSignedSeed(document.OuterXml).Error.Code);
    }

    [Fact]
    public void Receiver_BuildSignedReceiptAcknowledgement_ReturnsSignedArecf()
    {
        using var provider = BuildProvider(new StubHttpMessageHandler((_, _) => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)));
        using var scope = provider.CreateScope();
        var receiver = scope.ServiceProvider.GetRequiredService<IEcfReceiver>();

        var arecf = receiver.BuildSignedReceiptAcknowledgement(
            "<ECF><TipoeCF>31</TipoeCF><eNCF>E310000000001</eNCF><RNCEmisor>1</RNCEmisor><RNCComprador>130862346</RNCComprador></ECF>",
            "130862346");

        Assert.True(arecf.IsSuccess, arecf.Error.Message);
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?><ARECF><DetalleAcusedeRecibo>", arecf.Value);
        Assert.Contains("<Estado>0</Estado>", arecf.Value);
        Assert.Contains("<SignatureValue>", arecf.Value);
    }

    [Fact]
    public void Create_Standalone_ResolvesClientAndReceiver()
    {
        using var standalone = DgiiEcfClient.Create(options =>
        {
            options.Environment = DgiiEnvironment.Test;
            options.CertificatePath = TestEnvironment.SamplePath("SAMPLE_CERT.p12");
            options.CertificatePassword = TestEnvironment.SampleCertificatePassword;
        });

        var signed = standalone.Client.Sign("<Doc><A>1</A></Doc>");
        Assert.True(signed.IsSuccess, signed.Error.Message);
        Assert.False(string.IsNullOrEmpty(standalone.Receiver.GenerateSeed()));
    }

    [Fact]
    public void AddDgiiEcf_WithoutCertificate_FailsOptionsValidation()
    {
        var services = new ServiceCollection();
        services.AddDgiiEcf(options => options.Environment = DgiiEnvironment.Test);
        using var provider = services.BuildServiceProvider();

        Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(() =>
            provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Signing.Options.SigningOptions.SigningOptions>>().Value);
    }
}
