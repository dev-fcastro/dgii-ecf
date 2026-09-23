using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using DgiiEcf.Application.Common.Time.DominicanClock;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Xml.DgiiXmlSerializer;

/// <summary>
/// Serializes the typed DGII documents (<c>DgiiEcf.Domain.Documents.*</c>) to XML and back.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Elements are written in property declaration order, which follows the DGII XSD sequence.</item>
/// <item>Null values, empty strings, empty lists and empty sections are omitted.</item>
/// <item><see cref="decimal"/> uses the invariant culture and keeps the scale you provide (<c>100.50m</c> → <c>100.50</c>).</item>
/// <item><see cref="DateOnly"/> → <c>dd-MM-yyyy</c>; <see cref="DateTime"/> → <c>dd-MM-yyyy HH:mm:ss</c>.</item>
/// <item>Lists are written as a wrapper element (property name) with repeated <see cref="XmlArrayItemAttribute"/> children.</item>
/// </list>
/// </remarks>
public static class DgiiXmlSerializer
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<XmlMember>> Members = new();

    public static string Serialize<T>(T document, bool indent = true) where T : class =>
        DgiiXmlWriter.DgiiXmlWriter.Write(ToElement(document), indent);

    public static XElement ToElement<T>(T document) where T : class
    {
        ArgumentNullException.ThrowIfNull(document);

        var rootName = ResolveRootName(document.GetType())
            ?? throw new InvalidOperationException(XmlErrors.XmlErrors.MissingRootAttribute.Message);

        var root = new XElement(rootName);
        WriteMembers(root, document);
        return root;
    }

    public static Result<T> Deserialize<T>(string xml) where T : class, new()
    {
        var loaded = XmlDocumentLoader.XmlDocumentLoader.Load(xml);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        return FromElement<T>(loaded.Value.Root!);
    }

    public static Result<T> FromElement<T>(XElement root) where T : class, new()
    {
        var expectedRoot = ResolveRootName(typeof(T));
        if (expectedRoot is not null && root.Name.LocalName != expectedRoot)
        {
            return XmlErrors.XmlErrors.UnexpectedRoot(expectedRoot, root.Name.LocalName);
        }

        var result = ReadObject(root, typeof(T));
        return result.IsSuccess ? (T)result.Value : result.Error;
    }

    public static string? ResolveRootName(Type type) => type.GetCustomAttribute<XmlRootAttribute>()?.ElementName;

    private static void WriteMembers(XElement parent, object instance)
    {
        foreach (var member in GetMembers(instance.GetType()))
        {
            var value = member.Property.GetValue(instance);
            if (value is null)
            {
                continue;
            }

            if (member.ItemName is not null && value is IEnumerable items and not string)
            {
                var wrapper = new XElement(member.ElementName);
                foreach (var item in items)
                {
                    var child = CreateElement(member.ItemName, item);
                    if (child is not null)
                    {
                        wrapper.Add(child);
                    }
                }

                if (wrapper.HasElements)
                {
                    parent.Add(wrapper);
                }

                continue;
            }

            var element = CreateElement(member.ElementName, value);
            if (element is not null)
            {
                parent.Add(element);
            }
        }
    }

    private static XElement? CreateElement(string name, object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (IsScalar(value.GetType()))
        {
            var text = FormatScalar(value);
            return string.IsNullOrEmpty(text) ? null : new XElement(name, text);
        }

        var element = new XElement(name);
        WriteMembers(element, value);
        return element.HasElements ? element : null;
    }

    private static Result<object> ReadObject(XElement element, Type type)
    {
        var instance = Activator.CreateInstance(type)!;
        foreach (var member in GetMembers(type))
        {
            var child = element.Elements().FirstOrDefault(candidate => candidate.Name.LocalName == member.ElementName);
            if (child is null)
            {
                continue;
            }

            var propertyType = member.Property.PropertyType;
            if (member.ItemName is not null)
            {
                var itemType = propertyType.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType))!;
                foreach (var itemElement in child.Elements().Where(candidate => candidate.Name.LocalName == member.ItemName))
                {
                    var item = ReadValue(itemElement, itemType);
                    if (item.IsFailure)
                    {
                        return item.Error;
                    }

                    list.Add(item.Value);
                }

                member.Property.SetValue(instance, list);
                continue;
            }

            var value = ReadValue(child, propertyType);
            if (value.IsFailure)
            {
                return value.Error;
            }

            member.Property.SetValue(instance, value.Value);
        }

        return instance;
    }

    private static Result<object> ReadValue(XElement element, Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        if (!IsScalar(targetType))
        {
            return ReadObject(element, targetType);
        }

        var text = element.Value.Trim();
        return ParseScalar(targetType, text) is { } parsed
            ? Result<object>.Success(parsed)
            : XmlErrors.XmlErrors.InvalidValue(element.Name.LocalName, text);
    }

    private static bool IsScalar(Type type)
    {
        var target = Nullable.GetUnderlyingType(type) ?? type;
        return target.IsPrimitive
            || target.IsEnum
            || target == typeof(string)
            || target == typeof(decimal)
            || target == typeof(DateOnly)
            || target == typeof(DateTime)
            || target == typeof(DateTimeOffset);
    }

    private static string? FormatScalar(object value) => value switch
    {
        string text => text,
        decimal number => number.ToString(CultureInfo.InvariantCulture),
        DateOnly date => date.ToString(DominicanClock.DateFormat, CultureInfo.InvariantCulture),
        DateTime dateTime => dateTime.ToString(DominicanClock.DateTimeFormat, CultureInfo.InvariantCulture),
        DateTimeOffset dateTimeOffset => dateTimeOffset.ToOffset(DominicanClock.Offset)
            .ToString(DominicanClock.DateTimeFormat, CultureInfo.InvariantCulture),
        bool flag => flag ? "1" : "0",
        Enum enumValue => Convert.ToInt64(enumValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };

    private static object? ParseScalar(Type type, string text)
    {
        var invariant = CultureInfo.InvariantCulture;
        if (type == typeof(string))
        {
            return text;
        }

        if (type == typeof(decimal))
        {
            return decimal.TryParse(text, NumberStyles.Number, invariant, out var number) ? number : null;
        }

        if (type == typeof(int))
        {
            return int.TryParse(text, NumberStyles.Integer, invariant, out var integer) ? integer : null;
        }

        if (type == typeof(long))
        {
            return long.TryParse(text, NumberStyles.Integer, invariant, out var integer) ? integer : null;
        }

        if (type == typeof(DateOnly))
        {
            return DateOnly.TryParseExact(text, DominicanClock.DateFormat, invariant, DateTimeStyles.None, out var date) ? date : null;
        }

        if (type == typeof(DateTime))
        {
            return DateTime.TryParseExact(text, DominicanClock.DateTimeFormat, invariant, DateTimeStyles.None, out var dateTime)
                ? dateTime
                : null;
        }

        if (type == typeof(bool))
        {
            return text switch { "1" or "true" => true, "0" or "false" => false, _ => null };
        }

        if (type.IsEnum)
        {
            return long.TryParse(text, NumberStyles.Integer, invariant, out var raw) ? Enum.ToObject(type, raw) : null;
        }

        try
        {
            return Convert.ChangeType(text, type, invariant);
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
        {
            return null;
        }
    }

    private static IReadOnlyList<XmlMember> GetMembers(Type type) => Members.GetOrAdd(type, static target =>
        target.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)
            .Where(property => property.GetCustomAttribute<XmlIgnoreAttribute>() is null)
            .OrderBy(property => property.MetadataToken)
            .Select(property => new XmlMember(
                property,
                property.GetCustomAttribute<XmlElementAttribute>()?.ElementName is { Length: > 0 } elementName
                    ? elementName
                    : property.Name,
                property.GetCustomAttribute<XmlArrayItemAttribute>()?.ElementName))
            .ToList());

    private sealed record XmlMember(PropertyInfo Property, string ElementName, string? ItemName);
}
