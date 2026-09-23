using DgiiEcf.Application.Common.Contracts.IDgiiReceptionClient;
using DgiiEcf.Application.Common.Responses.CommercialApprovalResponse;
using DgiiEcf.Application.Common.Responses.InvoiceResponse;
using DgiiEcf.Application.Common.Responses.InvoiceSummaryResponse;
using DgiiEcf.Application.Common.Responses.VoidEncfResponse;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.ExternalServices.Http.DgiiEndpoints;
using DgiiEcf.ExternalServices.Http.DgiiHttpTransport;
using DgiiEcf.ExternalServices.Options.DgiiApiOptions;
using Microsoft.Extensions.Options;

namespace DgiiEcf.ExternalServices.Clients.DgiiReceptionClient;

public sealed class DgiiReceptionClient : IDgiiReceptionClient
{
    private readonly DgiiHttpTransport _transport;
    private readonly DgiiApiOptions _options;

    public DgiiReceptionClient(DgiiHttpTransport transport, IOptions<DgiiApiOptions> options)
    {
        _transport = transport;
        _options = options.Value;
    }

    public Task<Result<InvoiceResponse>> SendElectronicDocumentAsync(
        string signedXml, string fileName, string accessToken, string? buyerHost, CancellationToken cancellationToken)
    {
        var uri = string.IsNullOrWhiteSpace(buyerHost)
            ? DgiiEndpoints.Dgii(_options.EcfBaseUrl, _options.Environment, DgiiEndpoints.SendInvoice)
            : DgiiEndpoints.Receiver(buyerHost, DgiiEndpoints.ReceiverInvoice);

        return _transport.PostXmlFileAsync<InvoiceResponse>(uri, signedXml, fileName, AuthorizationHeader.Bearer(accessToken), cancellationToken);
    }

    public Task<Result<InvoiceSummaryResponse>> SendSummaryAsync(
        string signedXml, string fileName, string accessToken, CancellationToken cancellationToken)
    {
        var uri = DgiiEndpoints.Dgii(_options.FcBaseUrl, _options.Environment, DgiiEndpoints.SendSummary);
        return _transport.PostXmlFileAsync<InvoiceSummaryResponse>(uri, signedXml, fileName, AuthorizationHeader.Bearer(accessToken), cancellationToken);
    }

    public Task<Result<CommercialApprovalResponse>> SendCommercialApprovalAsync(
        string signedXml, string fileName, string accessToken, string? buyerHost, CancellationToken cancellationToken)
    {
        var uri = string.IsNullOrWhiteSpace(buyerHost)
            ? DgiiEndpoints.Dgii(_options.EcfBaseUrl, _options.Environment, DgiiEndpoints.CommercialApproval)
            : DgiiEndpoints.Receiver(buyerHost, DgiiEndpoints.ReceiverCommercialApproval);

        return _transport.PostXmlFileAsync<CommercialApprovalResponse>(uri, signedXml, fileName, AuthorizationHeader.Bearer(accessToken), cancellationToken);
    }

    public Task<Result<VoidEncfResponse>> VoidEncfAsync(
        string signedXml, string fileName, string accessToken, CancellationToken cancellationToken)
    {
        var uri = DgiiEndpoints.Dgii(_options.EcfBaseUrl, _options.Environment, DgiiEndpoints.VoidRange);
        return _transport.PostXmlFileAsync<VoidEncfResponse>(uri, signedXml, fileName, AuthorizationHeader.Bearer(accessToken), cancellationToken);
    }
}
