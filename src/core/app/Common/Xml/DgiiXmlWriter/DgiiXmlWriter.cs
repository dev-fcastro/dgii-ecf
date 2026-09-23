using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DgiiEcf.Application.Common.Xml.DgiiXmlWriter;

/// <summary>
/// Writes XML the way DGII samples are laid out: <c>&lt;?xml version="1.0" encoding="utf-8"?&gt;</c>,
/// two-space indentation, <c>\n</c> line endings and no trailing newline.
/// </summary>
public static class DgiiXmlWriter
{
    public const string Declaration = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

    public static string Write(XElement root, bool indent = true)
    {
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = indent,
            IndentChars = "  ",
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace,
            Encoding = new UTF8Encoding(false),
        };

        var builder = new StringBuilder();
        using (var writer = XmlWriter.Create(builder, settings))
        {
            root.WriteTo(writer);
        }

        return Declaration + (indent ? "\n" : string.Empty) + builder;
    }
}
