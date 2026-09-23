using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using DgiiEcf.Application.Common.Responses.DgiiMessage;

namespace DgiiEcf.ExternalServices.Http.DgiiErrorMessageExtractor;

/// <summary>
/// Reduces whatever DGII sends back on a failure to one human readable message (port of
/// <c>extractDgiiErrorMessage</c>). DGII answers with bare strings, JSON encoded strings,
/// <c>{ mensajes: [...] }</c>, ASP.NET ProblemDetails / ModelState payloads or HTML error pages.
/// </summary>
internal static class DgiiErrorMessageExtractor
{
    public const int MaxLength = 1000;

    private static readonly string[] MessageKeys =
    [
        "mensaje", "Mensaje", "message", "Message", "error", "Error", "errorMessage", "error_description",
        "detail", "Detail", "title", "Title", "descripcion", "Descripcion",
    ];

    private static readonly Regex Whitespace = new("\\s+", RegexOptions.Compiled);
    private static readonly Regex HtmlStart = new("^\\s*<(?:!doctype|html|body|head)\\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Scripts = new("<script\\b[^>]*>[\\s\\S]*?</script>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Styles = new("<style\\b[^>]*>[\\s\\S]*?</style>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex Tags = new("<[^>]+>", RegexOptions.Compiled);

    public static string? Extract(string? body)
    {
        if (body is null)
        {
            return null;
        }

        var trimmed = body.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed[0] is '[' or '{' or '"' && TryParse(trimmed) is { } parsed)
        {
            return Extract(parsed);
        }

        if (HtmlStart.IsMatch(trimmed))
        {
            var text = StripHtml(trimmed);
            return text.Length == 0 ? null : Truncate(text);
        }

        return Truncate(Collapse(trimmed));
    }

    public static string? Extract(JsonNode? node)
    {
        switch (node)
        {
            case null:
                return null;
            case JsonValue value when value.TryGetValue<string>(out var text):
                return Extract(text);
            case JsonValue value:
                return value.ToJsonString();
            case JsonArray array:
                var parts = array.Select(Extract).Where(part => !string.IsNullOrEmpty(part)).ToList();
                return parts.Count == 0 ? null : Truncate(string.Join(" | ", parts));
            case JsonObject jsonObject:
                return ExtractFromObject(jsonObject);
            default:
                return null;
        }
    }

    /// <summary>
    /// Reads <c>mensajes</c>/<c>Mensajes</c>, accepting both <c>{ valor, codigo }</c> objects and plain strings.
    /// </summary>
    public static IReadOnlyList<DgiiMessage>? ReadMessages(string? body)
    {
        var node = string.IsNullOrWhiteSpace(body) ? null : TryParse(body.Trim());
        return node is JsonObject jsonObject ? ReadMessages(jsonObject) : null;
    }

    private static IReadOnlyList<DgiiMessage>? ReadMessages(JsonObject jsonObject)
    {
        var source = jsonObject["mensajes"] ?? jsonObject["Mensajes"];
        if (source is not JsonArray { Count: > 0 } array)
        {
            return null;
        }

        var messages = new List<DgiiMessage>();
        foreach (var item in array)
        {
            if (item is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text))
            {
                messages.Add(new DgiiMessage(text.Trim(), 0));
            }
            else if (item is JsonObject message
                && (message["valor"] ?? message["Valor"]) is JsonValue valor
                && valor.TryGetValue<string>(out var valorText))
            {
                var codigo = (message["codigo"] ?? message["Codigo"]) is JsonValue codigoValue && codigoValue.TryGetValue<int>(out var number)
                    ? number
                    : 0;
                messages.Add(new DgiiMessage(valorText, codigo));
            }
        }

        return messages.Count == 0 ? null : messages;
    }

    private static string? ExtractFromObject(JsonObject jsonObject)
    {
        if (ReadMessages(jsonObject) is { } messages)
        {
            return Truncate(string.Join(" | ", messages.Select(message => message.Valor)));
        }

        foreach (var key in MessageKeys)
        {
            var value = jsonObject[key];
            if (value is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text))
            {
                return Truncate(Collapse(text));
            }

            // ProblemDetails may nest the description under `error`.
            if (key == "error" && value is JsonObject or JsonArray && Extract(value) is { } nested)
            {
                return nested;
            }
        }

        var errors = jsonObject["errors"] ?? jsonObject["Errors"];
        if (errors is JsonObject modelState)
        {
            var parts = modelState
                .Select(pair => (field: pair.Key, message: Extract(pair.Value)))
                .Where(pair => !string.IsNullOrEmpty(pair.message))
                .Select(pair => $"{pair.field}: {pair.message}")
                .ToList();
            if (parts.Count > 0)
            {
                return Truncate(string.Join(" | ", parts));
            }
        }
        else if (errors is not null && Extract(errors) is { } nestedErrors)
        {
            return nestedErrors;
        }

        var serialized = jsonObject.ToJsonString();
        return serialized == "{}" ? null : Truncate(serialized);
    }

    private static JsonNode? TryParse(string text)
    {
        try
        {
            return JsonNode.Parse(text);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string StripHtml(string html)
    {
        var text = Scripts.Replace(html, " ");
        text = Styles.Replace(text, " ");
        text = Tags.Replace(text, " ");
        return Collapse(text);
    }

    private static string Collapse(string value) => Whitespace.Replace(value, " ").Trim();

    private static string Truncate(string value) => value.Length > MaxLength ? value[..MaxLength] + "…" : value;
}
