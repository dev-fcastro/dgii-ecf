using DgiiEcf.Application.Common.Responses.InvoiceSummaryResponse;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Reception.SendSummary.Contracts;

public interface ISendSummaryHandler
{
    Task<Result<InvoiceSummaryResponse>> HandleAsync(SendSummaryCommand command, CancellationToken cancellationToken = default);
}
