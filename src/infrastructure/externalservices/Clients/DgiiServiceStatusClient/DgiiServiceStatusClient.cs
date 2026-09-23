using System.Globalization;
using DgiiEcf.Application.Common.Contracts.IDgiiServiceStatusClient;
using DgiiEcf.Application.Common.Responses.ServiceStatus;
using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.ExternalServices.Http.DgiiEndpoints;
using DgiiEcf.ExternalServices.Http.DgiiHttpTransport;
using DgiiEcf.ExternalServices.Options.DgiiApiOptions;
using Microsoft.Extensions.Options;

namespace DgiiEcf.ExternalServices.Clients.DgiiServiceStatusClient;

/// <summary>
/// Service status API. The API key goes as the raw <c>Authorization</c> header (no Bearer prefix).
/// </summary>
public sealed class DgiiServiceStatusClient : IDgiiServiceStatusClient
{
    private readonly DgiiHttpTransport _transport;
    private readonly DgiiApiOptions _options;

    public DgiiServiceStatusClient(DgiiHttpTransport transport, IOptions<DgiiApiOptions> options)
    {
        _transport = transport;
        _options = options.Value;
    }

    public async Task<Result<IReadOnlyList<DgiiServiceStatus>>> GetServicesStatusAsync(string apiKey, CancellationToken cancellationToken)
    {
        var result = await _transport.GetJsonAsync<List<DgiiServiceStatus>>(
            DgiiEndpoints.Status(_options.StatusBaseUrl, DgiiEndpoints.ServiceStatus),
            null,
            AuthorizationHeader.ApiKey(apiKey),
            cancellationToken);

        return result.IsSuccess ? result.Value : result.Error;
    }

    public Task<Result<MaintenanceResponse>> GetMaintenanceWindowsAsync(string apiKey, CancellationToken cancellationToken) =>
        _transport.GetJsonAsync<MaintenanceResponse>(
            DgiiEndpoints.Status(_options.StatusBaseUrl, DgiiEndpoints.ServiceMaintenance),
            null,
            AuthorizationHeader.ApiKey(apiKey),
            cancellationToken);

    public Task<Result<VerificationResponse>> VerifyStatusAsync(string apiKey, CancellationToken cancellationToken) =>
        _transport.GetJsonAsync<VerificationResponse>(
            DgiiEndpoints.Status(_options.StatusBaseUrl, DgiiEndpoints.ServiceVerification),
            new Dictionary<string, string?>
            {
                ["ambiente"] = _options.Environment.ToStatusCode().ToString(CultureInfo.InvariantCulture),
            },
            AuthorizationHeader.ApiKey(apiKey),
            cancellationToken);
}
