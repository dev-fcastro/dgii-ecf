using System.Globalization;
using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;
using DgiiEcf.Domain.Documents.Ecf;
using DgiiEcf.Domain.Tracking.TrackStatus;
using DgiiEcf.Facades.DgiiEcfClient;
using DgiiEcf.Tests.Support;

namespace DgiiEcf.Tests.Integration;

/// <summary>
/// End-to-end checks against DGII TesteCF (port of the Node <c>ECF.test.ts</c>). They need a real taxpayer
/// certificate authorized in the DGII test environment.
/// </summary>
public sealed class DgiiTestEnvironmentTests
{
    private static StandaloneDgiiEcfClient CreateClient() => DgiiEcfClient.Create(options =>
    {
        options.Environment = DgiiEnvironment.Test;
        options.CertificatePath = TestEnvironment.CertificatePath;
        options.CertificatePassword = TestEnvironment.CertificatePassword;
    });

    [DgiiIntegrationFact]
    public async Task Authenticate_ReturnsToken()
    {
        using var standalone = CreateClient();

        var token = await standalone.Client.AuthenticateAsync();

        Assert.True(token.IsSuccess, token.Error.Message);
        Assert.False(string.IsNullOrEmpty(token.Value.Token));
    }

    [DgiiIntegrationFact]
    public async Task SendEcf31_ThenTrackStatus()
    {
        using var standalone = CreateClient();
        var ecf = BuildEcf31(TestEnvironment.RncEmisor!);
        var signed = standalone.Client.SignDocument(ecf);
        Assert.True(signed.IsSuccess, signed.Error.Message);

        var sent = await standalone.Client.SendElectronicDocumentAsync(signed.Value);
        Assert.True(sent.IsSuccess, sent.Error.Message);
        Assert.False(string.IsNullOrEmpty(sent.Value.TrackId));

        var status = await standalone.Client.GetTrackStatusAsync(sent.Value.TrackId!);
        Assert.True(status.IsSuccess, status.Error.Message);
        Assert.Contains(status.Value.Status, new[] { TrackStatus.Accepted, TrackStatus.Rejected, TrackStatus.InProcess, TrackStatus.ConditionallyAccepted });
    }

    [DgiiIntegrationFact]
    public async Task GetCustomerDirectory_ReturnsEntries()
    {
        using var standalone = CreateClient();

        var directory = await standalone.Client.GetCustomerDirectoryAsync(TestEnvironment.RncEmisor!);

        Assert.True(directory.IsSuccess, directory.Error.Message);
    }

    private static Ecf BuildEcf31(string rncEmisor)
    {
        var sequence = DateTime.UtcNow.ToString("HHmmssfff", CultureInfo.InvariantCulture)[..9];
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-4));
        return new Ecf
        {
            Encabezado = new EcfEncabezado
            {
                IdDoc = new IdDoc
                {
                    TipoeCF = 31,
                    Encf = $"E310{sequence}",
                    FechaVencimientoSecuencia = new DateOnly(today.Year + 1, 12, 31),
                    IndicadorMontoGravado = 0,
                    TipoIngresos = "01",
                    TipoPago = 1,
                },
                Emisor = new Emisor
                {
                    RNCEmisor = rncEmisor,
                    RazonSocialEmisor = "EMISOR DE PRUEBA",
                    DireccionEmisor = "SANTO DOMINGO",
                    FechaEmision = today,
                },
                Comprador = new Comprador { RNCComprador = "131880681", RazonSocialComprador = "DGII" },
                Totales = new Totales
                {
                    MontoGravadoTotal = 100.00m,
                    MontoGravadoI1 = 100.00m,
                    ITBIS1 = 18,
                    TotalITBIS = 18.00m,
                    TotalITBIS1 = 18.00m,
                    MontoTotal = 118.00m,
                },
            },
            DetallesItems =
            [
                new Item
                {
                    NumeroLinea = 1,
                    IndicadorFacturacion = 1,
                    NombreItem = "Producto de prueba",
                    IndicadorBienoServicio = 1,
                    CantidadItem = 1m,
                    PrecioUnitarioItem = 100.00m,
                    MontoItem = 100.00m,
                },
            ],
            FechaHoraFirma = DateTime.UtcNow.AddHours(-4),
        };
    }
}
