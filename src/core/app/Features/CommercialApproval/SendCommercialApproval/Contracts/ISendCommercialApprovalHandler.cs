using DgiiEcf.Application.Common.Responses.CommercialApprovalResponse;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.CommercialApproval.SendCommercialApproval.Contracts;

public interface ISendCommercialApprovalHandler
{
    Task<Result<CommercialApprovalResponse>> HandleAsync(SendCommercialApprovalCommand command, CancellationToken cancellationToken = default);
}
