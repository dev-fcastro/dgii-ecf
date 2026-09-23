using System.Net;
using System.Text;

namespace DgiiEcf.ExternalServices.Tests.Support;

/// <summary>
/// Records every request and answers with a canned response (or throws the configured exception).
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

    public StubHttpMessageHandler(HttpStatusCode status, string body = "", string mediaType = "application/json")
        : this(_ => new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, mediaType) })
    {
    }

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        _respond = respond;
    }

    public List<RecordedRequest> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add(new RecordedRequest(
            request.Method,
            request.RequestUri!,
            request.Headers.TryGetValues("Authorization", out var authorization) ? authorization.Single() : null,
            request.Content?.Headers.ContentType?.ToString(),
            request.Content?.Headers.ContentLength,
            body));

        return _respond(request);
    }
}

internal sealed record RecordedRequest(HttpMethod Method, Uri Uri, string? Authorization, string? ContentType, long? ContentLength, string? Body);
