using DgiiEcf.Application.Common.Responses.ServiceStatus;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.ServiceStatus.GetMaintenanceWindows.Contracts;

public interface IGetMaintenanceWindowsHandler
{
    Task<Result<MaintenanceResponse>> HandleAsync(GetMaintenanceWindowsQuery query, CancellationToken cancellationToken = default);
}
