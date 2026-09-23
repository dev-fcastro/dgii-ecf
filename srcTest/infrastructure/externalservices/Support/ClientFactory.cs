using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;
using DgiiEcf.ExternalServices.Http.DgiiHttpTransport;
using DgiiEcf.ExternalServices.Options.DgiiApiOptions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DgiiEcf.ExternalServices.Tests.Support;

internal static class ClientFactory
{
    public static DgiiHttpTransport Transport(HttpMessageHandler handler) =>
        new(new HttpClient(handler), NullLogger<DgiiHttpTransport>.Instance);

    public static IOptions<DgiiApiOptions> Options(DgiiEnvironment environment = DgiiEnvironment.Test) =>
        Microsoft.Extensions.Options.Options.Create(new DgiiApiOptions { Environment = environment });
}
