using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DgiiEcf.Application.Common.Errors.DgiiApiError;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.ExternalServices.Http.DgiiErrorMessageExtractor;
using Microsoft.Extensions.Logging;

namespace DgiiEcf.ExternalServices.Http.DgiiHttpTransport;

/// <summary>
/// Typed <see cref="HttpClient"/> shared by every DGII client. Maps any failure to <see cref="DgiiApiError"/>
/// carrying the description DGII sent in the body.
/// </summary>
public sealed class DgiiHttpTransport
{
    public const string UnauthorizedFallback = "ERROR 401: Unauthorized, please check your credentials";
    public const string XmlFieldName = "xml";

    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<DgiiHttpTransport> _logger;

    public DgiiHttpTransport(HttpClient httpClient, ILogger<DgiiHttpTransport> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<Result<string>> GetStringAsync(
        Uri uri,
        IReadOnlyDictionary<string, string?>? query,
        AuthorizationHeader? authorization,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, WithQuery(uri, query));
        return SendAsync(request, authorization, cancellationToken);
    }

    public async Task<Result<T>> GetJsonAsync<T>(
        Uri uri,
        IReadOnlyDictionary<string, string?>? query,
        AuthorizationHeader? authorization,
        CancellationToken cancellationToken)
    {
        var body = await GetStringAsync(uri, query, authorization, cancellationToken);
        return body.IsSuccess ? Deserialize<T>(body.Value, uri) : body.Error;
    }

    /// <summary>
    /// Posts <paramref name="xml"/> as <c>multipart/form-data</c> with a single file part named <c>xml</c>.
    /// The body is buffered so <c>Content-Length</c> is always sent: without it DGII answers
    /// "Multipart cannot be empty".
    /// </summary>
    public async Task<Result<T>> PostXmlFileAsync<T>(
        Uri uri,
        string xml,
        string fileName,
        AuthorizationHeader? authorization,
        CancellationToken cancellationToken)
    {
        var boundary = "----DgiiEcf" + Guid.NewGuid().ToString("N");
        var content = new MultipartFormDataContent(boundary);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/form-data; boundary={boundary}");

        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(xml));
        file.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        file.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = $"\"{XmlFieldName}\"",
            FileName = $"\"{fileName}\"",
        };
        content.Add(file);
        content.Headers.ContentLength = (await content.ReadAsByteArrayAsync(cancellationToken)).LongLength;

        var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
        var body = await SendAsync(request, authorization, cancellationToken);
        return body.IsSuccess ? Deserialize<T>(body.Value, uri) : body.Error;
    }

    internal static Result<T> Deserialize<T>(string body, Uri uri)
    {
        if (typeof(T) == typeof(string))
        {
            return (Result<T>)(object)Result<string>.Success(body);
        }

        try
        {
            var value = JsonSerializer.Deserialize<T>(body, JsonOptions);
            return value is null ? InvalidResponse(uri, body, "respuesta vacía") : value;
        }
        catch (JsonException exception)
        {
            return InvalidResponse(uri, body, exception.Message);
        }
    }

    private async Task<Result<string>> SendAsync(HttpRequestMessage request, AuthorizationHeader? authorization, CancellationToken cancellationToken)
    {
        using (request)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (authorization is not null)
            {
                request.Headers.TryAddWithoutValidation("Authorization", authorization.Value);
            }

            var resource = request.RequestUri!.AbsolutePath;
            var method = request.Method.Method;

            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return body;
                }

                var error = FromResponse(response.StatusCode, response.ReasonPhrase, body, resource, method);
                _logger.LogWarning(
                    "DGII {Method} {Resource} failed with {Status}: {Message}",
                    method,
                    resource,
                    (int)response.StatusCode,
                    error.Message);
                return error;
            }
            catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(exception, "DGII {Method} {Resource} timed out", method, resource);
                return new DgiiApiError(
                    DgiiApiError.TimeoutCodeValue,
                    $"La solicitud a la DGII excedió el tiempo de espera ({resource}).",
                    TransportCode: "ECONNABORTED",
                    Resource: resource,
                    Method: method);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "DGII {Method} {Resource} transport error", method, resource);
                return new DgiiApiError(
                    DgiiApiError.TransportCodeValue,
                    string.IsNullOrWhiteSpace(exception.Message) ? "DGII request failed" : exception.Message,
                    TransportCode: exception.HttpRequestError.ToString(),
                    Resource: resource,
                    Method: method);
            }
        }
    }

    internal static DgiiApiError FromResponse(HttpStatusCode statusCode, string? reasonPhrase, string? body, string resource, string method)
    {
        var status = (int)statusCode;
        var message = DgiiErrorMessageExtractor.DgiiErrorMessageExtractor.Extract(body)
            ?? (statusCode == HttpStatusCode.Unauthorized
                ? UnauthorizedFallback
                : $"DGII request failed with status code {status}{(string.IsNullOrEmpty(reasonPhrase) ? string.Empty : $" ({reasonPhrase})")}");

        return new DgiiApiError(
            DgiiApiError.HttpCodePrefix + status,
            message,
            Status: status,
            StatusText: reasonPhrase,
            Resource: resource,
            Method: method,
            Messages: DgiiErrorMessageExtractor.DgiiErrorMessageExtractor.ReadMessages(body),
            RawBody: body);
    }

    private static DgiiApiError InvalidResponse(Uri uri, string body, string detail) => new(
        DgiiApiError.InvalidResponseCodeValue,
        $"La respuesta de la DGII no tiene el formato esperado: {detail}",
        Resource: uri.AbsolutePath,
        RawBody: body);

    private static Uri WithQuery(Uri uri, IReadOnlyDictionary<string, string?>? query)
    {
        var pairs = query?
            .Where(pair => !string.IsNullOrEmpty(pair.Value))
            .Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value!)}")
            .ToList();

        return pairs is { Count: > 0 } ? new Uri($"{uri}?{string.Join("&", pairs)}") : uri;
    }
}

/// <summary>
/// Value of the <c>Authorization</c> header: <c>Bearer {token}</c> for DGII and receivers, the raw API key
/// for the service status API.
/// </summary>
public sealed record AuthorizationHeader(string Value)
{
    public static AuthorizationHeader Bearer(string token) => new($"Bearer {token}");

    public static AuthorizationHeader ApiKey(string apiKey) => new(apiKey);

    public override string ToString() => "Authorization: ***";
}
