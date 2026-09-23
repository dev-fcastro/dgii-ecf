using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Contracts.IXmlDocumentSigner;

/// <summary>
/// Signs XML documents with the enveloped XML-DSig profile required by DGII.
/// </summary>
public interface IXmlDocumentSigner
{
    /// <summary>
    /// Signs <paramref name="xml"/> and returns the signed document serialized on a single line.
    /// </summary>
    /// <param name="xml">Unsigned XML document.</param>
    /// <param name="rootElementName">
    /// Expected root element (ECF, RFCE, ARECF, ACECF, ANECF, SemillaModel...). When null the actual root is used.
    /// </param>
    Result<string> Sign(string xml, string? rootElementName = null);
}
