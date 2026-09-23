using System.Xml.Linq;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Xml.XmlValueEditor;

/// <summary>
/// Replaces the text of the first element with the given name (port of <c>editXmlValue</c>).
/// </summary>
public static class XmlValueEditor
{
    public static Result<string> SetValue(string xml, string elementName, string value)
    {
        var loaded = XmlDocumentLoader.XmlDocumentLoader.Load(xml, LoadOptions.PreserveWhitespace);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var element = XmlDocumentLoader.XmlDocumentLoader.FindFirst(loaded.Value, elementName);
        if (element is null)
        {
            return XmlErrors.XmlErrors.ElementNotFound(elementName);
        }

        element.Value = value;
        var hasDeclaration = XmlDocumentLoader.XmlDocumentLoader.StripByteOrderMark(xml).TrimStart().StartsWith("<?xml", StringComparison.Ordinal);
        var body = loaded.Value.Root!.ToString(SaveOptions.DisableFormatting);
        return hasDeclaration ? DgiiXmlWriter.DgiiXmlWriter.Declaration + body : body;
    }
}
