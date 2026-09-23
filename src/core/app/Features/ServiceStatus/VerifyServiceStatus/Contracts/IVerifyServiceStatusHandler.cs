using DgiiEcf.Application.Common.Responses.ServiceStatus;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.ServiceStatus.VerifyServiceStatus.Contracts;

public interface IVerifyServiceStatusHandler
{
    Task<Result<VerificationResponse>> HandleAsync(VerifyServiceStatusQuery query, CancellationToken cancellationToken = default);
}
