using DgiiEcf.Application.Common.Responses.CustomerDirectoryEntry;
using DgiiEcf.Application.Common.Responses.InquiryStatusResponse;
using DgiiEcf.Application.Common.Responses.SummaryInvoiceInquiryResponse;
using DgiiEcf.Application.Common.Responses.TrackIdSummary;
using DgiiEcf.Application.Common.Responses.TrackingStatusResponse;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Contracts.IDgiiQueryClient;

/// <summary>
/// DGII inquiry services.
/// </summary>
public interface IDgiiQueryClient
{
    Task<Result<TrackingStatusResponse>> GetTrackStatusAsync(string trackId, string accessToken, CancellationToken cancellationToken);

    Task<Result<InquiryStatusResponse>> InquiryStatusAsync(
        string rncEmisor, string encf, string? rncComprador, string? securityCode, string accessToken, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<TrackIdSummary>>> GetTrackIdsAsync(
        string rncEmisor, string encf, string accessToken, CancellationToken cancellationToken);

    /// <summary>
    /// Always resolves to a list: DGII answers either with an array or with a single entry.
    /// </summary>
    Task<Result<IReadOnlyList<CustomerDirectoryEntry>>> GetCustomerDirectoryAsync(
        string rnc, string accessToken, CancellationToken cancellationToken);

    Task<Result<SummaryInvoiceInquiryResponse>> GetSummaryInvoiceInquiryAsync(
        string rncEmisor, string encf, string securityCode, string accessToken, CancellationToken cancellationToken);
}
