using System.Xml;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Signing.XmlSigning.SecureXml;

/// <summary>
/// Loads <see cref="XmlDocument"/> instances without DTD processing or external resolution.
/// </summary>
internal static class SecureXml
{
    public static Result<XmlDocument> Load(string xml, bool preserveWhitespace)
    {
        var text = xml.Length > 0 && xml[0] == '﻿' ? xml[1..] : xml;

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = !preserveWhitespace,
            IgnoreWhitespace = !preserveWhitespace,
        };

        var document = new XmlDocument { PreserveWhitespace = preserveWhitespace, XmlResolver = null };
        try
        {
            using var stringReader = new StringReader(preserveWhitespace ? text : text.Trim());
            using var reader = XmlReader.Create(stringReader, settings);
            document.Load(reader);
        }
        catch (XmlException exception)
        {
            return SigningErrors.SigningErrors.InvalidXml(exception.Message);
        }

        if (document.DocumentElement is null)
        {
            return SigningErrors.SigningErrors.InvalidXml("the document has no root element.");
        }

        return document;
    }
}
