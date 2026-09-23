using System.Text.Json;
using System.Text.Json.Nodes;
using DgiiEcf.Application.Common.Contracts.IDgiiQueryClient;
using DgiiEcf.Application.Common.Responses.CustomerDirectoryEntry;
using DgiiEcf.Application.Common.Responses.InquiryStatusResponse;
using DgiiEcf.Application.Common.Responses.SummaryInvoiceInquiryResponse;
using DgiiEcf.Application.Common.Responses.TrackIdSummary;
using DgiiEcf.Application.Common.Responses.TrackingStatusResponse;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.ExternalServices.Http.DgiiEndpoints;
using DgiiEcf.ExternalServices.Http.DgiiHttpTransport;
using DgiiEcf.ExternalServices.Options.DgiiApiOptions;
using Microsoft.Extensions.Options;

namespace DgiiEcf.ExternalServices.Clients.DgiiQueryClient;

public sealed class DgiiQueryClient : IDgiiQueryClient
{
    private readonly DgiiHttpTransport _transport;
    private readonly DgiiApiOptions _options;

    public DgiiQueryClient(DgiiHttpTransport transport, IOptions<DgiiApiOptions> options)
    {
        _transport = transport;
        _options = options.Value;
    }

    public Task<Result<TrackingStatusResponse>> GetTrackStatusAsync(string trackId, string accessToken, CancellationToken cancellationToken) =>
        _transport.GetJsonAsync<TrackingStatusResponse>(
            Ecf(DgiiEndpoints.TrackResultStatus),
            new Dictionary<string, string?> { ["trackId"] = trackId },
            AuthorizationHeader.Bearer(accessToken),
            cancellationToken);

    public Task<Result<InquiryStatusResponse>> InquiryStatusAsync(
        string rncEmisor, string encf, string? rncComprador, string? securityCode, string accessToken, CancellationToken cancellationToken) =>
        _transport.GetJsonAsync<InquiryStatusResponse>(
            Ecf(DgiiEndpoints.InquiryStatus),
            new Dictionary<string, string?>
            {
                ["rncEmisor"] = rncEmisor,
                ["ncfElectronico"] = encf,
                ["rncComprador"] = rncComprador,
                ["codigoSeguridad"] = securityCode,
            },
            AuthorizationHeader.Bearer(accessToken),
            cancellationToken);

    public async Task<Result<IReadOnlyList<TrackIdSummary>>> GetTrackIdsAsync(
        string rncEmisor, string encf, string accessToken, CancellationToken cancellationToken)
    {
        var result = await _transport.GetJsonAsync<List<TrackIdSummary>>(
            Ecf(DgiiEndpoints.TrackIds),
            new Dictionary<string, string?> { ["rncEmisor"] = rncEmisor, ["encf"] = encf },
            AuthorizationHeader.Bearer(accessToken),
            cancellationToken);

        return result.IsSuccess ? result.Value : result.Error;
    }

    public async Task<Result<IReadOnlyList<CustomerDirectoryEntry>>> GetCustomerDirectoryAsync(
        string rnc, string accessToken, CancellationToken cancellationToken)
    {
        var uri = Ecf(DgiiEndpoints.Directory(_options.Environment));
        var body = await _transport.GetStringAsync(
            uri,
            new Dictionary<string, string?> { ["rnc"] = rnc },
            AuthorizationHeader.Bearer(accessToken),
            cancellationToken);
        if (body.IsFailure)
        {
            return body.Error;
        }

        return NormalizeDirectory(body.Value, uri);
    }

    public Task<Result<SummaryInvoiceInquiryResponse>> GetSummaryInvoiceInquiryAsync(
        string rncEmisor, string encf, string securityCode, string accessToken, CancellationToken cancellationToken) =>
        _transport.GetJsonAsync<SummaryInvoiceInquiryResponse>(
            DgiiEndpoints.Dgii(_options.FcBaseUrl, _options.Environment, DgiiEndpoints.SummaryInvoiceInquiry),
            new Dictionary<string, string?>
            {
                ["rnc_emisor"] = rncEmisor,
                ["encf"] = encf,
                ["cod_seguridad_eCF"] = securityCode,
            },
            AuthorizationHeader.Bearer(accessToken),
            cancellationToken);

    /// <summary>
    /// DGII answers the directory with an array in some environments and a single object in others.
    /// </summary>
    internal static Result<IReadOnlyList<CustomerDirectoryEntry>> NormalizeDirectory(string body, Uri uri)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return Array.Empty<CustomerDirectoryEntry>();
        }

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(body);
        }
        catch (JsonException)
        {
            return DgiiHttpTransport.Deserialize<List<CustomerDirectoryEntry>>(body, uri).Error;
        }

        return node switch
        {
            null => Array.Empty<CustomerDirectoryEntry>(),
            JsonArray array => array.Deserialize<List<CustomerDirectoryEntry>>(DgiiHttpTransport.JsonOptions)!,
            _ => new[] { node.Deserialize<CustomerDirectoryEntry>(DgiiHttpTransport.JsonOptions)! },
        };
    }

    private Uri Ecf(string resource) => DgiiEndpoints.Dgii(_options.EcfBaseUrl, _options.Environment, resource);
}
