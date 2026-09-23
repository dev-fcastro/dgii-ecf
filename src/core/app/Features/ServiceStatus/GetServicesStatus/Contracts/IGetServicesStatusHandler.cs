using DgiiEcf.Application.Common.Responses.ServiceStatus;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.ServiceStatus.GetServicesStatus.Contracts;

public interface IGetServicesStatusHandler
{
    Task<Result<IReadOnlyList<ServiceStatus>>> HandleAsync(GetServicesStatusQuery query, CancellationToken cancellationToken = default);
}
