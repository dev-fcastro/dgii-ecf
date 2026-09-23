using System.Text;

namespace DgiiEcf.Tests.Support;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, string?, HttpResponseMessage> _respond;

    public StubHttpMessageHandler(Func<HttpRequestMessage, string?, HttpResponseMessage> respond)
    {
        _respond = respond;
    }

    public List<(Uri Uri, string? Authorization, string? Body)> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add((request.RequestUri!, request.Headers.Authorization?.ToString(), body));
        return _respond(request, body);
    }

    public static HttpResponseMessage Json(string json) => new(System.Net.HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };

    public static HttpResponseMessage Xml(string xml) => new(System.Net.HttpStatusCode.OK)
    {
        Content = new StringContent(xml, Encoding.UTF8, "application/xml"),
    };
}
