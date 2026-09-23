using DgiiEcf.Application.Common.Responses.ServiceStatus;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Common.Contracts.IDgiiServiceStatusClient;

/// <summary>
/// DGII service status API (<c>statusecf.dgii.gov.do</c>). Requires the API key issued by DGII.
/// </summary>
public interface IDgiiServiceStatusClient
{
    Task<Result<IReadOnlyList<DgiiServiceStatus>>> GetServicesStatusAsync(string apiKey, CancellationToken cancellationToken);

    Task<Result<MaintenanceResponse>> GetMaintenanceWindowsAsync(string apiKey, CancellationToken cancellationToken);

    Task<Result<VerificationResponse>> VerifyStatusAsync(string apiKey, CancellationToken cancellationToken);
}
