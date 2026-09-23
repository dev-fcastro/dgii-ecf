using System.Collections;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Xml.Serialization;
using DgiiEcf.Domain.Documents.Acecf;
using DgiiEcf.Domain.Documents.Anecf;
using DgiiEcf.Domain.Documents.Arecf;
using DgiiEcf.Domain.Documents.Ecf;
using DgiiEcf.Domain.Documents.Rfce;

namespace DgiiEcf.Application.Common.Json.JsonCasingNormalizer;

/// <summary>
/// Renames JSON keys case-insensitively to the exact DGII element names (port of
/// <c>transformeLowercasePayloadToCamelcase</c>): <c>{ "ecf": { "encabezado": { "idDoc": ... } } }</c> →
/// <c>{ "ECF": { "Encabezado": { "IdDoc": ... } } }</c>. The reference is the typed document model, so every
/// field of the XSD is covered. Key order is preserved and unknown keys are kept as they are.
/// </summary>
public static class JsonCasingNormalizer
{
    private static readonly Type[] Documents = [typeof(Ecf), typeof(Rfce), typeof(Arecf), typeof(Acecf), typeof(Anecf)];

    public static JsonObject Normalize(JsonObject source)
    {
        var roots = Documents.ToDictionary(
            type => type.GetCustomAttribute<XmlRootAttribute>()!.ElementName,
            type => new Reference(type, null),
            StringComparer.OrdinalIgnoreCase);

        return NormalizeObject(source, roots);
    }

    private static JsonObject NormalizeObject(JsonObject source, IReadOnlyDictionary<string, Reference> references)
    {
        var result = new JsonObject();
        foreach (var (key, value) in source)
        {
            if (key.StartsWith('_') || !references.TryGetValue(key, out var reference))
            {
                result[key] = value?.DeepClone();
                continue;
            }

            var name = references.Keys.First(candidate => string.Equals(candidate, key, StringComparison.OrdinalIgnoreCase));
            result[name] = NormalizeValue(value, reference);
        }

        return result;
    }

    private static JsonNode? NormalizeValue(JsonNode? value, Reference reference) => value switch
    {
        JsonArray array => new JsonArray(array.Select(item => NormalizeValue(item, reference)).ToArray()),
        JsonObject jsonObject when reference.ItemName is not null => NormalizeList(jsonObject, reference),
        JsonObject jsonObject when reference.Type is not null => NormalizeObject(jsonObject, ChildrenOf(reference.Type)),
        _ => value?.DeepClone(),
    };

    private static JsonObject NormalizeList(JsonObject wrapper, Reference reference)
    {
        var itemReference = new Reference(reference.Type, null);
        var items = new Dictionary<string, Reference>(StringComparer.OrdinalIgnoreCase) { [reference.ItemName!] = itemReference };
        return NormalizeObject(wrapper, items);
    }

    private static Dictionary<string, Reference> ChildrenOf(Type type)
    {
        var children = new Dictionary<string, Reference>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(property => property.MetadataToken))
        {
            var elementName = property.GetCustomAttribute<XmlElementAttribute>()?.ElementName is { Length: > 0 } name ? name : property.Name;
            var itemName = property.GetCustomAttribute<XmlArrayItemAttribute>()?.ElementName;
            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            Type? childType = null;
            if (itemName is not null && typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType.IsGenericType)
            {
                var itemType = propertyType.GetGenericArguments()[0];
                childType = itemType == typeof(string) ? null : itemType;
            }
            else if (propertyType.IsClass && propertyType != typeof(string))
            {
                childType = propertyType;
            }

            children.TryAdd(elementName, new Reference(childType, itemName));
        }

        return children;
    }

    private sealed record Reference(Type? Type, string? ItemName);
}
