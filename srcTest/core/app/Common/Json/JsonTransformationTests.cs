using System.Text.Json.Nodes;
using System.Xml.Linq;
using DgiiEcf.Application.Common.Json.JsonCasingNormalizer;
using DgiiEcf.Application.Common.Json.JsonXmlTransformer;
using DgiiEcf.Application.Common.Xml.XmlBodyExtractor;
using DgiiEcf.Application.Tests.Support;

namespace DgiiEcf.Application.Tests.Common.Json;

public sealed class JsonTransformationTests
{
    [Fact]
    public void XmlToJson_Arecf_ExposesTextNodes()
    {
        var json = JsonXmlTransformer.XmlToJson(Samples.Read("customer_receipt.xml")).Value;

        Assert.Equal("0", json["ARECF"]!["DetalleAcusedeRecibo"]!["Estado"]!["_text"]!.GetValue<string>());
        Assert.Equal("utf-8", json["_declaration"]!["_attributes"]!["encoding"]!.GetValue<string>());
    }

    [Fact]
    public void XmlToJson_AcecfFromMultipartBody_ReadsTheEncf()
    {
        var xml = XmlBodyExtractor.Extract(Samples.Read("commercial_approval_response.xml"))!;

        var json = JsonXmlTransformer.XmlToJson(xml).Value;

        Assert.Equal("E450000000001", json["ACECF"]!["DetalleAprobacionComercial"]!["eNCF"]!["_text"]!.GetValue<string>());
    }

    [Fact]
    public void XmlToJson_RepeatedElements_BecomeArrays()
    {
        var json = JsonXmlTransformer.XmlToJson("<A><B>1</B><B>2</B><C /></A>").Value;

        Assert.Equal(2, json["A"]!["B"]!.AsArray().Count);
        Assert.Empty(json["A"]!["C"]!.AsObject());
    }

    [Fact]
    public void JsonToXml_Rfce_KeepsKeyOrderAndRepeatsArrays()
    {
        var xml = JsonXmlTransformer.JsonToXml(Samples.Read("cf_json_data_32.json")).Value;

        var root = XDocument.Parse(xml).Root!;
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<RFCE>", xml);
        Assert.Equal("RFCE", root.Name.LocalName);
        Assert.Equal(["Version", "IdDoc", "Emisor", "Comprador", "Totales", "CodigoSeguridadeCF"], root.Element("Encabezado")!.Elements().Select(e => e.Name.LocalName));
        Assert.Equal("32", root.Descendants("TipoeCF").Single().Value);
    }

    [Fact]
    public void JsonToXml_Round_FormatsDecimalsExceptVersion()
    {
        var xml = JsonXmlTransformer.JsonToXml("""{"ECF":{"Version":"1.0","MontoTotal":"12.5","Cantidad":"3","Otro":12.345}}""", round: true).Value;

        Assert.Contains("<Version>1.0</Version>", xml);
        Assert.Contains("<MontoTotal>12.50</MontoTotal>", xml);
        Assert.Contains("<Cantidad>3</Cantidad>", xml);
        Assert.Contains("<Otro>12.35</Otro>", xml);
    }

    [Fact]
    public void JsonToXml_AttributesAndEmptyObjects_AreSupported()
    {
        var xml = JsonXmlTransformer.JsonToXml(
            """{"ARECF":{"_attributes":{"xmlns:xsi":"http://www.w3.org/2001/XMLSchema-instance"},"DetalleAcusedeRecibo":{"Estado":{"_text":"0"},"Vacio":{}}}}""").Value;

        Assert.Contains("<ARECF xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">", xml);
        Assert.Contains("<Estado>0</Estado>", xml);
        Assert.Contains("<Vacio></Vacio>", xml);
    }

    [Fact]
    public void JsonToXml_MoreThanOneRoot_Fails()
    {
        Assert.True(JsonXmlTransformer.JsonToXml("""{"A":{},"B":{}}""").IsFailure);
    }

    [Fact]
    public void Normalize_LowercasePayload_UsesDgiiNames()
    {
        var payload = JsonNode.Parse("""
            {
              "ecf": {
                "encabezado": {
                  "version": "1.0",
                  "iddoc": { "tipoecf": "31", "encf": "E310000009175", "tablaformaspago": { "formadepago": [ { "formapago": 1, "montopago": 10 } ] } },
                  "emisor": { "rncemisor": "123", "tablatelefonoemisor": { "telefonoemisor": ["809"] } },
                  "totales": { "montototal": 637.2, "itbis1": 18 }
                },
                "detallesitems": {
                  "item": [
                    { "numerolinea": "1", "tablasubdescuento": { "subdescuento": [ { "tiposubdescuento": "%", "montosubdescuento": 60 } ] } },
                    { "numerolinea": "2" }
                  ]
                },
                "campodesconocido": "x",
                "fechahorafirma": "15-07-2023 05:07:00"
              }
            }
            """)!.AsObject();

        var normalized = JsonCasingNormalizer.Normalize(payload);

        var ecf = normalized["ECF"]!;
        Assert.Equal("E310000009175", ecf["Encabezado"]!["IdDoc"]!["eNCF"]!.GetValue<string>());
        Assert.Equal("31", ecf["Encabezado"]!["IdDoc"]!["TipoeCF"]!.GetValue<string>());
        Assert.Equal(10, ecf["Encabezado"]!["IdDoc"]!["TablaFormasPago"]!["FormaDePago"]![0]!["MontoPago"]!.GetValue<int>());
        Assert.Equal("809", ecf["Encabezado"]!["Emisor"]!["TablaTelefonoEmisor"]!["TelefonoEmisor"]![0]!.GetValue<string>());
        Assert.Equal(18, ecf["Encabezado"]!["Totales"]!["ITBIS1"]!.GetValue<int>());
        Assert.Equal("%", ecf["DetallesItems"]!["Item"]![0]!["TablaSubDescuento"]!["SubDescuento"]![0]!["TipoSubDescuento"]!.GetValue<string>());
        Assert.Equal("2", ecf["DetallesItems"]!["Item"]![1]!["NumeroLinea"]!.GetValue<string>());
        Assert.Equal("x", ecf["campodesconocido"]!.GetValue<string>());
        Assert.Equal(
            ["Encabezado", "DetallesItems", "campodesconocido", "FechaHoraFirma"],
            ecf.AsObject().Select(pair => pair.Key));
    }
}
