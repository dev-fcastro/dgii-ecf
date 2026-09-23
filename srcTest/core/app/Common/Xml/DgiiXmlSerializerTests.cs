using DgiiEcf.Application.Common.Xml.DgiiXmlSerializer;
using DgiiEcf.Application.Tests.Support;
using DgiiEcf.Domain.Documents.Anecf;
using DgiiEcf.Domain.Documents.Arecf;
using DgiiEcf.Domain.Documents.Ecf;

namespace DgiiEcf.Application.Tests.Common.Xml;

public sealed class DgiiXmlSerializerTests
{
    [Fact]
    public void Serialize_Arecf_WritesXsdOrderAndDgiiFormats()
    {
        var document = new Arecf
        {
            DetalleAcusedeRecibo = new DetalleAcusedeRecibo
            {
                RNCEmisor = "130862346",
                RNCComprador = "101023122",
                Encf = "E310005000113",
                Estado = 0,
                FechaHoraAcuseRecibo = new DateTime(2023, 8, 19, 23, 37, 2),
            },
        };

        var xml = DgiiXmlSerializer.Serialize(document);

        Assert.Equal(
            """
            <?xml version="1.0" encoding="utf-8"?>
            <ARECF>
              <DetalleAcusedeRecibo>
                <Version>1.0</Version>
                <RNCEmisor>130862346</RNCEmisor>
                <RNCComprador>101023122</RNCComprador>
                <eNCF>E310005000113</eNCF>
                <Estado>0</Estado>
                <FechaHoraAcuseRecibo>19-08-2023 23:37:02</FechaHoraAcuseRecibo>
              </DetalleAcusedeRecibo>
            </ARECF>
            """.Replace("\r\n", "\n"),
            xml);
    }

    [Fact]
    public void Serialize_Ecf_WritesListsAsWrappersAndKeepsDecimalScale()
    {
        var ecf = new Ecf
        {
            Encabezado = new EcfEncabezado
            {
                IdDoc = new IdDoc
                {
                    TipoeCF = 31,
                    Encf = "E310000000001",
                    FechaVencimientoSecuencia = new DateOnly(2025, 12, 31),
                    TablaFormasPago = [new FormaDePago { FormaPago = 1, MontoPago = 7080.00m }],
                },
                Emisor = new Emisor { RNCEmisor = "131880738", TablaTelefonoEmisor = ["809-000-0000", "809-111-1111"] },
                Totales = new Totales { MontoTotal = 637.20m, ImpuestosAdicionales = [] },
            },
            DetallesItems = [new Item { NumeroLinea = 1, NombreItem = "Producto & Co" }],
        };

        var xml = DgiiXmlSerializer.Serialize(ecf, indent: false);

        Assert.Equal(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><ECF><Encabezado><Version>1.0</Version><IdDoc><TipoeCF>31</TipoeCF><eNCF>E310000000001</eNCF>"
            + "<FechaVencimientoSecuencia>31-12-2025</FechaVencimientoSecuencia><TablaFormasPago><FormaDePago><FormaPago>1</FormaPago>"
            + "<MontoPago>7080.00</MontoPago></FormaDePago></TablaFormasPago></IdDoc><Emisor><RNCEmisor>131880738</RNCEmisor>"
            + "<TablaTelefonoEmisor><TelefonoEmisor>809-000-0000</TelefonoEmisor><TelefonoEmisor>809-111-1111</TelefonoEmisor></TablaTelefonoEmisor>"
            + "</Emisor><Totales><MontoTotal>637.20</MontoTotal></Totales></Encabezado><DetallesItems><Item><NumeroLinea>1</NumeroLinea>"
            + "<NombreItem>Producto &amp; Co</NombreItem></Item></DetallesItems></ECF>",
            xml);
    }

    [Fact]
    public void Deserialize_SignedEcf_ReadsTypedValues()
    {
        var result = DgiiXmlSerializer.Deserialize<Ecf>(Samples.Read("130359334E310000008928.xml"));

        Assert.True(result.IsSuccess, result.Error.Message);
        var ecf = result.Value;
        Assert.Equal("E310000008928", ecf.Encabezado!.IdDoc!.Encf);
        Assert.Equal(new DateOnly(2025, 12, 31), ecf.Encabezado.IdDoc.FechaVencimientoSecuencia);
        Assert.Equal(637.2m, ecf.Encabezado.Totales!.MontoTotal);
        var subDescuento = Assert.Single(Assert.Single(ecf.DetallesItems!).TablaSubDescuento!);
        Assert.Equal("%", subDescuento.TipoSubDescuento);
        Assert.Equal(new DateTime(2025, 1, 20, 9, 34, 59), ecf.FechaHoraFirma);
    }

    [Fact]
    public void Deserialize_WrongRoot_ReturnsUnexpectedRoot()
    {
        Assert.Equal("xml.unexpected_root", DgiiXmlSerializer.Deserialize<Ecf>("<RFCE />").Error.Code);
    }

    [Fact]
    public void Deserialize_InvalidDate_ReturnsInvalidValue()
    {
        var result = DgiiXmlSerializer.Deserialize<Ecf>("<ECF><FechaHoraFirma>ayer</FechaHoraFirma></ECF>");

        Assert.Equal("xml.invalid_value", result.Error.Code);
    }

    [Fact]
    public void SerializeThenDeserialize_Anecf_RoundTrips()
    {
        var anecf = new Anecf
        {
            Encabezado = new AnecfEncabezado { RncEmisor = "131880681", CantidadeNCFAnulados = 2, FechaHoraAnulacioneNCF = new DateTime(2026, 8, 17, 8, 0, 0) },
            DetalleAnulacion =
            [
                new Anulacion
                {
                    NoLinea = 1,
                    TipoeCF = 31,
                    TablaRangoSecuenciasAnuladaseNCF = [new Secuencias { SecuenciaeNCFDesde = "E310000000010", SecuenciaeNCFHasta = "E310000000011" }],
                    CantidadeNCFAnulados = 2,
                },
            ],
        };

        var roundTrip = DgiiXmlSerializer.Deserialize<Anecf>(DgiiXmlSerializer.Serialize(anecf)).Value;

        Assert.Equal("E310000000011", roundTrip.DetalleAnulacion![0].TablaRangoSecuenciasAnuladaseNCF![0].SecuenciaeNCFHasta);
        Assert.Equal(anecf.Encabezado.FechaHoraAnulacioneNCF, roundTrip.Encabezado!.FechaHoraAnulacioneNCF);
    }
}
