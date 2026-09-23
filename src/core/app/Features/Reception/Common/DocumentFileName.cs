using DgiiEcf.Application.Common.Xml.XmlDocumentLoader;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Reception.Common;

/// <summary>
/// Resolves the multipart file name. DGII expects <c>{RNCEmisor}{eNCF}.xml</c>, e.g. <c>101672919E310000000001.xml</c>.
/// </summary>
public static class DocumentFileName
{
    public static readonly Error Required = new(
        "reception.file_name_required",
        "No se pudo determinar el nombre del archivo; indique fileName (formato {RNCEmisor}{eNCF}.xml).");

    public static Result<string> Resolve(string signedXml, string? fileName)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var trimmed = fileName.Trim();
            return trimmed.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ? trimmed : trimmed + ".xml";
        }

        var loaded = XmlDocumentLoader.Load(signedXml);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var rnc = (XmlDocumentLoader.FindFirst(loaded.Value, "RNCEmisor") ?? XmlDocumentLoader.FindFirst(loaded.Value, "RncEmisor"))?.Value.Trim();
        // ANECF has no eNCF element; the first voided sequence identifies the file instead.
        var encf = (XmlDocumentLoader.FindFirst(loaded.Value, "eNCF") ?? XmlDocumentLoader.FindFirst(loaded.Value, "SecuenciaeNCFDesde"))?.Value.Trim();
        if (string.IsNullOrEmpty(rnc) || string.IsNullOrEmpty(encf))
        {
            return Required;
        }

        return $"{rnc}{encf}.xml";
    }
}
