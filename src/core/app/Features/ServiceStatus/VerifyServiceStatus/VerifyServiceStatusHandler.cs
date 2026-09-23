using DgiiEcf.Application.Common.Contracts.IDgiiServiceStatusClient;
using DgiiEcf.Application.Common.Responses.ServiceStatus;
using DgiiEcf.Application.Features.Queries.QueryErrors;
using DgiiEcf.Application.Features.ServiceStatus.VerifyServiceStatus.Contracts;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.ServiceStatus.VerifyServiceStatus;

public sealed class VerifyServiceStatusHandler : IVerifyServiceStatusHandler
{
    private readonly IDgiiServiceStatusClient _client;

    public VerifyServiceStatusHandler(IDgiiServiceStatusClient client)
    {
        _client = client;
    }

    public async Task<Result<VerificationResponse>> HandleAsync(VerifyServiceStatusQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.ApiKey))
        {
            return QueryErrors.Required(nameof(query.ApiKey));
        }

        return await _client.VerifyStatusAsync(query.ApiKey, cancellationToken);
    }
}
