using DgiiEcf.Application.Common.Responses.TrackingStatusResponse;
using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Queries.GetTrackStatus.Contracts;

public interface IGetTrackStatusHandler
{
    Task<Result<TrackingStatusResponse>> HandleAsync(GetTrackStatusQuery query, CancellationToken cancellationToken = default);
}
