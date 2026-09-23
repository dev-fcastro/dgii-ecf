using DgiiEcf.Application.Common.Responses.CommercialApprovalResponse;
using DgiiEcf.Application.Common.Responses.InvoiceResponse;
using DgiiEcf.Application.Common.Responses.InvoiceSummaryResponse;
using DgiiEcf.Application.Common.Responses.VoidEncfResponse;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Contracts.IDgiiReceptionClient;

/// <summary>
/// Sends signed documents as <c>multipart/form-data</c> (field <c>xml</c>).
/// </summary>
public interface IDgiiReceptionClient
{
    Task<Result<InvoiceResponse>> SendElectronicDocumentAsync(
        string signedXml, string fileName, string accessToken, string? buyerHost, CancellationToken cancellationToken);

    Task<Result<InvoiceSummaryResponse>> SendSummaryAsync(
        string signedXml, string fileName, string accessToken, CancellationToken cancellationToken);

    Task<Result<CommercialApprovalResponse>> SendCommercialApprovalAsync(
        string signedXml, string fileName, string accessToken, string? buyerHost, CancellationToken cancellationToken);

    Task<Result<VoidEncfResponse>> VoidEncfAsync(
        string signedXml, string fileName, string accessToken, CancellationToken cancellationToken);
}
