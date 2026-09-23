using DgiiEcf.Application.Common.Responses.SummaryInvoiceInquiryResponse;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Queries.GetSummaryInvoiceInquiry.Contracts;

public interface IGetSummaryInvoiceInquiryHandler
{
    Task<Result<SummaryInvoiceInquiryResponse>> HandleAsync(GetSummaryInvoiceInquiryQuery query, CancellationToken cancellationToken = default);
}
