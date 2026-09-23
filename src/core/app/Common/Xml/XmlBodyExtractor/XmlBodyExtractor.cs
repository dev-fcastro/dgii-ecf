using System.Text.RegularExpressions;

namespace DgiiEcf.Application.Common.Xml.XmlBodyExtractor;

/// <summary>
/// Extracts an XML document embedded in a raw body, e.g. a multipart payload
/// (port of <c>getXmlFromBodyResponse</c>).
/// </summary>
public static class XmlBodyExtractor
{
    /// <summary>
    /// Returns the text from the first <c>&lt;?xml</c> up to the first closing <c>&lt;/{lastTagName}&gt;</c>,
    /// or null when there is no match.
    /// </summary>
    public static string? Extract(string? body, string lastTagName = "ACECF")
    {
        if (string.IsNullOrEmpty(body))
        {
            return null;
        }

        var pattern = $"<\\?xml[\\s\\S]*?</{Regex.Escape(lastTagName)}>";
        var match = Regex.Match(body, pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));
        return match.Success ? match.Value : null;
    }
}
