using DgiiEcf.Application.Common.Contracts.IDgiiAuthenticationClient;
using DgiiEcf.Application.Common.Responses.AccessToken;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.ExternalServices.Http.DgiiEndpoints;
using DgiiEcf.ExternalServices.Http.DgiiHttpTransport;
using DgiiEcf.ExternalServices.Options.DgiiApiOptions;
using Microsoft.Extensions.Options;

namespace DgiiEcf.ExternalServices.Clients.DgiiAuthenticationClient;

public sealed class DgiiAuthenticationClient : IDgiiAuthenticationClient
{
    private const string SignedSeedFileName = "signed.xml";

    private readonly DgiiHttpTransport _transport;
    private readonly DgiiApiOptions _options;

    public DgiiAuthenticationClient(DgiiHttpTransport transport, IOptions<DgiiApiOptions> options)
    {
        _transport = transport;
        _options = options.Value;
    }

    public Task<Result<string>> GetSeedAsync(string? buyerHost, CancellationToken cancellationToken)
    {
        var uri = string.IsNullOrWhiteSpace(buyerHost)
            ? DgiiEndpoints.Dgii(_options.EcfBaseUrl, _options.Environment, DgiiEndpoints.Seed)
            : DgiiEndpoints.Receiver(buyerHost, DgiiEndpoints.ReceiverSeed);

        return _transport.GetStringAsync(uri, null, null, cancellationToken);
    }

    public Task<Result<AccessToken>> ValidateSeedAsync(string signedSeed, string? buyerHost, CancellationToken cancellationToken)
    {
        var uri = string.IsNullOrWhiteSpace(buyerHost)
            ? DgiiEndpoints.Dgii(_options.EcfBaseUrl, _options.Environment, DgiiEndpoints.ValidateSeed)
            : DgiiEndpoints.Receiver(buyerHost, DgiiEndpoints.ReceiverValidateSeed);

        return _transport.PostXmlFileAsync<AccessToken>(uri, signedSeed, SignedSeedFileName, null, cancellationToken);
    }
}
