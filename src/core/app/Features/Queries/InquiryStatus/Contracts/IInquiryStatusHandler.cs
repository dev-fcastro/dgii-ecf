using DgiiEcf.Application.Common.Responses.InquiryStatusResponse;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Queries.InquiryStatus.Contracts;

public interface IInquiryStatusHandler
{
    Task<Result<InquiryStatusResponse>> HandleAsync(InquiryStatusQuery query, CancellationToken cancellationToken = default);
}
