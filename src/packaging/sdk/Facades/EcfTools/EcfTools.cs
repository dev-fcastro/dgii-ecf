using System.Text.Json.Nodes;
using DgiiEcf.Application.Common.Json.JsonCasingNormalizer;
using DgiiEcf.Application.Common.Json.JsonXmlTransformer;
using DgiiEcf.Application.Common.Time.DominicanClock;
using DgiiEcf.Application.Common.Xml.DgiiXmlSerializer;
using DgiiEcf.Application.Common.Xml.XmlBodyExtractor;
using DgiiEcf.Application.Common.Xml.XmlValueEditor;
using DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce;
using DgiiEcf.Application.Features.Documents.GenerateQrCodeUrl;
using DgiiEcf.Application.Features.Documents.GetSecurityCode;
using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.Domain.Documents.SecurityCode;

namespace DgiiEcf.Facades.EcfTools;

/// <summary>
/// Pure helpers that need neither the certificate nor HTTP (the <c>utils</c> of the Node package).
/// </summary>
public static class EcfTools
{
    private static readonly GetSecurityCodeHandler SecurityCodeHandler = new();
    private static readonly GenerateQrCodeUrlHandler QrCodeHandler = new();
    private static readonly ConvertEcf32ToRfceHandler RfceHandler = new(SecurityCodeHandler);

    /// <summary>First 6 characters of the SignatureValue (código de seguridad).</summary>
    public static Result<SecurityCode> GetSecurityCode(string signedXml) =>
        SecurityCodeHandler.Handle(new GetSecurityCodeQuery(signedXml));

    /// <summary>Unsigned RFCE of a signed e-CF 32 under RD$250,000.</summary>
    public static Result<RfceConversion> ConvertEcf32ToRfce(string signedEcf32Xml) =>
        RfceHandler.Handle(new ConvertEcf32ToRfceCommand(signedEcf32Xml));

    public static string EcfQrCodeUrl(
        string rncEmisor,
        string? rncComprador,
        string encf,
        string montoTotal,
        string fechaEmision,
        string fechaFirma,
        string securityCode,
        DgiiEnvironment environment) =>
        QrCodeHandler.Handle(new EcfQrCodeUrlQuery(rncEmisor, rncComprador, encf, montoTotal, fechaEmision, fechaFirma, securityCode, environment));

    public static string FcQrCodeUrl(string rncEmisor, string encf, decimal montoTotal, string securityCode, DgiiEnvironment environment) =>
        QrCodeHandler.Handle(new FcQrCodeUrlQuery(rncEmisor, encf, montoTotal, securityCode, environment));

    /// <summary>Replaces the text of the first element named <paramref name="elementName"/>.</summary>
    public static Result<string> SetXmlValue(string xml, string elementName, string value) =>
        XmlValueEditor.SetValue(xml, elementName, value);

    /// <summary>Extracts the first <c>&lt;?xml ... &lt;/{lastTagName}&gt;</c> from a raw body.</summary>
    public static string? ExtractXmlFromBody(string body, string lastTagName = "ACECF") =>
        XmlBodyExtractor.Extract(body, lastTagName);

    /// <summary>Serializes a typed document from <c>DgiiEcf.Domain.Documents</c>.</summary>
    public static string Serialize<TDocument>(TDocument document) where TDocument : class =>
        DgiiXmlSerializer.Serialize(document);

    public static Result<TDocument> Deserialize<TDocument>(string xml) where TDocument : class, new() =>
        DgiiXmlSerializer.Deserialize<TDocument>(xml);

    /// <summary>JSON (xml-js compact shape used by the Node package) to XML.</summary>
    public static Result<string> JsonToXml(string json, bool round = false) => JsonXmlTransformer.JsonToXml(json, round);

    public static Result<JsonObject> XmlToJson(string xml) => JsonXmlTransformer.XmlToJson(xml);

    /// <summary>Fixes key casing of a JSON payload to the DGII element names (e.g. <c>encf</c> → <c>eNCF</c>).</summary>
    public static JsonObject NormalizeJsonCasing(JsonObject payload) => JsonCasingNormalizer.Normalize(payload);

    /// <summary>Dominican date and time now, as <c>dd-MM-yyyy HH:mm:ss</c>.</summary>
    public static string CurrentFormattedDateTime(TimeProvider? timeProvider = null) =>
        DominicanClock.FormattedDateTime(timeProvider ?? TimeProvider.System);

    /// <summary>Dominican date now, as <c>dd-MM-yyyy</c>.</summary>
    public static string CurrentFormattedDate(TimeProvider? timeProvider = null) =>
        DominicanClock.FormattedDate(timeProvider ?? TimeProvider.System);
}
