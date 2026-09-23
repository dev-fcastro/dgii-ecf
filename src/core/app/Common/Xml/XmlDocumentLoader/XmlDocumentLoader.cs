using System.Xml;
using System.Xml.Linq;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Xml.XmlDocumentLoader;

/// <summary>
/// Parses XML safely (no DTD, no external resolution) and tolerates a leading UTF-8 BOM.
/// </summary>
public static class XmlDocumentLoader
{
    private const char ByteOrderMark = '﻿';

    public static string StripByteOrderMark(string xml) =>
        xml.Length > 0 && xml[0] == ByteOrderMark ? xml[1..] : xml;

    public static Result<XDocument> Load(string? xml, LoadOptions options = LoadOptions.None)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return XmlErrors.XmlErrors.Empty;
        }

        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                IgnoreComments = (options & LoadOptions.PreserveWhitespace) == 0,
            };

            using var stringReader = new StringReader(StripByteOrderMark(xml).TrimStart());
            using var reader = XmlReader.Create(stringReader, settings);
            var document = XDocument.Load(reader, options);
            if (document.Root is null)
            {
                return XmlErrors.XmlErrors.Invalid("el documento no tiene elemento raíz.");
            }

            return document;
        }
        catch (XmlException exception)
        {
            return XmlErrors.XmlErrors.Invalid(exception.Message);
        }
    }

    /// <summary>
    /// Finds the first descendant (or self) element with the given local name, ignoring namespaces.
    /// </summary>
    public static XElement? FindFirst(XContainer container, string localName) =>
        container is XElement self && self.Name.LocalName == localName
            ? self
            : container.Descendants().FirstOrDefault(element => element.Name.LocalName == localName);
}
