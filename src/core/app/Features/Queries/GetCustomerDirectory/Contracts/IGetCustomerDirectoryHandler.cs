using DgiiEcf.Application.Common.Responses.CustomerDirectoryEntry;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Queries.GetCustomerDirectory.Contracts;

public interface IGetCustomerDirectoryHandler
{
    Task<Result<IReadOnlyList<CustomerDirectoryEntry>>> HandleAsync(GetCustomerDirectoryQuery query, CancellationToken cancellationToken = default);
}
