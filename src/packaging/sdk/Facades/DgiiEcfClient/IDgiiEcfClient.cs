using DgiiEcf.Application.Common.Responses.AccessToken;
using DgiiEcf.Application.Common.Responses.CommercialApprovalResponse;
using DgiiEcf.Application.Common.Responses.CustomerDirectoryEntry;
using DgiiEcf.Application.Common.Responses.InquiryStatusResponse;
using DgiiEcf.Application.Common.Responses.InvoiceResponse;
using DgiiEcf.Application.Common.Responses.InvoiceSummaryResponse;
using DgiiEcf.Application.Common.Responses.ServiceStatus;
using DgiiEcf.Application.Common.Responses.SummaryInvoiceInquiryResponse;
using DgiiEcf.Application.Common.Responses.TrackIdSummary;
using DgiiEcf.Application.Common.Responses.TrackingStatusResponse;
using DgiiEcf.Application.Common.Responses.VoidEncfResponse;
using DgiiEcf.Application.Features.Documents.ConvertEcf32ToRfce;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Facades.DgiiEcfClient;

/// <summary>
/// Issuer side of DGII electronic invoicing (equivalent to the <c>ECF</c> class of the Node package).
/// Tokens are obtained and renewed automatically; call <see cref="AuthenticateAsync"/> only to force it.
/// </summary>
public interface IDgiiEcfClient
{
    Task<Result<AccessToken>> AuthenticateAsync(string? buyerHost = null, CancellationToken cancellationToken = default);

    /// <summary>Signs an XML document (ECF, RFCE, ACECF, ANECF, ARECF...).</summary>
    Result<string> Sign(string xml, string? rootElementName = null);

    /// <summary>Serializes and signs a typed document from <c>DgiiEcf.Domain.Documents</c>.</summary>
    Result<string> SignDocument<TDocument>(TDocument document) where TDocument : class;

    /// <summary>Converts a signed e-CF 32 under RD$250,000 to its RFCE and signs it.</summary>
    Result<SignedRfce> CreateSignedRfce(string signedEcf32Xml);

    Task<Result<InvoiceResponse>> SendElectronicDocumentAsync(
        string signedXml, string? fileName = null, string? buyerHost = null, CancellationToken cancellationToken = default);

    Task<Result<InvoiceSummaryResponse>> SendSummaryAsync(string signedRfceXml, string? fileName = null, CancellationToken cancellationToken = default);

    Task<Result<CommercialApprovalResponse>> SendCommercialApprovalAsync(
        string signedAcecfXml, string? fileName = null, string? buyerHost = null, CancellationToken cancellationToken = default);

    Task<Result<VoidEncfResponse>> VoidEncfAsync(string signedAnecfXml, string? fileName = null, CancellationToken cancellationToken = default);

    Task<Result<TrackingStatusResponse>> GetTrackStatusAsync(string trackId, CancellationToken cancellationToken = default);

    Task<Result<InquiryStatusResponse>> InquiryStatusAsync(
        string rncEmisor, string encf, string? rncComprador = null, string? securityCode = null, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TrackIdSummary>>> GetTrackIdsAsync(string rncEmisor, string encf, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CustomerDirectoryEntry>>> GetCustomerDirectoryAsync(string rnc, CancellationToken cancellationToken = default);

    Task<Result<SummaryInvoiceInquiryResponse>> GetSummaryInvoiceInquiryAsync(
        string rncEmisor, string encf, string securityCode, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<DgiiServiceStatus>>> GetServicesStatusAsync(string? apiKey = null, CancellationToken cancellationToken = default);

    Task<Result<MaintenanceResponse>> GetMaintenanceWindowsAsync(string? apiKey = null, CancellationToken cancellationToken = default);

    Task<Result<VerificationResponse>> VerifyServiceStatusAsync(string? apiKey = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Signed RFCE ready for <see cref="IDgiiEcfClient.SendSummaryAsync"/>, plus the security code printed on the invoice.
/// </summary>
public sealed record SignedRfce(string SignedXml, string SecurityCode, RfceConversion Conversion);
