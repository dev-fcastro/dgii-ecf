using DgiiEcf.Application.Common.Contracts.IDgiiServiceStatusClient;
using DgiiEcf.Application.Common.Responses.ServiceStatus;
using DgiiEcf.Application.Features.Queries.QueryErrors;
using DgiiEcf.Application.Features.ServiceStatus.GetServicesStatus.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.ServiceStatus.GetServicesStatus;

public sealed class GetServicesStatusHandler : IGetServicesStatusHandler
{
    private readonly IDgiiServiceStatusClient _client;

    public GetServicesStatusHandler(IDgiiServiceStatusClient client)
    {
        _client = client;
    }

    public async Task<Result<IReadOnlyList<ServiceStatus>>> HandleAsync(GetServicesStatusQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.ApiKey))
        {
            return QueryErrors.Required(nameof(query.ApiKey));
        }

        return await _client.GetServicesStatusAsync(query.ApiKey, cancellationToken);
    }
}
