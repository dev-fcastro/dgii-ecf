using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DgiiEcf.Application.Common.Xml.DgiiXmlWriter;
using DgiiEcf.Application.Common.Xml.XmlDocumentLoader;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Json.JsonXmlTransformer;

/// <summary>
/// JSON ⇄ XML conversion compatible with the "compact" format of the Node package (xml-js):
/// object keys are elements, arrays are repeated sibling elements, <c>_attributes</c> holds attributes and
/// <c>_text</c> holds text. Use it to keep sending the same JSON payloads you used with <c>dgii-ecf</c>.
/// </summary>
public static class JsonXmlTransformer
{
    private const string AttributesKey = "_attributes";
    private const string TextKey = "_text";
    private const string CDataKey = "_cdata";
    private const string DeclarationKey = "_declaration";
    private const string CommentKey = "_comment";
    private const string InstructionKey = "_instruction";

    private static readonly Regex DecimalPattern = new("^\\d+\\.\\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Converts a JSON object such as <c>{ "ECF": { "Encabezado": { ... } } }</c> to XML.
    /// </summary>
    /// <param name="json">JSON text or object.</param>
    /// <param name="round">When true, values like <c>12.5</c> are written with two decimals (<c>12.50</c>), except <c>Version</c>.</param>
    public static Result<string> JsonToXml(string json, bool round = false)
    {
        JsonNode? node;
        try
        {
            node = JsonNode.Parse(json);
        }
        catch (JsonException exception)
        {
            return new Error("json.invalid", $"JSON inválido: {exception.Message}");
        }

        return node is JsonObject jsonObject ? JsonToXml(jsonObject, round) : new Error("json.invalid", "Se esperaba un objeto JSON.");
    }

    public static Result<string> JsonToXml(JsonObject json, bool round = false)
    {
        var roots = json.Where(property => !property.Key.StartsWith('_')).ToList();
        if (roots.Count != 1 || roots[0].Value is not JsonObject)
        {
            return new Error("json.single_root_required", "El JSON debe tener un único elemento raíz de tipo objeto, por ejemplo { \"ECF\": { ... } }.");
        }

        var root = new XElement(roots[0].Key);
        Fill(root, (JsonObject)roots[0].Value!, round);
        return DgiiXmlWriter.Write(root);
    }

    /// <summary>
    /// Converts XML to the compact JSON shape: every value is <c>{ "_text": "..." }</c>, repeated elements
    /// become arrays and empty elements become <c>{}</c>.
    /// </summary>
    public static Result<JsonObject> XmlToJson(string xml)
    {
        var loaded = XmlDocumentLoader.Load(xml);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var result = new JsonObject();
        if (loaded.Value.Declaration is { } declaration)
        {
            var attributes = new JsonObject();
            if (declaration.Version is not null)
            {
                attributes["version"] = declaration.Version;
            }

            if (declaration.Encoding is not null)
            {
                attributes["encoding"] = declaration.Encoding;
            }

            result[DeclarationKey] = new JsonObject { [AttributesKey] = attributes };
        }

        var root = loaded.Value.Root!;
        result[ElementName(root)] = ToJson(root);
        return result;
    }

    private static void Fill(XElement element, JsonObject json, bool round)
    {
        foreach (var (key, value) in json)
        {
            switch (key)
            {
                case AttributesKey when value is JsonObject attributes:
                    foreach (var (name, attributeValue) in attributes)
                    {
                        if (attributeValue is not null)
                        {
                            element.SetAttributeValue(ResolveName(element, name), Scalar(attributeValue));
                        }
                    }

                    break;
                case TextKey:
                    element.Add(new XText(FormatText(Scalar(value), element.Name.LocalName, round)));
                    break;
                case CDataKey:
                    element.Add(new XCData(Scalar(value)));
                    break;
                case DeclarationKey or CommentKey or InstructionKey:
                    break;
                default:
                    AddChild(element, key, value, round);
                    break;
            }
        }

        if (!element.Nodes().Any())
        {
            element.Value = string.Empty;
        }
    }

    private static void AddChild(XElement parent, string name, JsonNode? value, bool round)
    {
        switch (value)
        {
            case null:
                return;
            case JsonArray array:
                foreach (var item in array)
                {
                    AddChild(parent, name, item, round);
                }

                return;
            case JsonObject jsonObject:
                var child = new XElement(ResolveName(parent, name));
                Fill(child, jsonObject, round);
                parent.Add(child);
                return;
            default:
                parent.Add(new XElement(ResolveName(parent, name), FormatText(Scalar(value), name, round)));
                return;
        }
    }

    private static XName ResolveName(XElement context, string name)
    {
        var separator = name.IndexOf(':');
        if (separator <= 0)
        {
            return name;
        }

        var prefix = name[..separator];
        var localName = name[(separator + 1)..];
        if (prefix == "xmlns")
        {
            return XNamespace.Xmlns + localName;
        }

        var ns = context.GetNamespaceOfPrefix(prefix);
        return ns is null ? XmlConvertSafe(name) : ns + localName;
    }

    private static string XmlConvertSafe(string name) => System.Xml.XmlConvert.EncodeLocalName(name)!;

    private static string Scalar(JsonNode? node) => node switch
    {
        null => string.Empty,
        JsonValue value when value.TryGetValue<string>(out var text) => text,
        JsonValue value when value.TryGetValue<bool>(out var flag) => flag ? "true" : "false",
        _ => node.ToJsonString(),
    };

    private static string FormatText(string text, string elementName, bool round)
    {
        if (round
            && !string.Equals(elementName, "version", StringComparison.OrdinalIgnoreCase)
            && DecimalPattern.IsMatch(text)
            && decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
        {
            return decimal.Round(number, 2, MidpointRounding.AwayFromZero).ToString("0.00", CultureInfo.InvariantCulture);
        }

        return text;
    }

    private static JsonNode ToJson(XElement element)
    {
        var result = new JsonObject();
        var attributes = element.Attributes().ToList();
        if (attributes.Count > 0)
        {
            var jsonAttributes = new JsonObject();
            foreach (var attribute in attributes)
            {
                jsonAttributes[AttributeName(attribute)] = attribute.Value;
            }

            result[AttributesKey] = jsonAttributes;
        }

        if (!element.HasElements)
        {
            if (element.Value.Length > 0)
            {
                result[TextKey] = element.Value;
            }

            return result;
        }

        foreach (var group in element.Elements().GroupBy(ElementName))
        {
            var items = group.Select(ToJson).ToList();
            result[group.Key] = items.Count == 1 ? items[0] : new JsonArray(items.ToArray());
        }

        return result;
    }

    private static string ElementName(XElement element)
    {
        var prefix = element.GetPrefixOfNamespace(element.Name.Namespace);
        return string.IsNullOrEmpty(prefix) ? element.Name.LocalName : $"{prefix}:{element.Name.LocalName}";
    }

    private static string AttributeName(XAttribute attribute)
    {
        if (attribute.IsNamespaceDeclaration)
        {
            return attribute.Name.Namespace == XNamespace.None ? "xmlns" : $"xmlns:{attribute.Name.LocalName}";
        }

        var prefix = attribute.Parent?.GetPrefixOfNamespace(attribute.Name.Namespace);
        return string.IsNullOrEmpty(prefix) ? attribute.Name.LocalName : $"{prefix}:{attribute.Name.LocalName}";
    }
}
