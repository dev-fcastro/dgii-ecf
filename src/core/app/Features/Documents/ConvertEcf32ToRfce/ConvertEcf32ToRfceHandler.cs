using System.Globalization;
using System.Xml.Linq;
using DgiiEcf.Application.Common.Xml.DgiiXmlWriter;
using DgiiEcf.Application.Common.Xml.XmlDocumentLoader;
using DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce.Contracts;
using DgiiEcf.Application.Features.Documents.GetSecurityCode;
using DgiiEcf.Application.Features.Documents.GetSecurityCode.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce;

/// <summary>
/// Copies the RFCE fields from the e-CF 32 in the order required by the RFCE XSD. Values are copied
/// verbatim and empty values or sections are dropped.
/// </summary>
public sealed class ConvertEcf32ToRfceHandler : IConvertEcf32ToRfceHandler
{
    private const decimal SummaryLimit = 250_000m;
    private const string ConsumoType = "32";

    private static readonly string[] IdDocFields = ["TipoeCF", "eNCF", "TipoIngresos", "TipoPago"];
    private static readonly string[] EmisorFields = ["RNCEmisor", "RazonSocialEmisor", "FechaEmision"];
    private static readonly string[] CompradorFields = ["RNCComprador", "IdentificadorExtranjero", "RazonSocialComprador"];

    private static readonly string[] TotalesLeadingFields =
    [
        "MontoGravadoTotal", "MontoGravadoI1", "MontoGravadoI2", "MontoGravadoI3", "MontoExento",
        "TotalITBIS", "TotalITBIS1", "TotalITBIS2", "TotalITBIS3", "MontoImpuestoAdicional",
    ];

    private static readonly string[] ImpuestoAdicionalFields =
    [
        "TipoImpuesto", "MontoImpuestoSelectivoConsumoEspecifico", "MontoImpuestoSelectivoConsumoAdvalorem", "OtrosImpuestosAdicionales",
    ];

    private static readonly string[] TotalesTrailingFields = ["MontoTotal", "MontoNoFacturable", "MontoPeriodo"];

    private readonly IGetSecurityCodeHandler _securityCodeHandler;

    public ConvertEcf32ToRfceHandler(IGetSecurityCodeHandler securityCodeHandler)
    {
        _securityCodeHandler = securityCodeHandler;
    }

    public Result<RfceConversion> Handle(ConvertEcf32ToRfceCommand command)
    {
        var loaded = XmlDocumentLoader.Load(command.SignedEcfXml);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var ecf = loaded.Value.Root!;
        var encabezado = Child(ecf, "Encabezado");
        if (ecf.Name.LocalName != "ECF" || encabezado is null)
        {
            return ConvertEcf32ToRfceErrors.NotAnEcf;
        }

        var idDoc = Child(encabezado, "IdDoc");
        var totales = Child(encabezado, "Totales");

        var tipo = Text(idDoc, "TipoeCF");
        if (tipo is not null && tipo != ConsumoType)
        {
            return ConvertEcf32ToRfceErrors.NotConsumo;
        }

        if (decimal.TryParse(Text(totales, "MontoTotal"), NumberStyles.Number, CultureInfo.InvariantCulture, out var montoTotal)
            && montoTotal >= SummaryLimit)
        {
            return ConvertEcf32ToRfceErrors.AmountAboveLimit;
        }

        var securityCode = _securityCodeHandler.Handle(new GetSecurityCodeQuery(command.SignedEcfXml));
        if (securityCode.IsFailure)
        {
            return securityCode.Error;
        }

        var rfceIdDoc = Section("IdDoc", Copy(idDoc, IdDocFields));
        AddIfNotEmpty(rfceIdDoc, CopyTree(Child(idDoc, "TablaFormasPago")));

        var impuestos = Section(
            "ImpuestosAdicionales",
            Children(Child(totales, "ImpuestosAdicionales"), "ImpuestoAdicional")
                .Select(impuesto => Section("ImpuestoAdicional", Copy(impuesto, ImpuestoAdicionalFields))));

        var rfceTotales = Section(
            "Totales",
            Copy(totales, TotalesLeadingFields).Append(impuestos).Concat(Copy(totales, TotalesTrailingFields)));

        var rfceEncabezado = Section(
            "Encabezado",
            Copy(encabezado, ["Version"])
                .Append(rfceIdDoc)
                .Append(Section("Emisor", Copy(Child(encabezado, "Emisor"), EmisorFields)))
                .Append(Section("Comprador", Copy(Child(encabezado, "Comprador"), CompradorFields)))
                .Append(rfceTotales)
                .Append(new XElement("CodigoSeguridadeCF", securityCode.Value.Value)));

        var rfce = new XElement("RFCE", rfceEncabezado);
        return new RfceConversion(DgiiXmlWriter.Write(rfce), securityCode.Value.Value);
    }

    private static XElement? Child(XElement? parent, string name) =>
        parent?.Elements().FirstOrDefault(element => element.Name.LocalName == name);

    private static IEnumerable<XElement> Children(XElement? parent, string name) =>
        parent?.Elements().Where(element => element.Name.LocalName == name) ?? [];

    private static string? Text(XElement? parent, string name)
    {
        var value = Child(parent, name)?.Value.Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static IEnumerable<XElement> Copy(XElement? parent, IEnumerable<string> names) =>
        names.Select(name => (name, value: Text(parent, name)))
            .Where(field => field.value is not null)
            .Select(field => new XElement(field.name, field.value));

    /// <summary>
    /// Copies a subtree without namespaces, dropping empty leaves and sections.
    /// </summary>
    private static XElement? CopyTree(XElement? source)
    {
        if (source is null)
        {
            return null;
        }

        if (!source.HasElements)
        {
            var value = source.Value.Trim();
            return value.Length == 0 ? null : new XElement(source.Name.LocalName, value);
        }

        var copy = new XElement(source.Name.LocalName);
        foreach (var child in source.Elements())
        {
            AddIfNotEmpty(copy, CopyTree(child));
        }

        return copy.HasElements ? copy : null;
    }

    private static XElement Section(string name, IEnumerable<XElement?> children)
    {
        var section = new XElement(name);
        foreach (var child in children)
        {
            AddIfNotEmpty(section, child);
        }

        return section;
    }

    private static void AddIfNotEmpty(XElement parent, XElement? child)
    {
        if (child is not null && (child.HasElements || child.Value.Length > 0))
        {
            parent.Add(child);
        }
    }
}
