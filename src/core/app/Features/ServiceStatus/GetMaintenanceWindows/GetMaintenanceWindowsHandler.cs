using DgiiEcf.Application.Common.Contracts.IDgiiServiceStatusClient;
using DgiiEcf.Application.Common.Responses.ServiceStatus;
using DgiiEcf.Application.Features.Queries.QueryErrors;
using DgiiEcf.Application.Features.ServiceStatus.GetMaintenanceWindows.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.ServiceStatus.GetMaintenanceWindows;

public sealed class GetMaintenanceWindowsHandler : IGetMaintenanceWindowsHandler
{
    private readonly IDgiiServiceStatusClient _client;

    public GetMaintenanceWindowsHandler(IDgiiServiceStatusClient client)
    {
        _client = client;
    }

    public async Task<Result<MaintenanceResponse>> HandleAsync(GetMaintenanceWindowsQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.ApiKey))
        {
            return QueryErrors.Required(nameof(query.ApiKey));
        }

        return await _client.GetMaintenanceWindowsAsync(query.ApiKey, cancellationToken);
    }
}
